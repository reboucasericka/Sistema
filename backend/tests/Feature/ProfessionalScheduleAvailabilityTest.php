<?php

namespace Tests\Feature;

use App\Enums\DayOfWeek;
use App\Enums\UserRole;
use App\Models\Appointment;
use App\Models\Client;
use App\Models\Professional;
use App\Models\ProfessionalSchedule;
use App\Models\Service;
use App\Models\User;
use App\Services\AppointmentService;
use Carbon\Carbon;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Hash;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class ProfessionalScheduleAvailabilityTest extends TestCase
{
    use RefreshDatabase;

    private Service $service;

    private Professional $professional;

    private User $admin;

    private AppointmentService $appointmentService;

    protected function setUp(): void
    {
        parent::setUp();

        Carbon::setTestNow(Carbon::parse('2026-08-10 08:00:00')); // Monday

        $this->appointmentService = app(AppointmentService::class);

        $this->admin = User::create([
            'first_name' => 'Admin',
            'last_name' => 'Avail',
            'email' => 'admin-avail@test.com',
            'role' => UserRole::Admin,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $proUser = User::create([
            'first_name' => 'Ana',
            'last_name' => 'Avail',
            'email' => 'ana-avail@test.com',
            'role' => UserRole::Professional,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->professional = Professional::create([
            'user_id' => $proUser->id,
            'name' => 'Ana Avail',
            'specialty' => 'Cabelo',
            'is_active' => true,
        ]);

        $this->service = Service::create([
            'name' => 'Corte',
            'price' => 25,
            'duration_minutes' => 60,
            'is_active' => true,
        ]);

        $this->professional->services()->attach($this->service->id);
    }

    protected function tearDown(): void
    {
        Carbon::setTestNow();
        parent::tearDown();
    }

    public function test_availability_returns_empty_without_schedules(): void
    {
        $date = Carbon::parse('2026-08-10'); // Monday

        $slots = $this->appointmentService->getAvailableSlots(
            $this->professional->id,
            $this->service,
            $date
        );

        $this->assertSame([], $slots);

        $this->getJson('/api/v1/public/availability?'.http_build_query([
            'professional_id' => $this->professional->id,
            'service_id' => $this->service->id,
            'date' => $date->toDateString(),
        ]))->assertOk()
            ->assertJsonPath('data.slots', []);
    }

    public function test_slots_respect_windows_and_skip_lunch_break(): void
    {
        $this->createWeekdayWindows();

        $date = Carbon::parse('2026-08-10'); // Monday
        $slots = $this->appointmentService->getAvailableSlots(
            $this->professional->id,
            $this->service,
            $date
        );

        $this->assertContains('09:00', $slots);
        $this->assertContains('11:00', $slots);
        $this->assertNotContains('11:30', $slots); // 11:30+60 > 12:00
        $this->assertNotContains('12:00', $slots);
        $this->assertNotContains('12:30', $slots);
        $this->assertNotContains('13:00', $slots);
        $this->assertNotContains('13:30', $slots);
        $this->assertContains('14:00', $slots);
        $this->assertContains('17:00', $slots);
        $this->assertNotContains('17:30', $slots);
        $this->assertSame($slots, array_values(array_unique($slots)));
        $sorted = $slots;
        sort($sorted);
        $this->assertSame($sorted, $slots);
    }

    public function test_existing_appointment_blocks_slot_but_canceled_does_not(): void
    {
        $this->createWeekdayWindows();

        $client = Client::create([
            'name' => 'Cliente',
            'email' => 'cliente-avail@test.com',
            'is_active' => true,
        ]);

        $date = Carbon::parse('2026-08-10');
        $start = $date->copy()->setTime(10, 0);
        $end = $start->copy()->addMinutes(60);

        Appointment::create([
            'client_id' => $client->id,
            'service_id' => $this->service->id,
            'professional_id' => $this->professional->id,
            'start_time' => $start,
            'end_time' => $end,
            'status' => Appointment::STATUS_PENDING,
            'total_price' => 25,
            'is_active' => true,
        ]);

        $slots = $this->appointmentService->getAvailableSlots(
            $this->professional->id,
            $this->service,
            $date
        );

        $this->assertNotContains('10:00', $slots);
        $this->assertNotContains('09:30', $slots); // overlaps 10:00-11:00

        Appointment::query()->update([
            'status' => Appointment::STATUS_CANCELED,
            'is_active' => false,
        ]);

        $slotsAfterCancel = $this->appointmentService->getAvailableSlots(
            $this->professional->id,
            $this->service,
            $date
        );

        $this->assertContains('10:00', $slotsAfterCancel);
    }

    public function test_sunday_uses_day_of_week_zero(): void
    {
        ProfessionalSchedule::factory()
            ->forProfessional($this->professional)
            ->onDay(DayOfWeek::Sunday)
            ->window('10:00', '14:00')
            ->create();

        $sunday = Carbon::parse('2026-08-16'); // future Sunday
        $this->assertSame(0, $sunday->dayOfWeek);

        $slots = $this->appointmentService->getAvailableSlots(
            $this->professional->id,
            $this->service,
            $sunday
        );

        $this->assertContains('10:00', $slots);
        $this->assertContains('13:00', $slots);
        $this->assertNotContains('13:30', $slots);
    }

    public function test_past_slots_are_excluded(): void
    {
        Carbon::setTestNow(Carbon::parse('2026-08-10 10:15:00'));
        $this->createWeekdayWindows();

        $slots = $this->appointmentService->getAvailableSlots(
            $this->professional->id,
            $this->service,
            Carbon::parse('2026-08-10')
        );

        $this->assertNotContains('09:00', $slots);
        $this->assertNotContains('10:00', $slots);
        $this->assertContains('10:30', $slots);
    }

    public function test_public_and_authenticated_appointment_within_and_outside_window(): void
    {
        $this->createWeekdayWindows();
        $date = Carbon::parse('2026-08-11'); // Tuesday

        $this->postJson('/api/v1/public/appointments', [
            'service_id' => $this->service->id,
            'professional_id' => $this->professional->id,
            'date' => $date->toDateString(),
            'time' => '10:00',
            'client_name' => 'Guest',
            'client_email' => 'guest-avail@test.com',
        ])->assertCreated();

        $this->postJson('/api/v1/public/appointments', [
            'service_id' => $this->service->id,
            'professional_id' => $this->professional->id,
            'date' => $date->toDateString(),
            'time' => '12:30',
            'client_name' => 'Guest2',
            'client_email' => 'guest2-avail@test.com',
        ])->assertUnprocessable()
            ->assertJsonValidationErrors(['start_time']);

        $this->postJson('/api/v1/public/appointments', [
            'service_id' => $this->service->id,
            'professional_id' => $this->professional->id,
            'date' => '2026-08-16', // Sunday — no schedule
            'time' => '10:00',
            'client_name' => 'Guest3',
            'client_email' => 'guest3-avail@test.com',
        ])->assertUnprocessable()
            ->assertJsonValidationErrors(['start_time']);

        $clientUser = User::create([
            'first_name' => 'Maria',
            'last_name' => 'Auth',
            'email' => 'maria-auth@test.com',
            'role' => UserRole::Client,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $client = Client::create([
            'user_id' => $clientUser->id,
            'name' => 'Maria Auth',
            'email' => 'maria-auth@test.com',
            'is_active' => true,
        ]);

        Sanctum::actingAs($this->admin);

        $this->postJson('/api/v1/appointments', [
            'client_id' => $client->id,
            'service_id' => $this->service->id,
            'professional_id' => $this->professional->id,
            'start_time' => $date->copy()->setTime(14, 0)->toIso8601String(),
        ])->assertCreated();

        $this->postJson('/api/v1/appointments', [
            'client_id' => $client->id,
            'service_id' => $this->service->id,
            'professional_id' => $this->professional->id,
            'start_time' => $date->copy()->setTime(11, 30)->toIso8601String(),
        ])->assertUnprocessable()
            ->assertJsonValidationErrors(['start_time']);
    }

    public function test_update_appointment_rejects_outside_window(): void
    {
        $this->createWeekdayWindows();
        $date = Carbon::parse('2026-08-11');

        $client = Client::create([
            'name' => 'Update Client',
            'email' => 'update-client@test.com',
            'is_active' => true,
        ]);

        $appointment = Appointment::create([
            'client_id' => $client->id,
            'service_id' => $this->service->id,
            'professional_id' => $this->professional->id,
            'start_time' => $date->copy()->setTime(10, 0),
            'end_time' => $date->copy()->setTime(11, 0),
            'status' => Appointment::STATUS_PENDING,
            'total_price' => 25,
            'is_active' => true,
        ]);

        Sanctum::actingAs($this->admin);

        $this->putJson("/api/v1/appointments/{$appointment->id}", [
            'start_time' => $date->copy()->setTime(12, 30)->toIso8601String(),
        ])->assertUnprocessable()
            ->assertJsonValidationErrors(['start_time']);
    }

    private function createWeekdayWindows(): void
    {
        foreach ([DayOfWeek::Monday, DayOfWeek::Tuesday, DayOfWeek::Wednesday, DayOfWeek::Thursday, DayOfWeek::Friday] as $day) {
            ProfessionalSchedule::factory()
                ->forProfessional($this->professional)
                ->onDay($day)
                ->window('09:00', '12:00')
                ->createdBy($this->admin)
                ->create();

            ProfessionalSchedule::factory()
                ->forProfessional($this->professional)
                ->onDay($day)
                ->window('14:00', '18:00')
                ->createdBy($this->admin)
                ->create();
        }
    }
}

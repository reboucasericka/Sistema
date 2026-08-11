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
use Carbon\Carbon;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Hash;
use Tests\TestCase;

class PublicApiTest extends TestCase
{
    use RefreshDatabase;

    private Service $service;

    private Professional $professional;

    protected function setUp(): void
    {
        parent::setUp();

        Carbon::setTestNow(Carbon::parse('2026-08-10 08:00:00')); // Monday

        $this->service = Service::create([
            'name' => 'Corte',
            'description' => 'Corte de cabelo',
            'price' => 25,
            'duration_minutes' => 60,
            'is_active' => true,
        ]);

        $professionalUser = User::create([
            'first_name' => 'Ana',
            'last_name' => 'Profissional',
            'email' => 'ana@salon.test',
            'role' => UserRole::Professional,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->professional = Professional::create([
            'user_id' => $professionalUser->id,
            'name' => 'Ana Profissional',
            'specialty' => 'Cabeleireira',
            'email' => 'ana@salon.test',
            'is_active' => true,
        ]);

        $this->professional->services()->attach($this->service->id);

        foreach ([
            DayOfWeek::Monday,
            DayOfWeek::Tuesday,
            DayOfWeek::Wednesday,
            DayOfWeek::Thursday,
            DayOfWeek::Friday,
            DayOfWeek::Saturday,
        ] as $day) {
            ProfessionalSchedule::factory()
                ->forProfessional($this->professional)
                ->onDay($day)
                ->window('09:00', '18:00')
                ->create();
        }
    }

    protected function tearDown(): void
    {
        Carbon::setTestNow();
        parent::tearDown();
    }

    public function test_public_can_list_active_services(): void
    {
        $response = $this->getJson('/api/v1/public/services');

        $response->assertOk()
            ->assertJsonPath('data.0.name', 'Corte');
    }

    public function test_client_registration_always_creates_client_role(): void
    {
        $response = $this->postJson('/api/v1/auth/register', [
            'first_name' => 'Maria',
            'last_name' => 'Silva',
            'email' => 'maria@example.com',
            'phone' => '910000000',
            'password' => 'password123',
            'password_confirmation' => 'password123',
        ]);

        $response->assertCreated()
            ->assertJsonPath('user.role', 'client');

        $userId = $response->json('user.id');

        $this->assertDatabaseHas('users', [
            'id' => $userId,
            'email' => 'maria@example.com',
            'role' => UserRole::Client->value,
        ]);

        $this->assertDatabaseHas('clients', [
            'email' => 'maria@example.com',
            'user_id' => $userId,
        ]);
    }

    public function test_public_can_list_active_professionals(): void
    {
        $response = $this->getJson('/api/v1/public/professionals');

        $response->assertOk()
            ->assertJsonFragment(['name' => 'Ana Profissional'])
            ->assertJsonMissingPath('data.0.commission_percentage');
    }

    public function test_public_can_create_appointment_as_guest(): void
    {
        $date = Carbon::tomorrow()->toDateString();

        $response = $this->postJson('/api/v1/public/appointments', [
            'service_id' => $this->service->id,
            'professional_id' => $this->professional->id,
            'date' => $date,
            'time' => '10:00',
            'client_name' => 'João Guest',
            'client_email' => 'joao@example.com',
            'client_phone' => '912345678',
            'notes' => 'Primeira visita',
        ]);

        $response->assertCreated()
            ->assertJsonPath('data.service_id', $this->service->id);

        $this->assertDatabaseHas('appointments', [
            'professional_id' => $this->professional->id,
            'status' => Appointment::STATUS_PENDING,
        ]);

        $this->assertDatabaseHas('clients', [
            'email' => 'joao@example.com',
            'name' => 'João Guest',
        ]);
    }

    public function test_public_booking_rejects_schedule_conflict(): void
    {
        $client = Client::create([
            'name' => 'Cliente Teste',
            'email' => 'cliente@test.com',
            'is_active' => true,
        ]);

        $date = Carbon::tomorrow();
        $start = $date->copy()->setTime(10, 0);
        $end = $start->copy()->addMinutes(60);

        Appointment::create([
            'client_id' => $client->id,
            'service_id' => $this->service->id,
            'professional_id' => $this->professional->id,
            'start_time' => $start,
            'end_time' => $end,
            'status' => Appointment::STATUS_PENDING,
            'total_price' => $this->service->price,
            'is_active' => true,
        ]);

        $response = $this->postJson('/api/v1/public/appointments', [
            'service_id' => $this->service->id,
            'professional_id' => $this->professional->id,
            'date' => $date->toDateString(),
            'time' => '10:00',
            'client_name' => 'Outro Cliente',
            'client_email' => 'outro@example.com',
        ]);

        $response->assertUnprocessable()
            ->assertJsonValidationErrors(['start_time']);
    }

    public function test_public_can_submit_contact_message(): void
    {
        $response = $this->postJson('/api/v1/public/contact', [
            'name' => 'Pedro',
            'email' => 'pedro@example.com',
            'phone' => '910000001',
            'subject' => 'Dúvida',
            'message' => 'Gostaria de saber mais sobre os serviços.',
        ]);

        $response->assertCreated();

        $this->assertDatabaseHas('contact_messages', [
            'email' => 'pedro@example.com',
            'is_read' => false,
        ]);
    }
}

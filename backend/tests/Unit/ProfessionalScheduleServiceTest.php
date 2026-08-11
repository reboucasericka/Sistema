<?php

namespace Tests\Unit;

use App\Enums\DayOfWeek;
use App\Enums\UserRole;
use App\Models\Professional;
use App\Models\ProfessionalSchedule;
use App\Models\User;
use App\Services\ProfessionalScheduleService;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Hash;
use Illuminate\Validation\ValidationException;
use Tests\TestCase;

class ProfessionalScheduleServiceTest extends TestCase
{
    use RefreshDatabase;

    private ProfessionalScheduleService $service;

    private Professional $professional;

    private User $admin;

    protected function setUp(): void
    {
        parent::setUp();

        $this->service = app(ProfessionalScheduleService::class);

        $this->admin = User::create([
            'first_name' => 'Admin',
            'last_name' => 'Test',
            'email' => 'admin-sched@test.com',
            'role' => UserRole::Admin,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $proUser = User::create([
            'first_name' => 'Pro',
            'last_name' => 'Test',
            'email' => 'pro-sched@test.com',
            'role' => UserRole::Professional,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->professional = Professional::create([
            'user_id' => $proUser->id,
            'name' => 'Pro Test',
            'specialty' => 'Geral',
            'is_active' => true,
        ]);
    }

    public function test_create_without_overlap(): void
    {
        $schedule = $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Monday->value,
            'start_time' => '09:00',
            'end_time' => '12:00',
        ], $this->admin);

        $this->assertDatabaseHas('professional_schedules', [
            'id' => $schedule->id,
            'created_by' => $this->admin->id,
        ]);
    }

    public function test_adjacent_windows_are_allowed(): void
    {
        $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Monday->value,
            'start_time' => '09:00',
            'end_time' => '12:00',
        ], $this->admin);

        $second = $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Monday->value,
            'start_time' => '12:00',
            'end_time' => '18:00',
        ], $this->admin);

        $this->assertNotNull($second->id);
    }

    public function test_multiple_non_overlapping_windows(): void
    {
        $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Tuesday->value,
            'start_time' => '09:00',
            'end_time' => '12:00',
        ], $this->admin);

        $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Tuesday->value,
            'start_time' => '14:00',
            'end_time' => '18:00',
        ], $this->admin);

        $this->assertSame(2, ProfessionalSchedule::query()->where('professional_id', $this->professional->id)->count());
    }

    public function test_partial_overlap_is_rejected(): void
    {
        $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Wednesday->value,
            'start_time' => '09:00',
            'end_time' => '13:00',
        ], $this->admin);

        $this->expectException(ValidationException::class);

        $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Wednesday->value,
            'start_time' => '12:00',
            'end_time' => '18:00',
        ], $this->admin);
    }

    public function test_contained_window_is_rejected(): void
    {
        $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Thursday->value,
            'start_time' => '09:00',
            'end_time' => '18:00',
        ], $this->admin);

        $this->expectException(ValidationException::class);

        $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Thursday->value,
            'start_time' => '10:00',
            'end_time' => '12:00',
        ], $this->admin);
    }

    public function test_containing_window_is_rejected(): void
    {
        $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Friday->value,
            'start_time' => '10:00',
            'end_time' => '12:00',
        ], $this->admin);

        $this->expectException(ValidationException::class);

        $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Friday->value,
            'start_time' => '09:00',
            'end_time' => '18:00',
        ], $this->admin);
    }

    public function test_identical_windows_are_rejected(): void
    {
        $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Saturday->value,
            'start_time' => '09:00',
            'end_time' => '13:00',
        ], $this->admin);

        $this->expectException(ValidationException::class);

        $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Saturday->value,
            'start_time' => '09:00',
            'end_time' => '13:00',
        ], $this->admin);
    }

    public function test_update_ignores_self_in_overlap_check(): void
    {
        $schedule = $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Monday->value,
            'start_time' => '09:00',
            'end_time' => '12:00',
        ], $this->admin);

        $updated = $this->service->update($schedule, [
            'start_time' => '09:30',
            'end_time' => '12:30',
        ]);

        $this->assertSame('09:30', $updated->startTimeHi());
        $this->assertSame('12:30', $updated->endTimeHi());
    }

    public function test_update_rejects_overlap_with_other_record(): void
    {
        $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Monday->value,
            'start_time' => '09:00',
            'end_time' => '12:00',
        ], $this->admin);

        $second = $this->service->create([
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Monday->value,
            'start_time' => '14:00',
            'end_time' => '18:00',
        ], $this->admin);

        $this->expectException(ValidationException::class);

        $this->service->update($second, [
            'start_time' => '11:00',
            'end_time' => '15:00',
        ]);
    }
}

<?php

namespace Tests\Feature;

use App\Enums\DayOfWeek;
use App\Enums\UserRole;
use App\Models\Professional;
use App\Models\ProfessionalSchedule;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Hash;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class ProfessionalScheduleCrudTest extends TestCase
{
    use RefreshDatabase;

    private User $admin;

    private User $professionalUser;

    private User $otherProfessionalUser;

    private User $clientUser;

    private Professional $professional;

    private Professional $otherProfessional;

    protected function setUp(): void
    {
        parent::setUp();

        $this->admin = User::create([
            'first_name' => 'Admin',
            'last_name' => 'User',
            'email' => 'admin@sched.test',
            'role' => UserRole::Admin,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->professionalUser = User::create([
            'first_name' => 'Ana',
            'last_name' => 'Pro',
            'email' => 'ana@sched.test',
            'role' => UserRole::Professional,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->otherProfessionalUser = User::create([
            'first_name' => 'Bruno',
            'last_name' => 'Pro',
            'email' => 'bruno@sched.test',
            'role' => UserRole::Professional,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->clientUser = User::create([
            'first_name' => 'Client',
            'last_name' => 'User',
            'email' => 'client@sched.test',
            'role' => UserRole::Client,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->professional = Professional::create([
            'user_id' => $this->professionalUser->id,
            'name' => 'Ana Pro',
            'specialty' => 'Cabelo',
            'is_active' => true,
        ]);

        $this->otherProfessional = Professional::create([
            'user_id' => $this->otherProfessionalUser->id,
            'name' => 'Bruno Pro',
            'specialty' => 'Unhas',
            'is_active' => true,
        ]);
    }

    public function test_admin_can_list_create_show_update_and_delete(): void
    {
        Sanctum::actingAs($this->admin);

        $rejected = $this->postJson('/api/v1/professional-schedules', [
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Monday->value,
            'start_time' => '09:00',
            'end_time' => '12:00',
            'created_by' => $this->clientUser->id,
        ]);

        $rejected->assertUnprocessable()
            ->assertJsonValidationErrors(['created_by']);

        $create = $this->postJson('/api/v1/professional-schedules', [
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Monday->value,
            'start_time' => '09:00',
            'end_time' => '12:00',
        ]);

        $create->assertCreated()
            ->assertJsonPath('data.day_name', 'Segunda-feira')
            ->assertJsonPath('data.start_time', '09:00')
            ->assertJsonPath('data.created_by', $this->admin->id);

        $id = $create->json('data.id');

        $this->getJson('/api/v1/professional-schedules')
            ->assertOk()
            ->assertJsonPath('data.0.id', $id);

        $this->getJson("/api/v1/professional-schedules/{$id}")
            ->assertOk()
            ->assertJsonPath('data.id', $id);

        $this->putJson("/api/v1/professional-schedules/{$id}", [
            'start_time' => '09:30',
            'end_time' => '12:30',
        ])->assertOk()
            ->assertJsonPath('data.start_time', '09:30');

        $this->deleteJson("/api/v1/professional-schedules/{$id}")
            ->assertOk();

        $this->assertDatabaseMissing('professional_schedules', ['id' => $id]);
    }

    public function test_professional_lists_and_views_only_own_schedules(): void
    {
        $own = ProfessionalSchedule::factory()
            ->forProfessional($this->professional)
            ->onDay(DayOfWeek::Monday)
            ->window('09:00', '12:00')
            ->createdBy($this->admin)
            ->create();

        $other = ProfessionalSchedule::factory()
            ->forProfessional($this->otherProfessional)
            ->onDay(DayOfWeek::Monday)
            ->window('09:00', '12:00')
            ->createdBy($this->admin)
            ->create();

        Sanctum::actingAs($this->professionalUser);

        $this->getJson('/api/v1/professional-schedules')
            ->assertOk()
            ->assertJsonCount(1, 'data')
            ->assertJsonPath('data.0.id', $own->id);

        $this->getJson("/api/v1/professional-schedules/{$own->id}")
            ->assertOk();

        $this->getJson("/api/v1/professional-schedules/{$other->id}")
            ->assertForbidden();
    }

    public function test_professional_cannot_mutate_schedules(): void
    {
        $schedule = ProfessionalSchedule::factory()
            ->forProfessional($this->professional)
            ->onDay(DayOfWeek::Tuesday)
            ->window('09:00', '12:00')
            ->create();

        Sanctum::actingAs($this->professionalUser);

        $this->postJson('/api/v1/professional-schedules', [
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Wednesday->value,
            'start_time' => '09:00',
            'end_time' => '12:00',
        ])->assertForbidden();

        $this->putJson("/api/v1/professional-schedules/{$schedule->id}", [
            'start_time' => '10:00',
            'end_time' => '13:00',
        ])->assertForbidden();

        $this->deleteJson("/api/v1/professional-schedules/{$schedule->id}")
            ->assertForbidden();
    }

    public function test_client_cannot_access_schedules(): void
    {
        Sanctum::actingAs($this->clientUser);

        $this->getJson('/api/v1/professional-schedules')->assertForbidden();
    }

    public function test_inactive_professional_is_rejected(): void
    {
        $this->professional->update(['is_active' => false]);

        Sanctum::actingAs($this->admin);

        $this->postJson('/api/v1/professional-schedules', [
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Monday->value,
            'start_time' => '09:00',
            'end_time' => '12:00',
        ])->assertUnprocessable()
            ->assertJsonValidationErrors(['professional_id']);
    }

    public function test_end_time_must_be_after_start_time(): void
    {
        Sanctum::actingAs($this->admin);

        $this->postJson('/api/v1/professional-schedules', [
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Monday->value,
            'start_time' => '12:00',
            'end_time' => '09:00',
        ])->assertUnprocessable()
            ->assertJsonValidationErrors(['end_time']);
    }

    public function test_invalid_day_of_week_is_rejected(): void
    {
        Sanctum::actingAs($this->admin);

        $this->postJson('/api/v1/professional-schedules', [
            'professional_id' => $this->professional->id,
            'day_of_week' => 7,
            'start_time' => '09:00',
            'end_time' => '12:00',
        ])->assertUnprocessable()
            ->assertJsonValidationErrors(['day_of_week']);
    }

    public function test_overlap_returns_422(): void
    {
        Sanctum::actingAs($this->admin);

        $this->postJson('/api/v1/professional-schedules', [
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Monday->value,
            'start_time' => '09:00',
            'end_time' => '13:00',
        ])->assertCreated();

        $this->postJson('/api/v1/professional-schedules', [
            'professional_id' => $this->professional->id,
            'day_of_week' => DayOfWeek::Monday->value,
            'start_time' => '12:00',
            'end_time' => '18:00',
        ])->assertUnprocessable()
            ->assertJsonValidationErrors(['start_time']);
    }
}

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
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class ProfessionalCrudTest extends TestCase
{
    use RefreshDatabase;

    private User $admin;

    private User $clientUser;

    private Service $service;

    protected function setUp(): void
    {
        parent::setUp();

        Carbon::setTestNow(Carbon::parse('2026-08-10 08:00:00'));

        $this->admin = User::create([
            'first_name' => 'Admin',
            'last_name' => 'Pro',
            'email' => 'admin-pro@test.com',
            'role' => UserRole::Admin,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->clientUser = User::create([
            'first_name' => 'Client',
            'last_name' => 'Pro',
            'email' => 'client-pro@test.com',
            'role' => UserRole::Client,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->service = Service::create([
            'name' => 'Corte',
            'price' => 25,
            'duration_minutes' => 60,
            'is_active' => true,
        ]);
    }

    protected function tearDown(): void
    {
        Carbon::setTestNow();
        parent::tearDown();
    }

    public function test_admin_can_create_professional_with_nullable_fields_and_list_it(): void
    {
        Sanctum::actingAs($this->admin);

        $create = $this->postJson('/api/v1/professionals', [
            'name' => 'Carla Mendes',
            'specialty' => 'Esteticista',
            'email' => null,
            'phone' => null,
            'biography' => 'Especialista em pele.',
            'years_experience' => 5,
            'instagram' => null,
            'facebook' => null,
            'photo' => null,
            'commission_percentage' => 15,
            'is_active' => true,
        ]);

        $create->assertCreated()
            ->assertJsonPath('data.name', 'Carla Mendes')
            ->assertJsonPath('data.user_id', null)
            ->assertJsonPath('data.is_active', true)
            ->assertJsonPath('data.email', null)
            ->assertJsonPath('data.years_experience', 5)
            ->assertJsonPath('data.biography', 'Especialista em pele.');

        $id = $create->json('data.id');

        $this->assertDatabaseHas('professionals', [
            'id' => $id,
            'name' => 'Carla Mendes',
            'specialty' => 'Esteticista',
            'user_id' => null,
            'is_active' => 1,
            'years_experience' => 5,
        ]);

        $this->getJson('/api/v1/professionals?search=Carla')
            ->assertOk()
            ->assertJsonPath('data.0.id', $id)
            ->assertJsonPath('data.0.name', 'Carla Mendes');

        $this->getJson('/api/v1/public/professionals')
            ->assertOk()
            ->assertJsonFragment(['id' => $id, 'name' => 'Carla Mendes']);
    }

    public function test_admin_can_create_inactive_professional_hidden_from_public(): void
    {
        Sanctum::actingAs($this->admin);

        $create = $this->postJson('/api/v1/professionals', [
            'name' => 'Inativa Lista',
            'specialty' => 'Manicure',
            'email' => '',
            'is_active' => false,
        ]);

        $create->assertCreated()
            ->assertJsonPath('data.is_active', false)
            ->assertJsonPath('data.email', null);

        $id = $create->json('data.id');

        $this->assertDatabaseHas('professionals', [
            'id' => $id,
            'is_active' => 0,
        ]);

        $this->getJson('/api/v1/professionals')
            ->assertOk()
            ->assertJsonFragment(['id' => $id, 'name' => 'Inativa Lista']);

        $this->getJson('/api/v1/public/professionals')
            ->assertOk()
            ->assertJsonMissing(['name' => 'Inativa Lista']);
    }

    public function test_admin_can_update_activate_and_delete_without_dependencies(): void
    {
        Sanctum::actingAs($this->admin);

        $id = $this->postJson('/api/v1/professionals', [
            'name' => 'Temporaria',
            'specialty' => 'Corte',
            'is_active' => true,
        ])->assertCreated()->json('data.id');

        $this->putJson("/api/v1/professionals/{$id}", [
            'specialty' => 'Corte avançado',
            'is_active' => false,
        ])->assertOk()
            ->assertJsonPath('data.specialty', 'Corte avançado')
            ->assertJsonPath('data.is_active', false);

        $this->putJson("/api/v1/professionals/{$id}", [
            'is_active' => true,
        ])->assertOk()
            ->assertJsonPath('data.is_active', true);

        $this->deleteJson("/api/v1/professionals/{$id}")
            ->assertNoContent();

        $this->assertDatabaseMissing('professionals', ['id' => $id]);
    }

    public function test_client_cannot_access_admin_professionals(): void
    {
        Sanctum::actingAs($this->clientUser);

        $this->getJson('/api/v1/professionals')->assertForbidden();
    }

    public function test_invalid_payload_returns_422(): void
    {
        Sanctum::actingAs($this->admin);

        $this->postJson('/api/v1/professionals', [
            'name' => '',
            'specialty' => '',
            'email' => 'not-an-email',
            'years_experience' => -1,
        ])->assertUnprocessable()
            ->assertJsonValidationErrors(['name', 'specialty', 'email', 'years_experience']);
    }

    public function test_cannot_delete_professional_with_appointments(): void
    {
        Sanctum::actingAs($this->admin);

        $professional = Professional::factory()->create(['name' => 'Com Histórico']);
        $client = Client::create([
            'name' => 'Cliente',
            'email' => 'cli-hist@test.com',
            'is_active' => true,
        ]);

        Appointment::create([
            'client_id' => $client->id,
            'service_id' => $this->service->id,
            'professional_id' => $professional->id,
            'start_time' => Carbon::parse('2026-08-11 10:00:00'),
            'end_time' => Carbon::parse('2026-08-11 11:00:00'),
            'status' => Appointment::STATUS_COMPLETED,
            'total_price' => 25,
            'is_active' => true,
        ]);

        $this->deleteJson("/api/v1/professionals/{$professional->id}")
            ->assertUnprocessable()
            ->assertJsonValidationErrors(['professional'])
            ->assertJsonPath(
                'errors.professional.0',
                'Este profissional possui dados associados e não pode ser eliminado. Desative-o em vez disso.',
            );

        $this->assertDatabaseHas('professionals', ['id' => $professional->id]);
        $this->assertDatabaseHas('appointments', ['professional_id' => $professional->id]);
    }

    public function test_public_lists_only_active_and_hides_inactive_detail(): void
    {
        $active = Professional::factory()->create([
            'name' => 'Ativa Silva',
            'is_active' => true,
            'biography' => 'Bio pública',
        ]);
        $inactive = Professional::factory()->inactive()->create([
            'name' => 'Inativa Costa',
        ]);

        $this->getJson('/api/v1/public/professionals')
            ->assertOk()
            ->assertJsonFragment(['name' => 'Ativa Silva'])
            ->assertJsonMissing(['name' => 'Inativa Costa'])
            ->assertJsonMissingPath('data.0.commission_percentage')
            ->assertJsonMissingPath('data.0.user_id');

        $this->getJson("/api/v1/public/professionals/{$active->id}")
            ->assertOk()
            ->assertJsonPath('data.biography', 'Bio pública');

        $this->getJson("/api/v1/public/professionals/{$inactive->id}")
            ->assertNotFound();
    }

    public function test_inactive_professional_is_rejected_by_public_booking(): void
    {
        $inactive = Professional::factory()->inactive()->create();

        ProfessionalSchedule::factory()
            ->forProfessional($inactive)
            ->onDay(DayOfWeek::Tuesday)
            ->window('09:00', '18:00')
            ->create();

        $this->postJson('/api/v1/public/appointments', [
            'service_id' => $this->service->id,
            'professional_id' => $inactive->id,
            'date' => '2026-08-11',
            'time' => '10:00',
            'client_name' => 'Guest',
            'client_email' => 'guest-inactive@test.com',
        ])->assertUnprocessable()
            ->assertJsonValidationErrors(['professional_id']);

        $this->getJson('/api/v1/public/availability?'.http_build_query([
            'professional_id' => $inactive->id,
            'service_id' => $this->service->id,
            'date' => '2026-08-11',
        ]))->assertUnprocessable()
            ->assertJsonValidationErrors(['professional_id']);
    }

    public function test_deactivating_keeps_history_and_hides_from_public(): void
    {
        Sanctum::actingAs($this->admin);

        $professional = Professional::factory()->create(['is_active' => true]);
        $client = Client::create([
            'name' => 'Histórico',
            'email' => 'hist@test.com',
            'is_active' => true,
        ]);

        Appointment::create([
            'client_id' => $client->id,
            'service_id' => $this->service->id,
            'professional_id' => $professional->id,
            'start_time' => Carbon::parse('2026-08-05 10:00:00'),
            'end_time' => Carbon::parse('2026-08-05 11:00:00'),
            'status' => Appointment::STATUS_COMPLETED,
            'total_price' => 25,
            'is_active' => true,
        ]);

        $this->putJson("/api/v1/professionals/{$professional->id}", [
            'is_active' => false,
        ])->assertOk();

        $this->getJson('/api/v1/public/professionals')
            ->assertOk()
            ->assertJsonMissing(['id' => $professional->id]);

        $this->assertDatabaseHas('appointments', [
            'professional_id' => $professional->id,
            'status' => Appointment::STATUS_COMPLETED,
        ]);
    }
}

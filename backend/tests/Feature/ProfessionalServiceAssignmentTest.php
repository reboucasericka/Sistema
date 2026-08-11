<?php

namespace Tests\Feature;

use App\Enums\DayOfWeek;
use App\Enums\UserRole;
use App\Models\Professional;
use App\Models\ProfessionalSchedule;
use App\Models\Service;
use App\Models\User;
use Carbon\Carbon;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Hash;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class ProfessionalServiceAssignmentTest extends TestCase
{
    use RefreshDatabase;

    private User $admin;

    private User $professionalUser;

    private User $clientUser;

    private Professional $professional;

    private Service $cut;

    private Service $color;

    private Service $inactiveService;

    protected function setUp(): void
    {
        parent::setUp();

        Carbon::setTestNow(Carbon::parse('2026-08-10 08:00:00'));

        $this->admin = User::create([
            'first_name' => 'Admin',
            'last_name' => 'Assign',
            'email' => 'admin-assign@test.com',
            'role' => UserRole::Admin,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->professionalUser = User::create([
            'first_name' => 'Ana',
            'last_name' => 'Assign',
            'email' => 'ana-assign@test.com',
            'role' => UserRole::Professional,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->clientUser = User::create([
            'first_name' => 'Client',
            'last_name' => 'Assign',
            'email' => 'client-assign@test.com',
            'role' => UserRole::Client,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->professional = Professional::create([
            'user_id' => $this->professionalUser->id,
            'name' => 'Ana Assign',
            'specialty' => 'Cabelo',
            'is_active' => true,
        ]);

        $this->cut = Service::create([
            'name' => 'Corte Feminino',
            'category' => 'Cabelo',
            'price' => 25,
            'duration_minutes' => 60,
            'is_active' => true,
        ]);

        $this->color = Service::create([
            'name' => 'Coloração',
            'category' => 'Cabelo',
            'price' => 45,
            'duration_minutes' => 90,
            'is_active' => true,
        ]);

        $this->inactiveService = Service::create([
            'name' => 'Serviço Inativo',
            'price' => 10,
            'duration_minutes' => 30,
            'is_active' => false,
        ]);

        ProfessionalSchedule::factory()
            ->forProfessional($this->professional)
            ->onDay(DayOfWeek::Monday)
            ->window('09:00', '18:00')
            ->create();
    }

    protected function tearDown(): void
    {
        Carbon::setTestNow();
        parent::tearDown();
    }

    public function test_admin_can_list_and_sync_professional_services(): void
    {
        Sanctum::actingAs($this->admin);

        $this->getJson("/api/v1/professionals/{$this->professional->id}/services")
            ->assertOk()
            ->assertJsonCount(0, 'data');

        $this->putJson("/api/v1/professionals/{$this->professional->id}/services", [
            'service_ids' => [$this->cut->id, $this->color->id],
        ])->assertOk()
            ->assertJsonPath('data.services_count', 2)
            ->assertJsonFragment(['name' => 'Corte Feminino'])
            ->assertJsonFragment(['name' => 'Coloração']);

        $this->assertDatabaseHas('professional_service', [
            'professional_id' => $this->professional->id,
            'service_id' => $this->cut->id,
        ]);
        $this->assertDatabaseHas('professional_service', [
            'professional_id' => $this->professional->id,
            'service_id' => $this->color->id,
        ]);

        $this->getJson("/api/v1/professionals/{$this->professional->id}/services")
            ->assertOk()
            ->assertJsonCount(2, 'data');

        $this->putJson("/api/v1/professionals/{$this->professional->id}/services", [
            'service_ids' => [$this->cut->id],
        ])->assertOk()
            ->assertJsonPath('data.services_count', 1);

        $this->assertDatabaseMissing('professional_service', [
            'professional_id' => $this->professional->id,
            'service_id' => $this->color->id,
        ]);

        $this->putJson("/api/v1/professionals/{$this->professional->id}/services", [
            'service_ids' => [$this->cut->id, $this->cut->id],
        ])->assertUnprocessable()
            ->assertJsonValidationErrors(['service_ids.1']);

        $this->assertSame(
            1,
            DB::table('professional_service')
                ->where('professional_id', $this->professional->id)
                ->where('service_id', $this->cut->id)
                ->count()
        );
    }

    public function test_invalid_or_inactive_service_ids_return_422(): void
    {
        Sanctum::actingAs($this->admin);

        $this->putJson("/api/v1/professionals/{$this->professional->id}/services", [
            'service_ids' => [$this->inactiveService->id],
        ])->assertUnprocessable()
            ->assertJsonValidationErrors(['service_ids.0']);

        $this->putJson("/api/v1/professionals/{$this->professional->id}/services", [
            'service_ids' => [99999],
        ])->assertUnprocessable()
            ->assertJsonValidationErrors(['service_ids.0']);
    }

    public function test_client_and_foreign_professional_cannot_sync(): void
    {
        Sanctum::actingAs($this->clientUser);

        $this->getJson("/api/v1/professionals/{$this->professional->id}/services")
            ->assertForbidden();

        $this->putJson("/api/v1/professionals/{$this->professional->id}/services", [
            'service_ids' => [$this->cut->id],
        ])->assertForbidden();

        $otherProUser = User::create([
            'first_name' => 'Other',
            'last_name' => 'Pro',
            'email' => 'other-pro@test.com',
            'role' => UserRole::Professional,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        Sanctum::actingAs($otherProUser);

        $this->getJson("/api/v1/professionals/{$this->professional->id}/services")
            ->assertForbidden();

        $this->putJson("/api/v1/professionals/{$this->professional->id}/services", [
            'service_ids' => [$this->cut->id],
        ])->assertForbidden();
    }

    public function test_professional_can_view_own_services_but_not_sync(): void
    {
        $this->professional->services()->attach([$this->cut->id]);

        Sanctum::actingAs($this->professionalUser);

        $this->getJson("/api/v1/professionals/{$this->professional->id}/services")
            ->assertOk()
            ->assertJsonCount(1, 'data');

        $this->putJson("/api/v1/professionals/{$this->professional->id}/services", [
            'service_ids' => [$this->cut->id, $this->color->id],
        ])->assertForbidden();
    }

    public function test_public_lists_only_active_associated_professionals(): void
    {
        $other = Professional::factory()->create([
            'name' => 'Bruno Livre',
            'is_active' => true,
        ]);
        $inactive = Professional::factory()->inactive()->create([
            'name' => 'Carla Inativa',
        ]);

        $this->professional->services()->attach([$this->cut->id]);
        $inactive->services()->attach([$this->cut->id]);

        $this->getJson("/api/v1/public/services/{$this->cut->id}/professionals")
            ->assertOk()
            ->assertJsonFragment(['name' => 'Ana Assign'])
            ->assertJsonMissing(['name' => 'Bruno Livre'])
            ->assertJsonMissing(['name' => 'Carla Inativa']);

        $this->getJson('/api/v1/public/professionals?'.http_build_query([
            'service_id' => $this->cut->id,
        ]))->assertOk()
            ->assertJsonFragment(['name' => 'Ana Assign'])
            ->assertJsonMissing(['name' => 'Bruno Livre']);

        $this->getJson("/api/v1/public/services/{$this->inactiveService->id}/professionals")
            ->assertNotFound();

        $this->assertTrue($other->exists());
    }

    public function test_booking_rejects_unassociated_professional_and_accepts_associated(): void
    {
        $unassociated = Professional::factory()->create([
            'name' => 'Sem Serviço',
            'is_active' => true,
        ]);

        ProfessionalSchedule::factory()
            ->forProfessional($unassociated)
            ->onDay(DayOfWeek::Monday)
            ->window('09:00', '18:00')
            ->create();

        $this->professional->services()->attach([$this->cut->id]);

        $this->postJson('/api/v1/public/appointments', [
            'service_id' => $this->cut->id,
            'professional_id' => $unassociated->id,
            'date' => '2026-08-10',
            'time' => '10:00',
            'client_name' => 'Guest',
            'client_email' => 'guest-assign@test.com',
        ])->assertUnprocessable()
            ->assertJsonValidationErrors(['professional_id'])
            ->assertJsonPath(
                'errors.professional_id.0',
                'O profissional selecionado não executa este serviço.',
            );

        $this->getJson('/api/v1/public/availability?'.http_build_query([
            'professional_id' => $unassociated->id,
            'service_id' => $this->cut->id,
            'date' => '2026-08-10',
        ]))->assertUnprocessable()
            ->assertJsonValidationErrors(['professional_id']);

        $this->postJson('/api/v1/public/appointments', [
            'service_id' => $this->cut->id,
            'professional_id' => $this->professional->id,
            'date' => '2026-08-10',
            'time' => '10:00',
            'client_name' => 'Guest Ok',
            'client_email' => 'guest-ok@test.com',
        ])->assertCreated()
            ->assertJsonPath('data.professional_id', $this->professional->id)
            ->assertJsonPath('data.service_id', $this->cut->id);
    }
}

<?php

namespace Tests\Feature;

use App\Enums\UserRole;
use App\Models\Product;
use App\Models\Professional;
use App\Models\Service;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Http\UploadedFile;
use Illuminate\Support\Facades\Hash;
use Illuminate\Support\Facades\Storage;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class MediaUploadTest extends TestCase
{
    use RefreshDatabase;

    private User $admin;

    private User $clientUser;

    protected function setUp(): void
    {
        parent::setUp();

        Storage::fake('public');

        $this->admin = User::create([
            'first_name' => 'Admin',
            'last_name' => 'Upload',
            'email' => 'admin-upload@test.com',
            'role' => UserRole::Admin,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->clientUser = User::create([
            'first_name' => 'Client',
            'last_name' => 'Upload',
            'email' => 'client-upload@test.com',
            'role' => UserRole::Client,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);
    }

    public function test_admin_creates_professional_with_image_and_resource_returns_url(): void
    {
        Sanctum::actingAs($this->admin);

        $file = UploadedFile::fake()->image('pro.jpg', 400, 400);

        $response = $this->post('/api/v1/professionals', [
            'name' => 'Ana Foto',
            'specialty' => 'Cílios',
            'is_active' => true,
            'photo' => $file,
        ], ['Accept' => 'application/json']);

        $response->assertCreated()
            ->assertJsonPath('data.name', 'Ana Foto')
            ->assertJsonStructure(['data' => ['photo', 'image_url']]);

        $path = $response->json('data.photo');
        $this->assertNotNull($path);
        $this->assertStringStartsWith('professionals/', $path);
        Storage::disk('public')->assertExists($path);
        $this->assertStringContainsString('/storage/'.$path, $response->json('data.image_url'));
    }

    public function test_admin_replaces_professional_photo_and_removes_old_file(): void
    {
        Sanctum::actingAs($this->admin);

        $first = UploadedFile::fake()->image('old.jpg');
        $create = $this->post('/api/v1/professionals', [
            'name' => 'Troca Foto',
            'specialty' => 'Makeup',
            'photo' => $first,
        ], ['Accept' => 'application/json'])->assertCreated();

        $oldPath = $create->json('data.photo');
        $id = $create->json('data.id');
        Storage::disk('public')->assertExists($oldPath);

        $second = UploadedFile::fake()->image('new.png');
        $update = $this->post("/api/v1/professionals/{$id}", [
            '_method' => 'PUT',
            'name' => 'Troca Foto',
            'specialty' => 'Makeup',
            'photo' => $second,
        ], ['Accept' => 'application/json'])->assertOk();

        $newPath = $update->json('data.photo');
        $this->assertNotSame($oldPath, $newPath);
        Storage::disk('public')->assertExists($newPath);
        Storage::disk('public')->assertMissing($oldPath);
    }

    public function test_invalid_format_and_oversized_file_return_422(): void
    {
        Sanctum::actingAs($this->admin);

        $this->post('/api/v1/professionals', [
            'name' => 'Inválido',
            'specialty' => 'Teste',
            'photo' => UploadedFile::fake()->create('doc.pdf', 100, 'application/pdf'),
        ], ['Accept' => 'application/json'])->assertStatus(422)->assertJsonValidationErrors(['photo']);

        $this->post('/api/v1/services', [
            'name' => 'Grande',
            'price' => 10,
            'duration_minutes' => 30,
            'image' => UploadedFile::fake()->image('big.jpg')->size(3072),
        ], ['Accept' => 'application/json'])->assertStatus(422)->assertJsonValidationErrors(['image']);
    }

    public function test_client_cannot_upload(): void
    {
        Sanctum::actingAs($this->clientUser);

        $this->post('/api/v1/professionals', [
            'name' => 'Bloqueado',
            'specialty' => 'X',
            'photo' => UploadedFile::fake()->image('x.jpg'),
        ], ['Accept' => 'application/json'])->assertForbidden();

        $this->post('/api/v1/products', [
            'name' => 'Produto',
            'price' => 10,
            'image' => UploadedFile::fake()->image('p.jpg'),
        ], ['Accept' => 'application/json'])->assertForbidden();
    }

    public function test_delete_professional_removes_file_deactivate_product_keeps_file(): void
    {
        Sanctum::actingAs($this->admin);

        $pro = $this->post('/api/v1/professionals', [
            'name' => 'Apagar',
            'specialty' => 'X',
            'photo' => UploadedFile::fake()->image('del.jpg'),
        ], ['Accept' => 'application/json'])->assertCreated();

        $proPath = $pro->json('data.photo');
        $proId = $pro->json('data.id');

        $this->deleteJson("/api/v1/professionals/{$proId}")->assertNoContent();
        Storage::disk('public')->assertMissing($proPath);
        $this->assertDatabaseMissing('professionals', ['id' => $proId]);

        $product = $this->post('/api/v1/products', [
            'name' => 'Creme',
            'price' => 15,
            'image' => UploadedFile::fake()->image('creme.jpg'),
        ], ['Accept' => 'application/json'])->assertCreated();

        $productPath = $product->json('data.image');
        $productId = $product->json('data.id');
        Storage::disk('public')->assertExists($productPath);

        $this->deleteJson("/api/v1/products/{$productId}")->assertOk();
        Storage::disk('public')->assertExists($productPath);
        $this->assertDatabaseHas('products', ['id' => $productId, 'is_active' => false, 'image' => $productPath]);
    }

    public function test_service_and_product_upload_and_remove_image_flag(): void
    {
        Sanctum::actingAs($this->admin);

        $service = $this->post('/api/v1/services', [
            'name' => 'Limpeza',
            'price' => 40,
            'duration_minutes' => 45,
            'image' => UploadedFile::fake()->image('svc.webp'),
        ], ['Accept' => 'application/json'])->assertCreated();

        $servicePath = $service->json('data.image');
        $serviceId = $service->json('data.id');
        $this->assertNotNull($service->json('data.image_url'));
        Storage::disk('public')->assertExists($servicePath);

        $this->post("/api/v1/services/{$serviceId}", [
            '_method' => 'PUT',
            'name' => 'Limpeza',
            'price' => 40,
            'duration_minutes' => 45,
            'remove_image' => true,
        ], ['Accept' => 'application/json'])->assertOk()
            ->assertJsonPath('data.image', null);

        Storage::disk('public')->assertMissing($servicePath);

        $product = Product::create([
            'name' => 'Serum',
            'price' => 20,
            'stock_quantity' => 5,
            'min_stock' => 1,
            'is_active' => true,
            'image' => null,
        ]);

        $upload = $this->post("/api/v1/products/{$product->id}", [
            '_method' => 'PUT',
            'name' => 'Serum',
            'price' => 20,
            'image' => UploadedFile::fake()->image('serum.jpg'),
        ], ['Accept' => 'application/json'])->assertOk();

        Storage::disk('public')->assertExists($upload->json('data.image'));
        $this->assertNotNull($upload->json('data.image_url'));
    }

    public function test_delete_service_removes_managed_image(): void
    {
        Sanctum::actingAs($this->admin);

        $service = $this->post('/api/v1/services', [
            'name' => 'Depilação',
            'price' => 15,
            'duration_minutes' => 20,
            'image' => UploadedFile::fake()->image('dep.jpg'),
        ], ['Accept' => 'application/json'])->assertCreated();

        $path = $service->json('data.image');
        $id = $service->json('data.id');

        $this->deleteJson("/api/v1/services/{$id}")->assertOk();
        Storage::disk('public')->assertMissing($path);
        $this->assertDatabaseMissing('services', ['id' => $id]);
    }

    public function test_legacy_public_path_is_not_deleted_as_storage_file(): void
    {
        Sanctum::actingAs($this->admin);

        $professional = Professional::create([
            'name' => 'Legado',
            'specialty' => 'X',
            'photo' => '/images/placeholders/user.png',
            'is_active' => true,
            'commission_percentage' => 0,
        ]);

        $this->deleteJson("/api/v1/professionals/{$professional->id}")->assertNoContent();
        $this->assertDatabaseMissing('professionals', ['id' => $professional->id]);
    }
}

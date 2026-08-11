<?php

namespace Tests\Feature;

use App\Enums\UserRole;
use App\Models\Product;
use App\Models\ProductCategory;
use App\Models\Service;
use App\Models\ServiceCategory;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Hash;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class CategoryCrudTest extends TestCase
{
    use RefreshDatabase;

    private User $admin;

    private User $professional;

    private User $client;

    protected function setUp(): void
    {
        parent::setUp();

        $this->admin = User::create([
            'first_name' => 'Admin',
            'last_name' => 'Cat',
            'email' => 'admin-cat@test.com',
            'role' => UserRole::Admin,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->professional = User::create([
            'first_name' => 'Pro',
            'last_name' => 'Cat',
            'email' => 'pro-cat@test.com',
            'role' => UserRole::Professional,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->client = User::create([
            'first_name' => 'Client',
            'last_name' => 'Cat',
            'email' => 'client-cat@test.com',
            'role' => UserRole::Client,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);
    }

    public function test_admin_can_crud_service_categories_with_auto_slug(): void
    {
        Sanctum::actingAs($this->admin);

        $create = $this->postJson('/api/v1/service-categories', [
            'name' => 'Sobrancelha',
            'description' => 'Cuidados de sobrancelha',
            'sort_order' => 2,
        ]);

        $create->assertCreated()
            ->assertJsonPath('data.name', 'Sobrancelha')
            ->assertJsonPath('data.slug', 'sobrancelha')
            ->assertJsonPath('data.is_active', true);

        $id = $create->json('data.id');

        $this->assertDatabaseHas('service_categories', [
            'id' => $id,
            'slug' => 'sobrancelha',
        ]);

        $this->getJson('/api/v1/service-categories?search=Sobra')
            ->assertOk()
            ->assertJsonFragment(['id' => $id]);

        $this->putJson("/api/v1/service-categories/{$id}", [
            'name' => 'Sobrancelhas',
            'is_active' => false,
        ])->assertOk()
            ->assertJsonPath('data.name', 'Sobrancelhas')
            ->assertJsonPath('data.is_active', false);

        $this->deleteJson("/api/v1/service-categories/{$id}")
            ->assertNoContent();

        $this->assertDatabaseMissing('service_categories', ['id' => $id]);
    }

    public function test_admin_can_crud_product_categories_and_unique_slug(): void
    {
        Sanctum::actingAs($this->admin);

        $this->postJson('/api/v1/product-categories', [
            'name' => 'Cabelo',
            'slug' => 'cabelo',
        ])->assertCreated();

        $this->postJson('/api/v1/product-categories', [
            'name' => 'Cabelo Extra',
            'slug' => 'cabelo',
        ])->assertUnprocessable()
            ->assertJsonValidationErrors(['slug']);

        $second = $this->postJson('/api/v1/product-categories', [
            'name' => 'Cabelo Extra',
        ])->assertCreated();

        $this->assertSame('cabelo-extra', $second->json('data.slug'));
    }

    public function test_non_admin_cannot_manage_categories(): void
    {
        Sanctum::actingAs($this->client);
        $this->getJson('/api/v1/service-categories')->assertForbidden();
        $this->getJson('/api/v1/product-categories')->assertForbidden();

        Sanctum::actingAs($this->professional);
        $this->postJson('/api/v1/service-categories', ['name' => 'X'])->assertForbidden();
        $this->postJson('/api/v1/product-categories', ['name' => 'Y'])->assertForbidden();
    }

    public function test_cannot_delete_category_with_dependencies(): void
    {
        Sanctum::actingAs($this->admin);

        $serviceCategory = ServiceCategory::create([
            'name' => 'Depilação',
            'slug' => 'depilacao',
            'is_active' => true,
            'sort_order' => 1,
        ]);

        Service::create([
            'name' => 'Depilação Pernas',
            'category' => 'Depilação',
            'service_category_id' => $serviceCategory->id,
            'price' => 20,
            'duration_minutes' => 30,
            'is_active' => true,
        ]);

        $this->deleteJson("/api/v1/service-categories/{$serviceCategory->id}")
            ->assertUnprocessable()
            ->assertJsonPath(
                'errors.category.0',
                'Esta categoria possui itens associados e não pode ser eliminada. Reatribua os itens ou desative a categoria.',
            );

        $productCategory = ProductCategory::create([
            'name' => 'Unhas',
            'slug' => 'unhas',
            'is_active' => true,
            'sort_order' => 1,
        ]);

        Product::create([
            'name' => 'Esmalte',
            'category' => 'Unhas',
            'product_category_id' => $productCategory->id,
            'price' => 8,
            'cost_price' => 3,
            'stock_quantity' => 10,
            'min_stock' => 2,
            'is_active' => true,
        ]);

        $this->deleteJson("/api/v1/product-categories/{$productCategory->id}")
            ->assertUnprocessable()
            ->assertJsonValidationErrors(['category']);
    }

    public function test_service_and_product_accept_category_ids_and_sync_legacy_text(): void
    {
        Sanctum::actingAs($this->admin);

        $serviceCategory = ServiceCategory::create([
            'name' => 'Beauty Treatment',
            'slug' => 'beauty-treatment',
            'is_active' => true,
            'sort_order' => 5,
        ]);

        $service = $this->postJson('/api/v1/services', [
            'name' => 'Massagem',
            'service_category_id' => $serviceCategory->id,
            'price' => 40,
            'duration_minutes' => 60,
            'is_active' => true,
        ])->assertCreated();

        $this->assertDatabaseHas('services', [
            'id' => $service->json('data.id'),
            'service_category_id' => $serviceCategory->id,
            'category' => 'Beauty Treatment',
        ]);

        $productCategory = ProductCategory::create([
            'name' => 'Cabelo',
            'slug' => 'cabelo',
            'is_active' => true,
            'sort_order' => 0,
        ]);

        $product = $this->postJson('/api/v1/products', [
            'name' => 'Shampoo',
            'product_category_id' => $productCategory->id,
            'price' => 15,
            'is_active' => true,
        ])->assertCreated();

        $this->assertDatabaseHas('products', [
            'id' => $product->json('data.id'),
            'product_category_id' => $productCategory->id,
            'category' => 'Cabelo',
        ]);
    }

    public function test_public_lists_only_active_categories_and_filters_items(): void
    {
        $active = ServiceCategory::create([
            'name' => 'Ativa',
            'slug' => 'ativa',
            'is_active' => true,
            'sort_order' => 1,
        ]);
        $inactive = ServiceCategory::create([
            'name' => 'Inativa',
            'slug' => 'inativa',
            'is_active' => false,
            'sort_order' => 2,
        ]);

        Service::create([
            'name' => 'Serviço Ativo',
            'category' => 'Ativa',
            'service_category_id' => $active->id,
            'price' => 10,
            'duration_minutes' => 30,
            'is_active' => true,
        ]);
        Service::create([
            'name' => 'Serviço Inativo Cat',
            'category' => 'Inativa',
            'service_category_id' => $inactive->id,
            'price' => 10,
            'duration_minutes' => 30,
            'is_active' => true,
        ]);

        $this->getJson('/api/v1/public/service-categories')
            ->assertOk()
            ->assertJsonFragment(['name' => 'Ativa'])
            ->assertJsonMissing(['name' => 'Inativa']);

        $this->getJson('/api/v1/public/services?service_category_id='.$active->id)
            ->assertOk()
            ->assertJsonFragment(['name' => 'Serviço Ativo'])
            ->assertJsonMissing(['name' => 'Serviço Inativo Cat']);

        ProductCategory::create([
            'name' => 'Prod Ativa',
            'slug' => 'prod-ativa',
            'is_active' => true,
            'sort_order' => 0,
        ]);
        ProductCategory::create([
            'name' => 'Prod Inativa',
            'slug' => 'prod-inativa',
            'is_active' => false,
            'sort_order' => 1,
        ]);

        $this->getJson('/api/v1/public/product-categories')
            ->assertOk()
            ->assertJsonFragment(['name' => 'Prod Ativa'])
            ->assertJsonMissing(['name' => 'Prod Inativa']);
    }
}

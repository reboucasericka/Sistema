<?php

namespace Tests\Feature;

use App\Enums\UserRole;
use App\Models\Product;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Hash;
use Tests\TestCase;

class ProductStockSaleTest extends TestCase
{
    use RefreshDatabase;

    private User $admin;

    private User $clientUser;

    protected function setUp(): void
    {
        parent::setUp();

        $this->admin = User::create([
            'first_name' => 'Admin',
            'last_name' => 'Test',
            'email' => 'admin@test.com',
            'role' => UserRole::Admin,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        $this->clientUser = User::create([
            'first_name' => 'Client',
            'last_name' => 'Test',
            'email' => 'client@test.com',
            'role' => UserRole::Client,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);
    }

    public function test_admin_can_create_product(): void
    {
        $response = $this->actingAsApi($this->admin)->postJson('/api/v1/products', [
            'name' => 'Produto Teste',
            'price' => 19.99,
            'stock_quantity' => 10,
            'min_stock' => 2,
        ]);

        $response->assertCreated()
            ->assertJsonPath('data.name', 'Produto Teste');

        $this->assertDatabaseHas('products', ['name' => 'Produto Teste']);
    }

    public function test_client_cannot_access_products(): void
    {
        $this->actingAsApi($this->clientUser)
            ->getJson('/api/v1/products')
            ->assertForbidden();
    }

    public function test_stock_in_increases_quantity(): void
    {
        $product = Product::create([
            'name' => 'Stock Test',
            'price' => 10,
            'stock_quantity' => 5,
            'min_stock' => 1,
        ]);

        $this->actingAsApi($this->admin)
            ->postJson("/api/v1/products/{$product->id}/stock/in", [
            'quantity' => 3,
        ])->assertCreated();

        $this->assertEquals(8, $product->fresh()->stock_quantity);
        $this->assertDatabaseHas('stock_movements', [
            'product_id' => $product->id,
            'type' => 'in',
            'new_quantity' => 8,
        ]);
    }

    public function test_paid_sale_reduces_stock(): void
    {
        $product = Product::create([
            'name' => 'Venda Test',
            'price' => 15,
            'stock_quantity' => 10,
            'min_stock' => 1,
        ]);

        $this->actingAsApi($this->admin)->postJson('/api/v1/sales', [
            'payment_method' => 'cash',
            'status' => 'paid',
            'items' => [
                ['product_id' => $product->id, 'quantity' => 4],
            ],
        ])->assertCreated();

        $this->assertEquals(6, $product->fresh()->stock_quantity);
    }

    public function test_sale_cannot_create_negative_stock(): void
    {
        $product = Product::create([
            'name' => 'Sem Stock',
            'price' => 15,
            'stock_quantity' => 2,
            'min_stock' => 1,
        ]);

        $this->actingAsApi($this->admin)->postJson('/api/v1/sales', [
            'payment_method' => 'cash',
            'status' => 'paid',
            'items' => [
                ['product_id' => $product->id, 'quantity' => 5],
            ],
        ])->assertStatus(422);

        $this->assertEquals(2, $product->fresh()->stock_quantity);
    }

    public function test_cancel_sale_restores_stock(): void
    {
        $product = Product::create([
            'name' => 'Cancel Test',
            'price' => 20,
            'stock_quantity' => 10,
            'min_stock' => 1,
        ]);

        $create = $this->actingAsApi($this->admin)->postJson('/api/v1/sales', [
            'payment_method' => 'card',
            'status' => 'paid',
            'items' => [
                ['product_id' => $product->id, 'quantity' => 3],
            ],
        ]);

        $saleId = $create->json('data.id');
        $this->assertEquals(7, $product->fresh()->stock_quantity);

        $this->actingAsApi($this->admin)->postJson("/api/v1/sales/{$saleId}/cancel")->assertOk();

        $this->assertEquals(10, $product->fresh()->stock_quantity);
    }
}

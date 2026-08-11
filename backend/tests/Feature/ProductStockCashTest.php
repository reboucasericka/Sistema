<?php

namespace Tests\Feature;

use App\Enums\UserRole;
use App\Models\Product;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Hash;
use Tests\TestCase;

class ProductStockCashTest extends TestCase
{
    use RefreshDatabase;

    private User $admin;

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
    }

    public function test_products_crud(): void
    {
        $create = $this->actingAsApi($this->admin)->postJson('/api/v1/products', [
            'name' => 'Creme Facial',
            'sku' => 'CRM-001',
            'barcode' => '1234567890123',
            'sale_price' => 29.99,
            'purchase_price' => 12.50,
            'current_stock' => 10,
            'minimum_stock' => 3,
        ]);

        $create->assertCreated()
            ->assertJsonPath('data.name', 'Creme Facial')
            ->assertJsonPath('data.sale_price', '29.99');

        $productId = $create->json('data.id');

        $this->actingAsApi($this->admin)
            ->getJson("/api/v1/products/{$productId}")
            ->assertOk()
            ->assertJsonPath('data.barcode', '1234567890123');

        $this->actingAsApi($this->admin)
            ->putJson("/api/v1/products/{$productId}", ['name' => 'Creme Facial Premium'])
            ->assertOk()
            ->assertJsonPath('data.name', 'Creme Facial Premium');

        $this->actingAsApi($this->admin)
            ->deleteJson("/api/v1/products/{$productId}")
            ->assertOk();

        $this->assertDatabaseHas('products', [
            'id' => $productId,
            'is_active' => false,
        ]);
    }

    public function test_stock_entry_increases_quantity(): void
    {
        $product = Product::factory()->create(['stock_quantity' => 5]);

        $this->actingAsApi($this->admin)->postJson('/api/v1/stock/entry', [
            'product_id' => $product->id,
            'quantity' => 4,
            'notes' => 'Compra fornecedor',
        ])->assertCreated()
            ->assertJsonPath('data.type', 'entry');

        $this->assertEquals(9, $product->fresh()->stock_quantity);
    }

    public function test_stock_exit_reduces_quantity(): void
    {
        $product = Product::factory()->create(['stock_quantity' => 8]);

        $this->actingAsApi($this->admin)->postJson('/api/v1/stock/exit', [
            'product_id' => $product->id,
            'quantity' => 3,
        ])->assertCreated()
            ->assertJsonPath('data.type', 'exit');

        $this->assertEquals(5, $product->fresh()->stock_quantity);
    }

    public function test_stock_exit_blocks_negative_stock(): void
    {
        $product = Product::factory()->create(['stock_quantity' => 2]);

        $this->actingAsApi($this->admin)->postJson('/api/v1/stock/exit', [
            'product_id' => $product->id,
            'quantity' => 5,
        ])->assertUnprocessable();

        $this->assertEquals(2, $product->fresh()->stock_quantity);
    }

    public function test_stock_adjustment_sets_quantity(): void
    {
        $product = Product::factory()->create(['stock_quantity' => 10]);

        $this->actingAsApi($this->admin)->postJson('/api/v1/stock/adjustment', [
            'product_id' => $product->id,
            'new_quantity' => 7,
        ])->assertCreated()
            ->assertJsonPath('data.type', 'adjustment');

        $this->assertEquals(7, $product->fresh()->stock_quantity);
    }

    public function test_low_stock_endpoint(): void
    {
        Product::factory()->create(['stock_quantity' => 10, 'min_stock' => 2]);
        $low = Product::factory()->lowStock()->create(['name' => 'Stock Baixo']);

        $response = $this->actingAsApi($this->admin)->getJson('/api/v1/stock/low');

        $response->assertOk()
            ->assertJsonFragment(['name' => 'Stock Baixo']);
    }

    public function test_stock_history_lists_movements(): void
    {
        $product = Product::factory()->create();

        $this->actingAsApi($this->admin)->postJson('/api/v1/stock/entry', [
            'product_id' => $product->id,
            'quantity' => 2,
        ]);

        $this->actingAsApi($this->admin)
            ->getJson('/api/v1/stock/history?product_id='.$product->id)
            ->assertOk()
            ->assertJsonPath('data.0.product_id', $product->id);
    }

    public function test_cash_open_close_and_transactions(): void
    {
        $this->actingAsApi($this->admin)->postJson('/api/v1/cash/open', [
            'opening_amount' => 100,
        ])->assertCreated();

        $this->actingAsApi($this->admin)->postJson('/api/v1/cash/income', [
            'amount' => 50,
            'description' => 'Venda balcão',
        ])->assertCreated();

        $this->actingAsApi($this->admin)->postJson('/api/v1/cash/expense', [
            'amount' => 20,
            'description' => 'Compra material',
        ])->assertCreated();

        $this->actingAsApi($this->admin)
            ->getJson('/api/v1/cash/current')
            ->assertOk()
            ->assertJsonPath('data.expected_balance', 130);

        $this->actingAsApi($this->admin)->postJson('/api/v1/cash/close', [
            'closing_amount' => 130,
        ])->assertOk();

        $this->actingAsApi($this->admin)
            ->getJson('/api/v1/cash/report')
            ->assertOk()
            ->assertJsonStructure(['data' => ['date', 'registers', 'totals']]);
    }

    public function test_cannot_open_two_registers(): void
    {
        $this->actingAsApi($this->admin)->postJson('/api/v1/cash/open', [
            'opening_amount' => 50,
        ])->assertCreated();

        $this->actingAsApi($this->admin)->postJson('/api/v1/cash/open', [
            'opening_amount' => 80,
        ])->assertUnprocessable();
    }

    public function test_income_requires_open_register(): void
    {
        $this->actingAsApi($this->admin)->postJson('/api/v1/cash/income', [
            'amount' => 10,
        ])->assertUnprocessable();
    }
}

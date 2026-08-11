<?php

namespace Database\Factories;

use App\Models\Product;
use Illuminate\Database\Eloquent\Factories\Factory;

/**
 * @extends Factory<Product>
 */
class ProductFactory extends Factory
{
    protected $model = Product::class;

    public function definition(): array
    {
        $purchase = fake()->randomFloat(2, 2, 50);
        $sale = $purchase + fake()->randomFloat(2, 5, 30);

        return [
            'name' => fake()->words(3, true),
            'description' => fake()->sentence(),
            'sku' => strtoupper(fake()->unique()->bothify('SKU-####')),
            'barcode' => fake()->unique()->numerify('#############'),
            'category' => fake()->randomElement(['Cabelo', 'Unhas', 'Pele', 'Acessórios']),
            'price' => $sale,
            'cost_price' => $purchase,
            'stock_quantity' => fake()->numberBetween(0, 50),
            'min_stock' => fake()->numberBetween(2, 10),
            'image' => null,
            'is_active' => true,
        ];
    }

    public function lowStock(): static
    {
        return $this->state(fn () => [
            'stock_quantity' => 1,
            'min_stock' => 5,
        ]);
    }
}

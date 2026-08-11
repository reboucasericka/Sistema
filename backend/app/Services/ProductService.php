<?php

namespace App\Services;

use App\Models\Product;
use Illuminate\Database\Eloquent\Collection;

class ProductService
{
    public function lowStockCount(): int
    {
        return Product::query()
            ->where('is_active', true)
            ->whereColumn('stock_quantity', '<=', 'min_stock')
            ->count();
    }

    public function lowStockProducts(int $limit = 5): Collection
    {
        return Product::query()
            ->where('is_active', true)
            ->whereColumn('stock_quantity', '<=', 'min_stock')
            ->orderBy('stock_quantity')
            ->limit($limit)
            ->get();
    }
}

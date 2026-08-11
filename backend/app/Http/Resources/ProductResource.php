<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

/** @mixin \App\Models\Product */
class ProductResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        return [
            'id' => $this->id,
            'name' => $this->name,
            'description' => $this->description,
            'sku' => $this->sku,
            'barcode' => $this->barcode,
            'category' => $this->categoryLabel(),
            'product_category_id' => $this->product_category_id,
            'product_category' => new ProductCategoryResource($this->whenLoaded('productCategory')),
            'price' => $this->price,
            'sale_price' => $this->price,
            'cost_price' => $this->cost_price,
            'purchase_price' => $this->cost_price,
            'stock_quantity' => $this->stock_quantity,
            'current_stock' => $this->stock_quantity,
            'min_stock' => $this->min_stock,
            'minimum_stock' => $this->min_stock,
            'image' => $this->image,
            'image_url' => $this->imageUrl(),
            'is_active' => $this->is_active,
            'is_low_stock' => $this->isLowStock(),
            'created_at' => $this->created_at?->toIso8601String(),
            'updated_at' => $this->updated_at?->toIso8601String(),
        ];
    }
}

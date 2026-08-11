<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

/** @mixin \App\Models\Service */
class ServiceResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        return [
            'id' => $this->id,
            'name' => $this->name,
            'category' => $this->categoryLabel(),
            'service_category_id' => $this->service_category_id,
            'service_category' => new ServiceCategoryResource($this->whenLoaded('serviceCategory')),
            'description' => $this->description,
            'price' => $this->price,
            'duration_minutes' => $this->duration_minutes,
            'image' => $this->image,
            'image_url' => $this->imageUrl(),
            'is_active' => $this->is_active,
            'appointments_count' => $this->whenCounted('appointments'),
            'created_at' => $this->created_at?->toIso8601String(),
            'updated_at' => $this->updated_at?->toIso8601String(),
        ];
    }
}

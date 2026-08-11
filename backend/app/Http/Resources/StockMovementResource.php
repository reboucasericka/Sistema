<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

/** @mixin \App\Models\StockMovement */
class StockMovementResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        return [
            'id' => $this->id,
            'product_id' => $this->product_id,
            'user_id' => $this->user_id,
            'type' => $this->mapTypeForApi($this->type?->value),
            'type_label' => $this->type?->label(),
            'quantity' => $this->quantity,
            'previous_quantity' => $this->previous_quantity,
            'new_quantity' => $this->new_quantity,
            'reason' => $this->reason,
            'notes' => $this->notes,
            'product' => new ProductResource($this->whenLoaded('product')),
            'user' => new UserResource($this->whenLoaded('user')),
            'created_at' => $this->created_at?->toIso8601String(),
        ];
    }

    private function mapTypeForApi(?string $type): ?string
    {
        return match ($type) {
            'in' => 'entry',
            'out' => 'exit',
            default => $type,
        };
    }
}

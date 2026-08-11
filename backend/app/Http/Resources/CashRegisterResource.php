<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

/** @mixin \App\Models\CashRegister */
class CashRegisterResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        return [
            'id' => $this->id,
            'opening_amount' => $this->opening_amount,
            'closing_amount' => $this->closing_amount,
            'opened_by' => $this->opened_by,
            'closed_by' => $this->closed_by,
            'opened_at' => $this->opened_at?->toIso8601String(),
            'closed_at' => $this->closed_at?->toIso8601String(),
            'status' => $this->status?->value,
            'status_label' => $this->status?->label(),
            'opened_by_user' => new UserResource($this->whenLoaded('openedByUser')),
            'closed_by_user' => new UserResource($this->whenLoaded('closedByUser')),
            'transactions' => CashTransactionResource::collection($this->whenLoaded('transactions')),
        ];
    }
}

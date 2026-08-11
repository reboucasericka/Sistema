<?php

namespace App\Services;

use App\Enums\StockMovementType;
use App\Models\Product;
use App\Models\StockMovement;
use App\Models\User;
use Illuminate\Support\Facades\DB;
use Illuminate\Validation\ValidationException;

class StockService
{
    public function stockIn(
        Product $product,
        int $quantity,
        ?User $user = null,
        ?string $reason = null,
        ?string $notes = null
    ): StockMovement {
        return $this->applyMovement($product, StockMovementType::In, $quantity, $user, $reason, $notes);
    }

    public function stockOut(
        Product $product,
        int $quantity,
        ?User $user = null,
        ?string $reason = null,
        ?string $notes = null
    ): StockMovement {
        return $this->applyMovement($product, StockMovementType::Out, $quantity, $user, $reason, $notes);
    }

    public function adjustStock(
        Product $product,
        int $newQuantity,
        ?User $user = null,
        ?string $reason = null,
        ?string $notes = null
    ): StockMovement {
        if ($newQuantity < 0) {
            throw ValidationException::withMessages([
                'new_quantity' => 'O stock não pode ser negativo.',
            ]);
        }

        return DB::transaction(function () use ($product, $newQuantity, $user, $reason, $notes) {
            $product = Product::query()->lockForUpdate()->findOrFail($product->id);
            $previous = $product->stock_quantity;
            $delta = abs($newQuantity - $previous);

            $product->update(['stock_quantity' => $newQuantity]);

            return StockMovement::create([
                'product_id' => $product->id,
                'user_id' => $user?->id,
                'type' => StockMovementType::Adjustment,
                'quantity' => $delta,
                'previous_quantity' => $previous,
                'new_quantity' => $newQuantity,
                'reason' => $reason ?? 'adjustment',
                'notes' => $notes,
            ]);
        });
    }

    private function applyMovement(
        Product $product,
        StockMovementType $type,
        int $quantity,
        ?User $user,
        ?string $reason,
        ?string $notes
    ): StockMovement {
        if ($quantity <= 0) {
            throw ValidationException::withMessages([
                'quantity' => 'A quantidade deve ser maior que zero.',
            ]);
        }

        return DB::transaction(function () use ($product, $type, $quantity, $user, $reason, $notes) {
            $product = Product::query()->lockForUpdate()->findOrFail($product->id);
            $previous = $product->stock_quantity;

            $newQuantity = match ($type) {
                StockMovementType::In => $previous + $quantity,
                StockMovementType::Out => $previous - $quantity,
                StockMovementType::Adjustment => $quantity,
            };

            if ($newQuantity < 0) {
                throw ValidationException::withMessages([
                    'quantity' => 'Stock insuficiente. Disponível: '.$previous,
                ]);
            }

            $product->update(['stock_quantity' => $newQuantity]);

            return StockMovement::create([
                'product_id' => $product->id,
                'user_id' => $user?->id,
                'type' => $type,
                'quantity' => $quantity,
                'previous_quantity' => $previous,
                'new_quantity' => $newQuantity,
                'reason' => $reason,
                'notes' => $notes,
            ]);
        });
    }
}

<?php

namespace App\Services;

use App\Enums\PaymentMethod;
use App\Enums\SaleStatus;
use App\Models\Product;
use App\Models\Sale;
use App\Models\Service;
use App\Models\User;
use Illuminate\Support\Facades\DB;
use Illuminate\Validation\ValidationException;

class SaleService
{
    public function __construct(private readonly StockService $stockService) {}

    public function create(array $data, User $user): Sale
    {
        return DB::transaction(function () use ($data, $user) {
            $items = $data['items'] ?? [];

            if (empty($items)) {
                throw ValidationException::withMessages([
                    'items' => 'A venda deve ter pelo menos um item.',
                ]);
            }

            $status = SaleStatus::from($data['status'] ?? SaleStatus::Paid->value);
            $paymentMethod = PaymentMethod::from($data['payment_method']);

            $sale = Sale::create([
                'client_id' => $data['client_id'] ?? null,
                'user_id' => $user->id,
                'total_amount' => 0,
                'payment_method' => $paymentMethod,
                'status' => $status,
                'notes' => $data['notes'] ?? null,
            ]);

            $total = 0;

            foreach ($items as $item) {
                $line = $this->buildSaleItem($sale, $item);
                $total += (float) $line->total_price;

                if ($line->product_id && $status === SaleStatus::Paid) {
                    $this->stockService->stockOut(
                        $line->product,
                        $line->quantity,
                        $user,
                        'sale',
                        'Venda #'.$sale->id
                    );
                }
            }

            $sale->update(['total_amount' => $total]);

            return $sale->fresh()->load(['items.product', 'items.service', 'client', 'user']);
        });
    }

    public function cancel(Sale $sale, User $user): Sale
    {
        if ($sale->isCancelled()) {
            throw ValidationException::withMessages([
                'sale' => 'Esta venda já está cancelada.',
            ]);
        }

        return DB::transaction(function () use ($sale, $user) {
            $sale = Sale::query()->lockForUpdate()->with('items.product')->findOrFail($sale->id);

            if ($sale->isPaid()) {
                foreach ($sale->items as $item) {
                    if ($item->product_id && $item->product) {
                        $this->stockService->stockIn(
                            $item->product,
                            $item->quantity,
                            $user,
                            'sale_cancelled',
                            'Cancelamento venda #'.$sale->id
                        );
                    }
                }
            }

            $sale->update(['status' => SaleStatus::Cancelled]);

            return $sale->fresh()->load(['items.product', 'items.service', 'client', 'user']);
        });
    }

    public function todayStats(): array
    {
        $salesToday = Sale::query()
            ->whereDate('created_at', today())
            ->where('status', SaleStatus::Paid)
            ->get();

        return [
            'sales_count' => $salesToday->count(),
            'revenue' => (float) $salesToday->sum('total_amount'),
        ];
    }

    private function buildSaleItem(Sale $sale, array $item)
    {
        $quantity = (int) ($item['quantity'] ?? 1);

        if ($quantity <= 0) {
            throw ValidationException::withMessages([
                'items' => 'Quantidade inválida.',
            ]);
        }

        $productId = $item['product_id'] ?? null;
        $serviceId = $item['service_id'] ?? null;

        if (! $productId && ! $serviceId) {
            throw ValidationException::withMessages([
                'items' => 'Cada item deve referenciar um produto ou serviço.',
            ]);
        }

        if ($productId && $serviceId) {
            throw ValidationException::withMessages([
                'items' => 'Item não pode ter produto e serviço ao mesmo tempo.',
            ]);
        }

        $description = $item['description'] ?? null;
        $unitPrice = isset($item['unit_price']) ? (float) $item['unit_price'] : null;

        if ($productId) {
            $product = Product::findOrFail($productId);
            $description ??= $product->name;
            $unitPrice ??= (float) $product->price;
        } else {
            $service = Service::findOrFail($serviceId);
            $description ??= $service->name;
            $unitPrice ??= (float) $service->price;
        }

        $totalPrice = round($unitPrice * $quantity, 2);

        return $sale->items()->create([
            'product_id' => $productId,
            'service_id' => $serviceId,
            'description' => $description,
            'quantity' => $quantity,
            'unit_price' => $unitPrice,
            'total_price' => $totalPrice,
        ])->load(['product', 'service']);
    }
}

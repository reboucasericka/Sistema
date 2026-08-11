<?php

namespace App\Http\Controllers\Api\V1;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\V1\StockAdjustRequest;
use App\Http\Requests\Api\V1\StockEntryRequest;
use App\Http\Requests\Api\V1\StockMovementRequest;
use App\Http\Resources\ProductResource;
use App\Http\Resources\StockMovementResource;
use App\Models\Product;
use App\Models\StockMovement;
use App\Services\ProductService;
use App\Services\StockService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;

class StockController extends Controller
{
    public function __construct(
        private readonly StockService $stockService,
        private readonly ProductService $productService
    ) {}

    public function entry(StockEntryRequest $request): JsonResponse
    {
        $product = Product::findOrFail($request->integer('product_id'));
        $this->authorize('manageStock', $product);

        $movement = $this->stockService->stockIn(
            $product,
            $request->integer('quantity'),
            $request->user(),
            $request->input('reason', 'entry'),
            $request->input('notes')
        );

        return response()->json([
            'message' => 'Entrada de stock registada.',
            'data' => new StockMovementResource($movement->load(['product', 'user'])),
        ], 201);
    }

    public function exit(StockEntryRequest $request): JsonResponse
    {
        $product = Product::findOrFail($request->integer('product_id'));
        $this->authorize('manageStock', $product);

        $movement = $this->stockService->stockOut(
            $product,
            $request->integer('quantity'),
            $request->user(),
            $request->input('reason', 'exit'),
            $request->input('notes')
        );

        return response()->json([
            'message' => 'Saída de stock registada.',
            'data' => new StockMovementResource($movement->load(['product', 'user'])),
        ], 201);
    }

    public function adjustment(StockAdjustRequest $request): JsonResponse
    {
        $product = Product::findOrFail($request->integer('product_id'));
        $this->authorize('manageStock', $product);

        $movement = $this->stockService->adjustStock(
            $product,
            $request->integer('new_quantity'),
            $request->user(),
            $request->input('reason', 'adjustment'),
            $request->input('notes')
        );

        return response()->json([
            'message' => 'Stock ajustado com sucesso.',
            'data' => new StockMovementResource($movement->load(['product', 'user'])),
        ], 201);
    }

    public function history(Request $request): AnonymousResourceCollection
    {
        $this->authorize('viewAny', StockMovement::class);

        $movements = StockMovement::query()
            ->with(['product', 'user'])
            ->when($request->filled('product_id'), fn ($q) => $q->where('product_id', $request->integer('product_id')))
            ->when($request->filled('type'), fn ($q) => $q->where('type', $this->mapTypeFilter($request->string('type')->toString())))
            ->latest()
            ->paginate($request->integer('per_page', 15));

        return StockMovementResource::collection($movements);
    }

    public function lowStock(): AnonymousResourceCollection
    {
        $this->authorize('viewAny', Product::class);

        return ProductResource::collection(
            Product::query()
                ->where('is_active', true)
                ->whereColumn('stock_quantity', '<=', 'min_stock')
                ->orderBy('stock_quantity')
                ->paginate(50)
        );
    }

    private function mapTypeFilter(string $type): string
    {
        return match ($type) {
            'entry' => 'in',
            'exit' => 'out',
            default => $type,
        };
    }
}

<?php

namespace App\Http\Controllers\Api\V1;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\V1\StockAdjustRequest;
use App\Http\Requests\Api\V1\StockMovementRequest;
use App\Http\Resources\StockMovementResource;
use App\Models\Product;
use App\Models\StockMovement;
use App\Services\StockService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;

class StockMovementController extends Controller
{
    public function __construct(private readonly StockService $stockService) {}

    public function index(Request $request): AnonymousResourceCollection
    {
        $this->authorize('viewAny', StockMovement::class);

        $movements = StockMovement::query()
            ->with(['product', 'user'])
            ->when($request->filled('product_id'), fn ($q) => $q->where('product_id', $request->integer('product_id')))
            ->when($request->filled('type'), fn ($q) => $q->where('type', $request->string('type')))
            ->latest()
            ->paginate($request->integer('per_page', 15));

        return StockMovementResource::collection($movements);
    }

    public function stockIn(StockMovementRequest $request, Product $product): JsonResponse
    {
        $this->authorize('manageStock', $product);

        $movement = $this->stockService->stockIn(
            $product,
            $request->integer('quantity'),
            $request->user(),
            $request->input('reason', 'manual_in'),
            $request->input('notes')
        );

        return response()->json([
            'message' => 'Entrada de stock registada.',
            'data' => new StockMovementResource($movement->load(['product', 'user'])),
        ], 201);
    }

    public function stockOut(StockMovementRequest $request, Product $product): JsonResponse
    {
        $this->authorize('manageStock', $product);

        $movement = $this->stockService->stockOut(
            $product,
            $request->integer('quantity'),
            $request->user(),
            $request->input('reason', 'manual_out'),
            $request->input('notes')
        );

        return response()->json([
            'message' => 'Saída de stock registada.',
            'data' => new StockMovementResource($movement->load(['product', 'user'])),
        ], 201);
    }

    public function adjust(StockAdjustRequest $request, Product $product): JsonResponse
    {
        $this->authorize('manageStock', $product);

        $movement = $this->stockService->adjustStock(
            $product,
            $request->integer('new_quantity'),
            $request->user(),
            $request->input('reason', 'manual_adjust'),
            $request->input('notes')
        );

        return response()->json([
            'message' => 'Stock ajustado com sucesso.',
            'data' => new StockMovementResource($movement->load(['product', 'user'])),
        ], 201);
    }
}

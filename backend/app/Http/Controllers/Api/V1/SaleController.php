<?php

namespace App\Http\Controllers\Api\V1;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\V1\StoreSaleRequest;
use App\Http\Resources\SaleResource;
use App\Models\Sale;
use App\Services\SaleService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;

class SaleController extends Controller
{
    public function __construct(private readonly SaleService $saleService) {}

    public function index(Request $request): AnonymousResourceCollection
    {
        $this->authorize('viewAny', Sale::class);

        $sales = Sale::query()
            ->with(['client', 'user', 'items'])
            ->when($request->filled('status'), fn ($q) => $q->where('status', $request->string('status')))
            ->when($request->filled('date'), fn ($q) => $q->whereDate('created_at', $request->string('date')))
            ->latest()
            ->paginate($request->integer('per_page', 15));

        return SaleResource::collection($sales);
    }

    public function store(StoreSaleRequest $request): JsonResponse
    {
        $this->authorize('create', Sale::class);

        $sale = $this->saleService->create($request->validated(), $request->user());

        return response()->json([
            'message' => 'Venda registada com sucesso.',
            'data' => new SaleResource($sale),
        ], 201);
    }

    public function show(Sale $sale): JsonResponse
    {
        $this->authorize('view', $sale);

        return response()->json([
            'data' => new SaleResource($sale->load(['client', 'user', 'items.product', 'items.service'])),
        ]);
    }

    public function cancel(Request $request, Sale $sale): JsonResponse
    {
        $this->authorize('cancel', $sale);

        $sale = $this->saleService->cancel($sale, $request->user());

        return response()->json([
            'message' => 'Venda cancelada e stock reposto.',
            'data' => new SaleResource($sale),
        ]);
    }
}

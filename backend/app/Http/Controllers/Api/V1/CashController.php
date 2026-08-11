<?php

namespace App\Http\Controllers\Api\V1;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\V1\CashCloseRequest;
use App\Http\Requests\Api\V1\CashOpenRequest;
use App\Http\Requests\Api\V1\CashTransactionRequest;
use App\Http\Resources\CashRegisterResource;
use App\Http\Resources\CashTransactionResource;
use App\Models\CashRegister;
use App\Services\CashService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class CashController extends Controller
{
    public function __construct(private readonly CashService $cashService) {}

    public function open(CashOpenRequest $request): JsonResponse
    {
        $this->authorize('manage', CashRegister::class);

        $register = $this->cashService->open(
            $request->user(),
            $request->float('opening_amount')
        );

        return response()->json([
            'message' => 'Caixa aberto com sucesso.',
            'data' => new CashRegisterResource($register->load('openedByUser')),
        ], 201);
    }

    public function close(CashCloseRequest $request): JsonResponse
    {
        $this->authorize('manage', CashRegister::class);

        $register = $this->cashService->close(
            $request->user(),
            $request->float('closing_amount')
        );

        return response()->json([
            'message' => 'Caixa fechado com sucesso.',
            'data' => new CashRegisterResource($register->load(['openedByUser', 'closedByUser', 'transactions'])),
        ]);
    }

    public function income(CashTransactionRequest $request): JsonResponse
    {
        $this->authorize('manage', CashRegister::class);

        $transaction = $this->cashService->recordIncome(
            $request->user(),
            $request->float('amount'),
            $request->input('description')
        );

        return response()->json([
            'message' => 'Receita registada com sucesso.',
            'data' => new CashTransactionResource($transaction->load(['user', 'cashRegister'])),
        ], 201);
    }

    public function expense(CashTransactionRequest $request): JsonResponse
    {
        $this->authorize('manage', CashRegister::class);

        $transaction = $this->cashService->recordExpense(
            $request->user(),
            $request->float('amount'),
            $request->input('description')
        );

        return response()->json([
            'message' => 'Despesa registada com sucesso.',
            'data' => new CashTransactionResource($transaction->load(['user', 'cashRegister'])),
        ], 201);
    }

    public function current(): JsonResponse
    {
        $this->authorize('viewAny', CashRegister::class);

        $summary = $this->cashService->currentSummary();

        return response()->json([
            'data' => $summary,
        ]);
    }

    public function report(Request $request): JsonResponse
    {
        $this->authorize('viewAny', CashRegister::class);

        $date = $request->filled('date') ? $request->date('date') : null;

        return response()->json([
            'data' => $this->cashService->dailyReport($date),
        ]);
    }
}

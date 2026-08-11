<?php

namespace App\Services;

use App\Enums\CashRegisterStatus;
use App\Enums\CashTransactionType;
use App\Models\CashRegister;
use App\Models\CashTransaction;
use App\Models\User;
use Carbon\Carbon;
use Illuminate\Support\Facades\DB;
use Illuminate\Validation\ValidationException;

class CashService
{
    public function open(User $user, float $openingAmount): CashRegister
    {
        if ($this->getOpenRegister()) {
            throw ValidationException::withMessages([
                'cash_register' => 'Já existe um caixa aberto.',
            ]);
        }

        return CashRegister::create([
            'opening_amount' => $openingAmount,
            'opened_by' => $user->id,
            'opened_at' => now(),
            'status' => CashRegisterStatus::Open,
        ]);
    }

    public function close(User $user, float $closingAmount): CashRegister
    {
        $register = $this->getOpenRegister();

        if (! $register) {
            throw ValidationException::withMessages([
                'cash_register' => 'Não existe caixa aberto para fechar.',
            ]);
        }

        $register->update([
            'closing_amount' => $closingAmount,
            'closed_by' => $user->id,
            'closed_at' => now(),
            'status' => CashRegisterStatus::Closed,
        ]);

        return $register->fresh();
    }

    public function recordIncome(User $user, float $amount, ?string $description = null): CashTransaction
    {
        return $this->recordTransaction($user, CashTransactionType::Income, $amount, $description);
    }

    public function recordExpense(User $user, float $amount, ?string $description = null): CashTransaction
    {
        return $this->recordTransaction($user, CashTransactionType::Expense, $amount, $description);
    }

    public function getOpenRegister(): ?CashRegister
    {
        return CashRegister::query()
            ->where('status', CashRegisterStatus::Open)
            ->latest('opened_at')
            ->first();
    }

    /**
     * @return array<string, mixed>|null
     */
    public function currentSummary(): ?array
    {
        $register = $this->getOpenRegister();

        if (! $register) {
            return null;
        }

        return $this->buildRegisterSummary($register);
    }

    /**
     * @return array<string, mixed>
     */
    public function dailyReport(?Carbon $date = null): array
    {
        $date = ($date ?? today())->copy()->startOfDay();
        $end = $date->copy()->endOfDay();

        $registers = CashRegister::query()
            ->with(['transactions', 'openedByUser', 'closedByUser'])
            ->whereBetween('opened_at', [$date, $end])
            ->orderBy('opened_at')
            ->get();

        $summaries = $registers->map(fn (CashRegister $register) => $this->buildRegisterSummary($register));

        return [
            'date' => $date->toDateString(),
            'registers' => $summaries,
            'totals' => [
                'opening_amount' => $summaries->sum('opening_amount'),
                'income' => $summaries->sum('income_total'),
                'expense' => $summaries->sum('expense_total'),
                'expected_balance' => $summaries->sum('expected_balance'),
                'closing_amount' => $summaries->sum('closing_amount'),
            ],
        ];
    }

    private function recordTransaction(
        User $user,
        CashTransactionType $type,
        float $amount,
        ?string $description
    ): CashTransaction {
        if ($amount <= 0) {
            throw ValidationException::withMessages([
                'amount' => 'O valor deve ser maior que zero.',
            ]);
        }

        $register = $this->getOpenRegister();

        if (! $register) {
            throw ValidationException::withMessages([
                'cash_register' => 'Abra o caixa antes de registar movimentos.',
            ]);
        }

        return CashTransaction::create([
            'cash_register_id' => $register->id,
            'type' => $type,
            'amount' => $amount,
            'description' => $description,
            'user_id' => $user->id,
        ]);
    }

    /**
     * @return array<string, mixed>
     */
    private function buildRegisterSummary(CashRegister $register): array
    {
        $register->loadMissing(['transactions', 'openedByUser', 'closedByUser']);

        $income = (float) $register->transactions
            ->where('type', CashTransactionType::Income)
            ->sum('amount');

        $expense = (float) $register->transactions
            ->where('type', CashTransactionType::Expense)
            ->sum('amount');

        $opening = (float) $register->opening_amount;
        $expected = $opening + $income - $expense;

        return [
            'id' => $register->id,
            'status' => $register->status->value,
            'status_label' => $register->status->label(),
            'opening_amount' => $opening,
            'closing_amount' => $register->closing_amount !== null ? (float) $register->closing_amount : null,
            'income_total' => $income,
            'expense_total' => $expense,
            'expected_balance' => $expected,
            'opened_at' => $register->opened_at?->toIso8601String(),
            'closed_at' => $register->closed_at?->toIso8601String(),
            'opened_by' => $register->openedByUser?->full_name,
            'closed_by' => $register->closedByUser?->full_name,
            'transactions_count' => $register->transactions->count(),
        ];
    }
}

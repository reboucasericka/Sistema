<?php

namespace App\Enums;

enum SaleStatus: string
{
    case Paid = 'paid';
    case Pending = 'pending';
    case Cancelled = 'cancelled';

    public function label(): string
    {
        return match ($this) {
            self::Paid => 'Paga',
            self::Pending => 'Pendente',
            self::Cancelled => 'Cancelada',
        };
    }
}

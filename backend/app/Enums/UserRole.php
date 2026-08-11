<?php

namespace App\Enums;

enum UserRole: string
{
    case Admin = 'admin';
    case Professional = 'professional';
    case Client = 'client';

    public function label(): string
    {
        return match ($this) {
            self::Admin => 'Administrador',
            self::Professional => 'Profissional',
            self::Client => 'Cliente',
        };
    }
}

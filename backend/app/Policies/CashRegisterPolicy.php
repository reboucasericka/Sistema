<?php

namespace App\Policies;

use App\Enums\UserRole;
use App\Models\CashRegister;
use App\Models\User;

class CashRegisterPolicy
{
    public function viewAny(User $user): bool
    {
        return in_array($user->role, [UserRole::Admin, UserRole::Professional], true);
    }

    public function manage(User $user): bool
    {
        return in_array($user->role, [UserRole::Admin, UserRole::Professional], true);
    }
}

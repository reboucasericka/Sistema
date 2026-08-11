<?php

namespace App\Policies;

use App\Enums\UserRole;
use App\Models\Product;
use App\Models\User;

class ProductPolicy
{
    public function viewAny(User $user): bool
    {
        return in_array($user->role, [UserRole::Admin, UserRole::Professional], true);
    }

    public function view(User $user, Product $product): bool
    {
        return $this->viewAny($user);
    }

    public function create(User $user): bool
    {
        return $user->role === UserRole::Admin;
    }

    public function update(User $user, Product $product): bool
    {
        return $user->role === UserRole::Admin;
    }

    public function delete(User $user, Product $product): bool
    {
        return $user->role === UserRole::Admin;
    }

    public function manageStock(User $user, Product $product): bool
    {
        return $user->role === UserRole::Admin;
    }
}

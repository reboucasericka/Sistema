<?php

namespace App\Policies;

use App\Enums\UserRole;
use App\Models\Professional;
use App\Models\User;

class ProfessionalPolicy
{
    public function viewAny(User $user): bool
    {
        return in_array($user->role, [UserRole::Admin, UserRole::Professional], true);
    }

    public function view(User $user, Professional $professional): bool
    {
        if ($user->role === UserRole::Admin) {
            return true;
        }

        if ($user->role === UserRole::Professional) {
            return $professional->user_id === $user->id;
        }

        return false;
    }

    public function create(User $user): bool
    {
        return $user->role === UserRole::Admin;
    }

    public function update(User $user, Professional $professional): bool
    {
        return $user->role === UserRole::Admin;
    }

    public function delete(User $user, Professional $professional): bool
    {
        return $user->role === UserRole::Admin;
    }
}

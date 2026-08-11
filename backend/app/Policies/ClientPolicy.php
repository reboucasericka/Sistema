<?php

namespace App\Policies;

use App\Enums\UserRole;
use App\Models\Client;
use App\Models\User;

class ClientPolicy
{
    public function viewAny(User $user): bool
    {
        return in_array($user->role, [UserRole::Admin, UserRole::Professional], true);
    }

    public function view(User $user, Client $client): bool
    {
        if (in_array($user->role, [UserRole::Admin, UserRole::Professional], true)) {
            return true;
        }

        return $user->role === UserRole::Client
            && $client->user_id === $user->id;
    }

    public function create(User $user): bool
    {
        return in_array($user->role, [UserRole::Admin, UserRole::Professional], true);
    }

    public function update(User $user, Client $client): bool
    {
        if ($user->role === UserRole::Admin) {
            return true;
        }

        return $user->role === UserRole::Client
            && $client->user_id === $user->id;
    }

    public function delete(User $user, Client $client): bool
    {
        return $user->role === UserRole::Admin;
    }
}

<?php

namespace App\Policies;

use App\Enums\UserRole;
use App\Models\ProfessionalSchedule;
use App\Models\User;

class ProfessionalSchedulePolicy
{
    public function viewAny(User $user): bool
    {
        return in_array($user->role, [UserRole::Admin, UserRole::Professional], true);
    }

    public function view(User $user, ProfessionalSchedule $professionalSchedule): bool
    {
        if ($user->role === UserRole::Admin) {
            return true;
        }

        if ($user->role === UserRole::Professional) {
            return $professionalSchedule->professional?->user_id === $user->id;
        }

        return false;
    }

    public function create(User $user): bool
    {
        return $user->role === UserRole::Admin;
    }

    public function update(User $user, ProfessionalSchedule $professionalSchedule): bool
    {
        return $user->role === UserRole::Admin;
    }

    public function delete(User $user, ProfessionalSchedule $professionalSchedule): bool
    {
        return $user->role === UserRole::Admin;
    }
}

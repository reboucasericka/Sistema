<?php

namespace Database\Factories;

use App\Enums\DayOfWeek;
use App\Enums\UserRole;
use App\Models\Professional;
use App\Models\ProfessionalSchedule;
use App\Models\User;
use Illuminate\Database\Eloquent\Factories\Factory;
use Illuminate\Support\Facades\Hash;

/**
 * @extends Factory<ProfessionalSchedule>
 */
class ProfessionalScheduleFactory extends Factory
{
    protected $model = ProfessionalSchedule::class;

    public function definition(): array
    {
        return [
            'professional_id' => $this->createProfessional()->id,
            'day_of_week' => DayOfWeek::Monday,
            'start_time' => '09:00:00',
            'end_time' => '18:00:00',
        ];
    }

    public function forProfessional(Professional $professional): static
    {
        return $this->state(fn () => [
            'professional_id' => $professional->id,
        ]);
    }

    public function onDay(DayOfWeek $day): static
    {
        return $this->state(fn () => [
            'day_of_week' => $day,
        ]);
    }

    public function window(string $start, string $end): static
    {
        return $this->state(fn () => [
            'start_time' => strlen($start) === 5 ? "{$start}:00" : $start,
            'end_time' => strlen($end) === 5 ? "{$end}:00" : $end,
        ]);
    }

    public function createdBy(?User $user): static
    {
        return $this->afterMaking(function (ProfessionalSchedule $schedule) use ($user) {
            $schedule->created_by = $user?->id;
        });
    }

    private function createProfessional(): Professional
    {
        $user = User::create([
            'first_name' => fake()->firstName(),
            'last_name' => fake()->lastName(),
            'email' => fake()->unique()->safeEmail(),
            'role' => UserRole::Professional,
            'is_active' => true,
            'password' => Hash::make('password'),
        ]);

        return Professional::create([
            'user_id' => $user->id,
            'name' => $user->full_name,
            'specialty' => 'Geral',
            'email' => $user->email,
            'is_active' => true,
        ]);
    }
}

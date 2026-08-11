<?php

namespace Database\Factories;

use App\Enums\UserRole;
use App\Models\Professional;
use App\Models\User;
use Illuminate\Database\Eloquent\Factories\Factory;
use Illuminate\Support\Facades\Hash;

/**
 * @extends Factory<Professional>
 */
class ProfessionalFactory extends Factory
{
    protected $model = Professional::class;

    public function definition(): array
    {
        return [
            'user_id' => null,
            'name' => fake()->name(),
            'specialty' => fake()->randomElement(['Cabeleireiro', 'Esteticista', 'Manicure', 'Barbeiro']),
            'photo' => null,
            'biography' => fake()->optional()->paragraph(),
            'phone' => fake()->optional()->numerify('9########'),
            'email' => fake()->optional()->safeEmail(),
            'instagram' => null,
            'facebook' => null,
            'years_experience' => fake()->optional()->numberBetween(1, 25),
            'commission_percentage' => 0,
            'is_active' => true,
        ];
    }

    public function inactive(): static
    {
        return $this->state(fn () => ['is_active' => false]);
    }

    public function withUser(?User $user = null): static
    {
        return $this->state(function () use ($user) {
            $account = $user ?? User::create([
                'first_name' => fake()->firstName(),
                'last_name' => fake()->lastName(),
                'email' => fake()->unique()->safeEmail(),
                'role' => UserRole::Professional,
                'is_active' => true,
                'password' => Hash::make('password'),
            ]);

            return [
                'user_id' => $account->id,
                'name' => $account->full_name,
                'email' => $account->email,
            ];
        });
    }
}

<?php

namespace Database\Seeders;

use App\Enums\DayOfWeek;
use App\Enums\UserRole;
use App\Models\Professional;
use App\Models\ProfessionalSchedule;
use App\Models\User;
use Illuminate\Database\Seeder;

class ProfessionalScheduleSeeder extends Seeder
{
    public function run(): void
    {
        $professional = Professional::query()
            ->where(function ($query) {
                $query->where('email', 'ana.costa@salon.test')
                    ->orWhere('name', 'Ana Costa');
            })
            ->first();

        if (! $professional) {
            return;
        }

        if (ProfessionalSchedule::query()->where('professional_id', $professional->id)->exists()) {
            return;
        }

        $adminId = User::query()->where('role', UserRole::Admin)->value('id');

        $weekdayWindows = [
            ['09:00:00', '12:00:00'],
            ['14:00:00', '18:00:00'],
        ];

        $weekdays = [
            DayOfWeek::Monday,
            DayOfWeek::Tuesday,
            DayOfWeek::Wednesday,
            DayOfWeek::Thursday,
            DayOfWeek::Friday,
        ];

        foreach ($weekdays as $day) {
            foreach ($weekdayWindows as [$start, $end]) {
                $this->createSchedule($professional->id, $day, $start, $end, $adminId);
            }
        }

        $this->createSchedule(
            $professional->id,
            DayOfWeek::Saturday,
            '09:00:00',
            '13:00:00',
            $adminId
        );
    }

    private function createSchedule(
        int $professionalId,
        DayOfWeek $day,
        string $start,
        string $end,
        ?int $createdBy
    ): void {
        $schedule = new ProfessionalSchedule([
            'professional_id' => $professionalId,
            'day_of_week' => $day,
            'start_time' => $start,
            'end_time' => $end,
        ]);
        $schedule->created_by = $createdBy;
        $schedule->save();
    }
}

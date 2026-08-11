<?php

namespace App\Services;

use App\Models\ProfessionalSchedule;
use App\Models\User;
use Illuminate\Database\Eloquent\Collection;
use Illuminate\Support\Facades\DB;
use Illuminate\Validation\ValidationException;

class ProfessionalScheduleService
{
    public function create(array $data, User $actor): ProfessionalSchedule
    {
        $this->assertNoOverlap(
            (int) $data['professional_id'],
            (int) $data['day_of_week'],
            $this->normalizeTime($data['start_time']),
            $this->normalizeTime($data['end_time']),
        );

        return DB::transaction(function () use ($data, $actor) {
            $schedule = new ProfessionalSchedule([
                'professional_id' => $data['professional_id'],
                'day_of_week' => $data['day_of_week'],
                'start_time' => $this->normalizeTime($data['start_time'], withSeconds: true),
                'end_time' => $this->normalizeTime($data['end_time'], withSeconds: true),
            ]);
            $schedule->created_by = $actor->id;
            $schedule->save();

            return $schedule->fresh(['professional', 'creator']);
        });
    }

    public function update(ProfessionalSchedule $schedule, array $data): ProfessionalSchedule
    {
        $professionalId = (int) ($data['professional_id'] ?? $schedule->professional_id);
        $dayOfWeek = (int) ($data['day_of_week'] ?? $schedule->day_of_week->value);
        $start = $this->normalizeTime($data['start_time'] ?? $schedule->startTimeHi());
        $end = $this->normalizeTime($data['end_time'] ?? $schedule->endTimeHi());

        $this->assertNoOverlap($professionalId, $dayOfWeek, $start, $end, $schedule->id);

        return DB::transaction(function () use ($schedule, $data, $professionalId, $dayOfWeek, $start, $end) {
            $payload = [
                'professional_id' => $professionalId,
                'day_of_week' => $dayOfWeek,
                'start_time' => $this->normalizeTime($start, withSeconds: true),
                'end_time' => $this->normalizeTime($end, withSeconds: true),
            ];

            $schedule->update($payload);

            return $schedule->fresh(['professional', 'creator']);
        });
    }

    public function assertNoOverlap(
        int $professionalId,
        int $dayOfWeek,
        string $start,
        string $end,
        ?int $ignoreId = null
    ): void {
        $start = $this->normalizeTime($start);
        $end = $this->normalizeTime($end);

        $query = ProfessionalSchedule::query()
            ->where('professional_id', $professionalId)
            ->where('day_of_week', $dayOfWeek);

        if ($ignoreId) {
            $query->where('id', '!=', $ignoreId);
        }

        /** @var Collection<int, ProfessionalSchedule> $existing */
        $existing = $query->get();

        foreach ($existing as $window) {
            $existingStart = $window->startTimeHi();
            $existingEnd = $window->endTimeHi();

            // Semi-open intervals [start, end): overlap if new_start < existing_end AND new_end > existing_start
            if ($start < $existingEnd && $end > $existingStart) {
                throw ValidationException::withMessages([
                    'start_time' => 'Já existe uma janela de horário sobreposta para este profissional neste dia.',
                ]);
            }
        }
    }

    /**
     * @return Collection<int, ProfessionalSchedule>
     */
    public function forProfessionalOnDay(int $professionalId, int $dayOfWeek): Collection
    {
        return ProfessionalSchedule::query()
            ->where('professional_id', $professionalId)
            ->where('day_of_week', $dayOfWeek)
            ->orderBy('start_time')
            ->get();
    }

    private function normalizeTime(string $time, bool $withSeconds = false): string
    {
        $hi = strlen($time) >= 5 ? substr($time, 0, 5) : $time;

        return $withSeconds ? "{$hi}:00" : $hi;
    }
}

<?php

namespace App\Services;

use App\Models\Appointment;
use App\Models\Service;
use Carbon\Carbon;
use Illuminate\Validation\ValidationException;

class AppointmentService
{
    private const SLOT_INTERVAL_MINUTES = 30;

    public function __construct(
        private readonly ProfessionalScheduleService $professionalScheduleService
    ) {}

    public function calculateEndTime(Carbon $startTime, Service $service): Carbon
    {
        return $startTime->copy()->addMinutes($service->duration_minutes);
    }

    /**
     * @return list<string> Times in H:i format
     */
    public function getAvailableSlots(int $professionalId, Service $service, Carbon $date): array
    {
        $duration = $service->duration_minutes;
        $dayOfWeek = $date->dayOfWeek;

        $windows = $this->professionalScheduleService->forProfessionalOnDay($professionalId, $dayOfWeek);

        if ($windows->isEmpty()) {
            return [];
        }

        $slots = [];

        foreach ($windows as $window) {
            $dayStart = $date->copy()->setTimeFromTimeString($window->startTimeHi());
            $dayEnd = $date->copy()->setTimeFromTimeString($window->endTimeHi());
            $cursor = $dayStart->copy();

            while ($cursor->copy()->addMinutes($duration)->lte($dayEnd)) {
                $slotEnd = $cursor->copy()->addMinutes($duration);

                if ($cursor->isFuture() && ! $this->hasScheduleConflict($professionalId, $cursor, $slotEnd)) {
                    $slots[] = $cursor->format('H:i');
                }

                $cursor->addMinutes(self::SLOT_INTERVAL_MINUTES);
            }
        }

        $slots = array_values(array_unique($slots));
        sort($slots);

        return $slots;
    }

    public function hasScheduleConflict(
        int $professionalId,
        Carbon $startTime,
        Carbon $endTime,
        ?int $ignoreAppointmentId = null
    ): bool {
        $query = Appointment::query()
            ->where('professional_id', $professionalId)
            ->where('is_active', true)
            ->whereNot('status', Appointment::STATUS_CANCELED)
            ->where('start_time', '<', $endTime)
            ->where('end_time', '>', $startTime);

        if ($ignoreAppointmentId) {
            $query->where('id', '!=', $ignoreAppointmentId);
        }

        return $query->exists();
    }

    public function ensureNoScheduleConflict(
        int $professionalId,
        Carbon $startTime,
        Carbon $endTime,
        ?int $ignoreAppointmentId = null
    ): void {
        if ($this->hasScheduleConflict($professionalId, $startTime, $endTime, $ignoreAppointmentId)) {
            throw ValidationException::withMessages([
                'start_time' => 'O profissional já tem um agendamento neste horário.',
            ]);
        }
    }

    public function ensureWithinProfessionalSchedule(
        int $professionalId,
        Carbon $start,
        Carbon $end
    ): void {
        $windows = $this->professionalScheduleService->forProfessionalOnDay(
            $professionalId,
            $start->dayOfWeek
        );

        foreach ($windows as $window) {
            $windowStart = $start->copy()->setTimeFromTimeString($window->startTimeHi());
            $windowEnd = $start->copy()->setTimeFromTimeString($window->endTimeHi());

            if ($start->gte($windowStart) && $end->lte($windowEnd)) {
                return;
            }
        }

        throw ValidationException::withMessages([
            'start_time' => 'O horário selecionado está fora da disponibilidade do profissional.',
        ]);
    }
}

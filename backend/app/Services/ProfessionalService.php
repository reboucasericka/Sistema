<?php

namespace App\Services;

use App\Models\Appointment;
use App\Models\Professional;
use Illuminate\Http\UploadedFile;
use Illuminate\Support\Facades\DB;
use Illuminate\Validation\ValidationException;

class ProfessionalService
{
    public function __construct(private readonly MediaUploadService $media) {}

    /**
     * @param  array<string, mixed>  $data
     */
    public function create(array $data, ?UploadedFile $photo = null): Professional
    {
        unset($data['remove_photo'], $data['photo']);

        if ($photo) {
            $data['photo'] = $this->media->store($photo, 'professionals');
        }

        return Professional::create([
            ...$data,
            'is_active' => $data['is_active'] ?? true,
            'commission_percentage' => $data['commission_percentage'] ?? 0,
        ]);
    }

    /**
     * @param  array<string, mixed>  $data
     */
    public function update(Professional $professional, array $data, ?UploadedFile $photo = null): Professional
    {
        $removePhoto = (bool) ($data['remove_photo'] ?? false);
        unset($data['remove_photo'], $data['photo']);

        $previousPhoto = $professional->photo;
        $storedPath = null;

        if ($photo) {
            $storedPath = $this->media->store($photo, 'professionals');
            $data['photo'] = $storedPath;
        } elseif ($removePhoto) {
            $data['photo'] = null;
        }

        try {
            $professional->update($data);
        } catch (\Throwable $e) {
            if ($storedPath) {
                $this->media->delete($storedPath);
            }
            throw $e;
        }

        if (($photo || $removePhoto) && $previousPhoto && $previousPhoto !== $professional->photo) {
            $this->media->delete($previousPhoto);
        }

        return $professional->fresh();
    }

    public function ensureIsActive(int $professionalId): Professional
    {
        $professional = Professional::query()->find($professionalId);

        if (! $professional || ! $professional->is_active) {
            throw ValidationException::withMessages([
                'professional_id' => 'O profissional selecionado não está disponível para novos agendamentos.',
            ]);
        }

        return $professional;
    }

    public function delete(Professional $professional): void
    {
        $hasAppointments = Appointment::query()
            ->where('professional_id', $professional->id)
            ->exists();

        if ($hasAppointments) {
            throw ValidationException::withMessages([
                'professional' => 'Este profissional possui dados associados e não pode ser eliminado. Desative-o em vez disso.',
            ]);
        }

        $photo = $professional->photo;

        DB::transaction(function () use ($professional) {
            $professional->schedules()->delete();
            $professional->delete();
        });

        $this->media->delete($photo);
    }
}

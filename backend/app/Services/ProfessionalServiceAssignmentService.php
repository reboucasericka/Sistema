<?php

namespace App\Services;

use App\Models\Professional;
use App\Models\Service;
use Illuminate\Support\Collection;
use Illuminate\Support\Facades\DB;
use Illuminate\Validation\ValidationException;

class ProfessionalServiceAssignmentService
{
    public function listFor(Professional $professional): Collection
    {
        return $professional->services()
            ->with('serviceCategory')
            ->orderBy('name')
            ->get();
    }

    /**
     * @param  list<int>  $serviceIds
     */
    public function sync(Professional $professional, array $serviceIds): Professional
    {
        $uniqueIds = array_values(array_unique(array_map('intval', $serviceIds)));

        $activeIds = Service::query()
            ->whereIn('id', $uniqueIds)
            ->where('is_active', true)
            ->pluck('id')
            ->all();

        sort($uniqueIds);
        $sortedActive = $activeIds;
        sort($sortedActive);

        if ($uniqueIds !== $sortedActive) {
            throw ValidationException::withMessages([
                'service_ids' => 'Um ou mais serviços são inválidos ou estão inativos.',
            ]);
        }

        DB::transaction(function () use ($professional, $uniqueIds) {
            $professional->services()->sync($uniqueIds);
        });

        return $professional->load([
            'services' => fn ($query) => $query->with('serviceCategory')->orderBy('name'),
        ]);
    }

    public function ensureOffersService(Professional $professional, int $serviceId): void
    {
        $offers = $professional->services()
            ->where('services.id', $serviceId)
            ->where('services.is_active', true)
            ->exists();

        if (! $offers) {
            throw ValidationException::withMessages([
                'professional_id' => 'O profissional selecionado não executa este serviço.',
            ]);
        }
    }

    public function activeProfessionalsForService(Service $service): Collection
    {
        if (! $service->is_active) {
            return collect();
        }

        return $service->professionals()
            ->where('professionals.is_active', true)
            ->orderBy('professionals.name')
            ->get();
    }
}

<?php

namespace App\Http\Controllers\Api\V1;

use App\Enums\UserRole;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\V1\StoreProfessionalScheduleRequest;
use App\Http\Requests\Api\V1\UpdateProfessionalScheduleRequest;
use App\Http\Resources\ProfessionalScheduleResource;
use App\Models\ProfessionalSchedule;
use App\Services\ProfessionalScheduleService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;

class ProfessionalScheduleController extends Controller
{
    public function __construct(
        private readonly ProfessionalScheduleService $professionalScheduleService
    ) {}

    public function index(Request $request): AnonymousResourceCollection
    {
        $this->authorize('viewAny', ProfessionalSchedule::class);

        $query = ProfessionalSchedule::query()
            ->with(['professional', 'creator'])
            ->when($request->filled('professional_id'), fn ($q) => $q->where('professional_id', $request->integer('professional_id')))
            ->when($request->filled('day_of_week'), fn ($q) => $q->where('day_of_week', $request->integer('day_of_week')))
            ->orderBy('professional_id')
            ->orderBy('day_of_week')
            ->orderBy('start_time');

        $user = $request->user();

        if ($user->role === UserRole::Professional) {
            $professionalId = $user->professionalProfile?->id;
            $query->where('professional_id', $professionalId);
        }

        $schedules = $query->paginate($request->integer('per_page', 15));

        return ProfessionalScheduleResource::collection($schedules);
    }

    public function store(StoreProfessionalScheduleRequest $request): JsonResponse
    {
        $this->authorize('create', ProfessionalSchedule::class);

        $schedule = $this->professionalScheduleService->create(
            $request->validated(),
            $request->user()
        );

        return response()->json([
            'message' => 'Horário criado com sucesso.',
            'data' => new ProfessionalScheduleResource($schedule),
        ], 201);
    }

    public function show(ProfessionalSchedule $professionalSchedule): JsonResponse
    {
        $this->authorize('view', $professionalSchedule);

        $professionalSchedule->load(['professional', 'creator']);

        return response()->json([
            'data' => new ProfessionalScheduleResource($professionalSchedule),
        ]);
    }

    public function update(
        UpdateProfessionalScheduleRequest $request,
        ProfessionalSchedule $professionalSchedule
    ): JsonResponse {
        $this->authorize('update', $professionalSchedule);

        $schedule = $this->professionalScheduleService->update(
            $professionalSchedule,
            $request->validated()
        );

        return response()->json([
            'message' => 'Horário atualizado com sucesso.',
            'data' => new ProfessionalScheduleResource($schedule),
        ]);
    }

    public function destroy(ProfessionalSchedule $professionalSchedule): JsonResponse
    {
        $this->authorize('delete', $professionalSchedule);

        $professionalSchedule->delete();

        return response()->json([
            'message' => 'Horário eliminado com sucesso.',
        ]);
    }
}

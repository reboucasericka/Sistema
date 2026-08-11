<?php

namespace App\Http\Controllers\Api\V1;

use App\Enums\UserRole;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\V1\StoreAppointmentRequest;
use App\Http\Requests\Api\V1\UpdateAppointmentRequest;
use App\Http\Resources\AppointmentResource;
use App\Models\Appointment;
use App\Models\Service;
use App\Services\AppointmentService;
use App\Services\ProfessionalService;
use App\Services\ProfessionalServiceAssignmentService;
use Carbon\Carbon;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;

class AppointmentController extends Controller
{
    public function __construct(
        private readonly AppointmentService $appointmentService,
        private readonly ProfessionalService $professionalService,
        private readonly ProfessionalServiceAssignmentService $assignmentService,
    ) {}

    public function index(Request $request): AnonymousResourceCollection
    {
        $this->authorize('viewAny', Appointment::class);

        $query = Appointment::query()
            ->with(['client', 'service', 'professional'])
            ->when($request->filled('status'), fn ($q) => $q->where('status', $request->string('status')))
            ->when($request->filled('professional_id'), fn ($q) => $q->where('professional_id', $request->integer('professional_id')))
            ->when($request->filled('client_id'), fn ($q) => $q->where('client_id', $request->integer('client_id')))
            ->when($request->filled('date'), fn ($q) => $q->whereDate('start_time', $request->string('date')))
            ->orderBy('start_time');

        $user = $request->user();

        if ($user->role === UserRole::Professional) {
            $professionalId = $user->professionalProfile?->id;
            $query->where('professional_id', $professionalId);
        }

        if ($user->role === UserRole::Client) {
            $clientId = $user->clientProfile?->id;
            $query->where('client_id', $clientId);
        }

        $appointments = $query->paginate($request->integer('per_page', 15));

        return AppointmentResource::collection($appointments);
    }

    public function store(StoreAppointmentRequest $request): JsonResponse
    {
        $this->authorize('create', Appointment::class);

        $service = Service::findOrFail($request->integer('service_id'));
        $professional = $this->professionalService->ensureIsActive($request->integer('professional_id'));
        $this->assignmentService->ensureOffersService($professional, $service->id);
        $startTime = Carbon::parse($request->string('start_time')->toString());
        $endTime = $this->appointmentService->calculateEndTime($startTime, $service);

        $this->appointmentService->ensureNoScheduleConflict(
            $request->integer('professional_id'),
            $startTime,
            $endTime
        );

        $this->appointmentService->ensureWithinProfessionalSchedule(
            $request->integer('professional_id'),
            $startTime,
            $endTime
        );

        $appointment = Appointment::create([
            ...$request->validated(),
            'start_time' => $startTime,
            'end_time' => $endTime,
            'status' => $request->input('status', Appointment::STATUS_PENDING),
            'total_price' => $service->price,
        ]);

        return response()->json([
            'message' => 'Agendamento criado com sucesso.',
            'data' => new AppointmentResource($appointment->load(['client', 'service', 'professional'])),
        ], 201);
    }

    public function show(Appointment $appointment): JsonResponse
    {
        $this->authorize('view', $appointment);

        return response()->json([
            'data' => new AppointmentResource($appointment->load(['client', 'service', 'professional'])),
        ]);
    }

    public function update(UpdateAppointmentRequest $request, Appointment $appointment): JsonResponse
    {
        $this->authorize('update', $appointment);

        $data = $request->validated();
        $service = isset($data['service_id'])
            ? Service::findOrFail($data['service_id'])
            : $appointment->service;

        $startTime = isset($data['start_time'])
            ? Carbon::parse($data['start_time'])
            : $appointment->start_time;

        $endTime = $this->appointmentService->calculateEndTime($startTime, $service);
        $professionalId = $data['professional_id'] ?? $appointment->professional_id;

        $professional = $this->professionalService->ensureIsActive((int) $professionalId);
        $this->assignmentService->ensureOffersService($professional, $service->id);

        $this->appointmentService->ensureNoScheduleConflict(
            $professionalId,
            $startTime,
            $endTime,
            $appointment->id
        );

        $this->appointmentService->ensureWithinProfessionalSchedule(
            $professionalId,
            $startTime,
            $endTime
        );

        $appointment->update([
            ...$data,
            'start_time' => $startTime,
            'end_time' => $endTime,
            'total_price' => $service->price,
        ]);

        return response()->json([
            'message' => 'Agendamento atualizado com sucesso.',
            'data' => new AppointmentResource($appointment->fresh()->load(['client', 'service', 'professional'])),
        ]);
    }

    public function destroy(Appointment $appointment): JsonResponse
    {
        $this->authorize('delete', $appointment);

        $appointment->update([
            'status' => Appointment::STATUS_CANCELED,
            'is_active' => false,
        ]);

        return response()->json([
            'message' => 'Agendamento cancelado com sucesso.',
        ]);
    }
}

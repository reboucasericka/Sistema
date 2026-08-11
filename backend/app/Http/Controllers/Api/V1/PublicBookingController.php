<?php

namespace App\Http\Controllers\Api\V1;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\V1\StorePublicAppointmentRequest;
use App\Http\Resources\AppointmentResource;
use App\Http\Resources\PublicProfessionalResource;
use App\Models\Appointment;
use App\Models\Client;
use App\Models\Professional;
use App\Models\Service;
use App\Models\User;
use App\Services\AppointmentService;
use App\Services\ProfessionalService;
use App\Services\ProfessionalServiceAssignmentService;
use Carbon\Carbon;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;
use Illuminate\Validation\ValidationException;
use Laravel\Sanctum\PersonalAccessToken;

class PublicBookingController extends Controller
{
    public function __construct(
        private readonly AppointmentService $appointmentService,
        private readonly ProfessionalService $professionalService,
        private readonly ProfessionalServiceAssignmentService $assignmentService,
    ) {}

    public function professionals(Request $request): AnonymousResourceCollection
    {
        $query = Professional::query()
            ->where('is_active', true)
            ->when($request->filled('service_id'), function ($builder) use ($request) {
                $serviceId = $request->integer('service_id');
                $builder->whereHas('services', function ($services) use ($serviceId) {
                    $services->where('services.id', $serviceId)
                        ->where('services.is_active', true);
                });
            })
            ->orderBy('name');

        $professionals = $query->paginate($request->integer('per_page', 12));

        return PublicProfessionalResource::collection($professionals);
    }

    public function showProfessional(Professional $professional): JsonResponse
    {
        if (! $professional->is_active) {
            abort(404);
        }

        return response()->json([
            'data' => new PublicProfessionalResource($professional),
        ]);
    }

    public function serviceProfessionals(Service $service): AnonymousResourceCollection
    {
        if (! $service->is_active) {
            abort(404);
        }

        $professionals = $this->assignmentService->activeProfessionalsForService($service);

        return PublicProfessionalResource::collection($professionals);
    }

    public function availability(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'professional_id' => ['required', 'integer'],
            'service_id' => ['required', 'exists:services,id'],
            'date' => ['required', 'date', 'after_or_equal:today'],
        ]);

        $professional = $this->professionalService->ensureIsActive((int) $validated['professional_id']);

        $service = Service::where('is_active', true)
            ->findOrFail($validated['service_id']);

        $this->assignmentService->ensureOffersService($professional, $service->id);

        $date = Carbon::parse($validated['date'])->startOfDay();
        $slots = $this->appointmentService->getAvailableSlots($professional->id, $service, $date);

        return response()->json([
            'data' => [
                'date' => $date->toDateString(),
                'professional_id' => $professional->id,
                'service_id' => $service->id,
                'slots' => $slots,
            ],
        ]);
    }

    public function store(StorePublicAppointmentRequest $request): JsonResponse
    {
        $service = Service::where('is_active', true)
            ->findOrFail($request->integer('service_id'));

        $professional = $this->professionalService->ensureIsActive($request->integer('professional_id'));
        $this->assignmentService->ensureOffersService($professional, $service->id);

        $startTime = Carbon::parse(
            $request->string('date')->toString().' '.$request->string('time')->toString()
        );
        $endTime = $this->appointmentService->calculateEndTime($startTime, $service);

        $this->appointmentService->ensureNoScheduleConflict(
            $professional->id,
            $startTime,
            $endTime
        );

        $this->appointmentService->ensureWithinProfessionalSchedule(
            $professional->id,
            $startTime,
            $endTime
        );

        $client = $this->resolveClient($request);

        $appointment = Appointment::create([
            'client_id' => $client->id,
            'service_id' => $service->id,
            'professional_id' => $professional->id,
            'start_time' => $startTime,
            'end_time' => $endTime,
            'status' => Appointment::STATUS_PENDING,
            'notes' => $request->input('notes'),
            'total_price' => $service->price,
            'is_active' => true,
        ]);

        return response()->json([
            'message' => 'Agendamento criado com sucesso.',
            'data' => new AppointmentResource($appointment->load(['client', 'service', 'professional'])),
        ], 201);
    }

    private function resolveClient(StorePublicAppointmentRequest $request): Client
    {
        $user = $this->resolveAuthenticatedUser($request);

        if ($user?->isClient()) {
            $client = $user->clientProfile;

            if (! $client) {
                throw ValidationException::withMessages([
                    'client' => 'Perfil de cliente não encontrado.',
                ]);
            }

            return $client;
        }

        $email = $request->string('client_email')->toString();
        $name = $request->string('client_name')->toString();
        $phone = $request->input('client_phone');

        $client = Client::query()->where('email', $email)->first();

        if ($client) {
            $client->update([
                'name' => $name,
                'phone' => $phone ?? $client->phone,
            ]);

            return $client->fresh();
        }

        return Client::create([
            'name' => $name,
            'email' => $email,
            'phone' => $phone,
            'is_active' => true,
        ]);
    }

    private function resolveAuthenticatedUser(Request $request): ?User
    {
        $user = $request->user();
        if ($user) {
            return $user;
        }

        $token = $request->bearerToken();
        if (! $token) {
            return null;
        }

        $accessToken = PersonalAccessToken::findToken($token);
        $tokenable = $accessToken?->tokenable;

        return $tokenable instanceof User ? $tokenable : null;
    }
}

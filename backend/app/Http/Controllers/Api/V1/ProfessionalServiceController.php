<?php

namespace App\Http\Controllers\Api\V1;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\V1\SyncProfessionalServicesRequest;
use App\Http\Resources\ServiceResource;
use App\Models\Professional;
use App\Services\ProfessionalServiceAssignmentService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;

class ProfessionalServiceController extends Controller
{
    public function __construct(
        private readonly ProfessionalServiceAssignmentService $assignmentService,
    ) {}

    public function index(Professional $professional): AnonymousResourceCollection
    {
        $this->authorize('view', $professional);

        $services = $this->assignmentService->listFor($professional);

        return ServiceResource::collection($services);
    }

    public function sync(SyncProfessionalServicesRequest $request, Professional $professional): JsonResponse
    {
        $this->authorize('update', $professional);

        $professional = $this->assignmentService->sync(
            $professional,
            $request->validated('service_ids'),
        );

        return response()->json([
            'message' => 'Serviços do profissional atualizados com sucesso.',
            'data' => [
                'professional_id' => $professional->id,
                'services' => ServiceResource::collection($professional->services),
                'services_count' => $professional->services->count(),
            ],
        ]);
    }
}

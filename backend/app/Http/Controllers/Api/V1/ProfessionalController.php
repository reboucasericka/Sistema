<?php

namespace App\Http\Controllers\Api\V1;

use App\Enums\UserRole;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\V1\StoreProfessionalRequest;
use App\Http\Requests\Api\V1\UpdateProfessionalRequest;
use App\Http\Resources\ProfessionalResource;
use App\Models\Professional;
use App\Services\ProfessionalService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;
use Illuminate\Http\Response;

class ProfessionalController extends Controller
{
    public function __construct(private readonly ProfessionalService $professionalService) {}

    public function index(Request $request): AnonymousResourceCollection
    {
        $this->authorize('viewAny', Professional::class);

        $query = Professional::query()
            ->with('user')
            ->withCount('services')
            ->when($request->filled('is_active'), fn ($q) => $q->where('is_active', $request->boolean('is_active')))
            ->when($request->filled('search'), function ($query) use ($request) {
                $search = $request->string('search')->toString();
                $query->where(function ($builder) use ($search) {
                    $builder->where('name', 'like', "%{$search}%")
                        ->orWhere('specialty', 'like', "%{$search}%")
                        ->orWhere('email', 'like', "%{$search}%");
                });
            })
            ->orderBy('name');

        $user = $request->user();

        if ($user->role === UserRole::Professional) {
            $query->where('user_id', $user->id);
        }

        $professionals = $query->paginate($request->integer('per_page', 15));

        return ProfessionalResource::collection($professionals);
    }

    public function store(StoreProfessionalRequest $request): JsonResponse
    {
        $this->authorize('create', Professional::class);

        $professional = $this->professionalService->create(
            $request->safe()->except(['photo']),
            $request->file('photo'),
        );

        return response()->json([
            'message' => 'Profissional criado com sucesso.',
            'data' => new ProfessionalResource($professional->load('user')),
        ], 201);
    }

    public function show(Professional $professional): JsonResponse
    {
        $this->authorize('view', $professional);

        return response()->json([
            'data' => new ProfessionalResource($professional->load('user')),
        ]);
    }

    public function update(UpdateProfessionalRequest $request, Professional $professional): JsonResponse
    {
        $this->authorize('update', $professional);

        $professional = $this->professionalService->update(
            $professional,
            $request->safe()->except(['photo']),
            $request->file('photo'),
        );

        return response()->json([
            'message' => 'Profissional atualizado com sucesso.',
            'data' => new ProfessionalResource($professional->load('user')),
        ]);
    }

    public function destroy(Professional $professional): Response
    {
        $this->authorize('delete', $professional);

        $this->professionalService->delete($professional);

        return response()->noContent();
    }
}

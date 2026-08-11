<?php

namespace App\Http\Controllers\Api\V1;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\V1\StoreServiceRequest;
use App\Http\Requests\Api\V1\UpdateServiceRequest;
use App\Http\Resources\ServiceResource;
use App\Models\Service;
use App\Models\ServiceCategory;
use App\Services\MediaUploadService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;

class ServiceController extends Controller
{
    public function __construct(private readonly MediaUploadService $media) {}

    public function index(Request $request): AnonymousResourceCollection
    {
        $this->authorize('viewAny', Service::class);

        $services = Service::query()
            ->with('serviceCategory')
            ->when($request->filled('is_active'), fn ($query) => $query->where('is_active', $request->boolean('is_active')))
            ->when($request->filled('service_category_id'), fn ($query) => $query->where('service_category_id', $request->integer('service_category_id')))
            ->when($request->filled('search'), fn ($query) => $query->where('name', 'like', '%'.$request->string('search').'%'))
            ->orderBy('name')
            ->paginate($request->integer('per_page', 15));

        return ServiceResource::collection($services);
    }

    public function store(StoreServiceRequest $request): JsonResponse
    {
        $this->authorize('create', Service::class);

        $data = $this->payloadWithLegacyCategory(
            $request->safe()->except(['image', 'remove_image'])
        );

        if ($request->hasFile('image')) {
            $data['image'] = $this->media->store($request->file('image'), 'services');
        }

        $service = Service::create($data);

        return response()->json([
            'message' => 'Serviço criado com sucesso.',
            'data' => new ServiceResource($service->load('serviceCategory')),
        ], 201);
    }

    public function show(Service $service): JsonResponse
    {
        $this->authorize('view', $service);

        return response()->json([
            'data' => new ServiceResource($service->load('serviceCategory')),
        ]);
    }

    public function update(UpdateServiceRequest $request, Service $service): JsonResponse
    {
        $this->authorize('update', $service);

        $data = $this->payloadWithLegacyCategory(
            $request->safe()->except(['image', 'remove_image']),
            $service,
        );

        $previousImage = $service->image;
        $storedPath = $this->media->applyToPayload(
            $data,
            'image',
            'services',
            $request->file('image'),
            $request->boolean('remove_image'),
        );

        try {
            $service->update($data);
        } catch (\Throwable $e) {
            if ($storedPath) {
                $this->media->delete($storedPath);
            }
            throw $e;
        }

        if (
            ($request->hasFile('image') || $request->boolean('remove_image'))
            && $previousImage
            && $previousImage !== $service->fresh()->image
        ) {
            $this->media->delete($previousImage);
        }

        return response()->json([
            'message' => 'Serviço atualizado com sucesso.',
            'data' => new ServiceResource($service->fresh()->load('serviceCategory')),
        ]);
    }

    public function destroy(Service $service): JsonResponse
    {
        $this->authorize('delete', $service);

        $image = $service->image;
        $service->delete();
        $this->media->delete($image);

        return response()->json([
            'message' => 'Serviço removido com sucesso.',
        ]);
    }

    /**
     * @param  array<string, mixed>  $data
     * @return array<string, mixed>
     */
    private function payloadWithLegacyCategory(array $data, ?Service $existing = null): array
    {
        if (! array_key_exists('service_category_id', $data)) {
            return $data;
        }

        if ($data['service_category_id'] === null) {
            $data['category'] = null;

            return $data;
        }

        $category = ServiceCategory::query()->find($data['service_category_id']);
        $data['category'] = $category?->name ?? $existing?->category;

        return $data;
    }
}

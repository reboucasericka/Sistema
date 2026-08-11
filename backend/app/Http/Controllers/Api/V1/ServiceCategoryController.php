<?php

namespace App\Http\Controllers\Api\V1;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\V1\StoreServiceCategoryRequest;
use App\Http\Requests\Api\V1\UpdateServiceCategoryRequest;
use App\Http\Resources\ServiceCategoryResource;
use App\Models\ServiceCategory;
use App\Services\CategoryService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;
use Illuminate\Http\Response;

class ServiceCategoryController extends Controller
{
    public function __construct(private readonly CategoryService $categoryService) {}

    public function index(Request $request): AnonymousResourceCollection
    {
        $this->authorize('viewAny', ServiceCategory::class);

        $categories = ServiceCategory::query()
            ->withCount('services')
            ->when($request->filled('is_active'), fn ($q) => $q->where('is_active', $request->boolean('is_active')))
            ->when($request->filled('search'), function ($query) use ($request) {
                $search = $request->string('search')->toString();
                $query->where(function ($builder) use ($search) {
                    $builder->where('name', 'like', "%{$search}%")
                        ->orWhere('slug', 'like', "%{$search}%");
                });
            })
            ->orderBy('sort_order')
            ->orderBy('name')
            ->paginate($request->integer('per_page', 50));

        return ServiceCategoryResource::collection($categories);
    }

    public function store(StoreServiceCategoryRequest $request): JsonResponse
    {
        $this->authorize('create', ServiceCategory::class);

        $category = $this->categoryService->create(ServiceCategory::class, $request->validated());

        return response()->json([
            'message' => 'Categoria de serviço criada com sucesso.',
            'data' => new ServiceCategoryResource($category->loadCount('services')),
        ], 201);
    }

    public function show(ServiceCategory $serviceCategory): JsonResponse
    {
        $this->authorize('view', $serviceCategory);

        return response()->json([
            'data' => new ServiceCategoryResource($serviceCategory->loadCount('services')),
        ]);
    }

    public function update(UpdateServiceCategoryRequest $request, ServiceCategory $serviceCategory): JsonResponse
    {
        $this->authorize('update', $serviceCategory);

        $category = $this->categoryService->update($serviceCategory, $request->validated());

        return response()->json([
            'message' => 'Categoria de serviço atualizada com sucesso.',
            'data' => new ServiceCategoryResource($category->loadCount('services')),
        ]);
    }

    public function destroy(ServiceCategory $serviceCategory): Response
    {
        $this->authorize('delete', $serviceCategory);

        $this->categoryService->delete($serviceCategory);

        return response()->noContent();
    }
}

<?php

namespace App\Http\Controllers\Api\V1;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\V1\StoreProductCategoryRequest;
use App\Http\Requests\Api\V1\UpdateProductCategoryRequest;
use App\Http\Resources\ProductCategoryResource;
use App\Models\ProductCategory;
use App\Services\CategoryService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;
use Illuminate\Http\Response;

class ProductCategoryController extends Controller
{
    public function __construct(private readonly CategoryService $categoryService) {}

    public function index(Request $request): AnonymousResourceCollection
    {
        $this->authorize('viewAny', ProductCategory::class);

        $categories = ProductCategory::query()
            ->withCount('products')
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

        return ProductCategoryResource::collection($categories);
    }

    public function store(StoreProductCategoryRequest $request): JsonResponse
    {
        $this->authorize('create', ProductCategory::class);

        $category = $this->categoryService->create(ProductCategory::class, $request->validated());

        return response()->json([
            'message' => 'Categoria de produto criada com sucesso.',
            'data' => new ProductCategoryResource($category->loadCount('products')),
        ], 201);
    }

    public function show(ProductCategory $productCategory): JsonResponse
    {
        $this->authorize('view', $productCategory);

        return response()->json([
            'data' => new ProductCategoryResource($productCategory->loadCount('products')),
        ]);
    }

    public function update(UpdateProductCategoryRequest $request, ProductCategory $productCategory): JsonResponse
    {
        $this->authorize('update', $productCategory);

        $category = $this->categoryService->update($productCategory, $request->validated());

        return response()->json([
            'message' => 'Categoria de produto atualizada com sucesso.',
            'data' => new ProductCategoryResource($category->loadCount('products')),
        ]);
    }

    public function destroy(ProductCategory $productCategory): Response
    {
        $this->authorize('delete', $productCategory);

        $this->categoryService->delete($productCategory);

        return response()->noContent();
    }
}

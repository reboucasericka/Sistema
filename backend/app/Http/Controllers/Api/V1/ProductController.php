<?php

namespace App\Http\Controllers\Api\V1;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\V1\StoreProductRequest;
use App\Http\Requests\Api\V1\UpdateProductRequest;
use App\Http\Resources\ProductResource;
use App\Models\Product;
use App\Models\ProductCategory;
use App\Services\MediaUploadService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;

class ProductController extends Controller
{
    public function __construct(private readonly MediaUploadService $media) {}

    public function index(Request $request): AnonymousResourceCollection
    {
        $this->authorize('viewAny', Product::class);

        $products = Product::query()
            ->with('productCategory')
            ->when($request->filled('search'), function ($query) use ($request) {
                $search = $request->string('search')->toString();
                $query->where(function ($builder) use ($search) {
                    $builder->where('name', 'like', "%{$search}%")
                        ->orWhere('sku', 'like', "%{$search}%")
                        ->orWhere('category', 'like', "%{$search}%")
                        ->orWhereHas('productCategory', fn ($q) => $q->where('name', 'like', "%{$search}%"));
                });
            })
            ->when($request->filled('is_active'), fn ($q) => $q->where('is_active', $request->boolean('is_active')))
            ->when($request->filled('product_category_id'), function ($query) use ($request) {
                $value = $request->input('product_category_id');
                if ($value === 'null' || $value === 'none') {
                    $query->whereNull('product_category_id');

                    return;
                }
                $query->where('product_category_id', (int) $value);
            })
            ->when($request->boolean('low_stock'), fn ($q) => $q->whereColumn('stock_quantity', '<=', 'min_stock'))
            ->orderBy('name')
            ->paginate($request->integer('per_page', 15));

        return ProductResource::collection($products);
    }

    public function store(StoreProductRequest $request): JsonResponse
    {
        $this->authorize('create', Product::class);

        $data = $this->payloadWithLegacyCategory(
            $request->safe()->except(['image', 'remove_image'])
        );

        if ($request->hasFile('image')) {
            $data['image'] = $this->media->store($request->file('image'), 'products');
        }

        $product = Product::create($data);

        return response()->json([
            'message' => 'Produto criado com sucesso.',
            'data' => new ProductResource($product->load('productCategory')),
        ], 201);
    }

    public function show(Product $product): JsonResponse
    {
        $this->authorize('view', $product);

        return response()->json(['data' => new ProductResource($product->load('productCategory'))]);
    }

    public function update(UpdateProductRequest $request, Product $product): JsonResponse
    {
        $this->authorize('update', $product);

        $data = $this->payloadWithLegacyCategory(
            $request->safe()->except(['image', 'remove_image']),
            $product,
        );

        $previousImage = $product->image;
        $storedPath = $this->media->applyToPayload(
            $data,
            'image',
            'products',
            $request->file('image'),
            $request->boolean('remove_image'),
        );

        try {
            $product->update($data);
        } catch (\Throwable $e) {
            if ($storedPath) {
                $this->media->delete($storedPath);
            }
            throw $e;
        }

        if (
            ($request->hasFile('image') || $request->boolean('remove_image'))
            && $previousImage
            && $previousImage !== $product->fresh()->image
        ) {
            $this->media->delete($previousImage);
        }

        return response()->json([
            'message' => 'Produto atualizado com sucesso.',
            'data' => new ProductResource($product->fresh()->load('productCategory')),
        ]);
    }

    public function destroy(Product $product): JsonResponse
    {
        $this->authorize('delete', $product);

        // Desativar: manter imagem no storage.
        $product->update(['is_active' => false]);

        return response()->json(['message' => 'Produto desativado com sucesso.']);
    }

    /**
     * @param  array<string, mixed>  $data
     * @return array<string, mixed>
     */
    private function payloadWithLegacyCategory(array $data, ?Product $existing = null): array
    {
        if (! array_key_exists('product_category_id', $data)) {
            return $data;
        }

        if ($data['product_category_id'] === null) {
            $data['category'] = null;

            return $data;
        }

        $category = ProductCategory::query()->find($data['product_category_id']);
        $data['category'] = $category?->name ?? $existing?->category;

        return $data;
    }
}

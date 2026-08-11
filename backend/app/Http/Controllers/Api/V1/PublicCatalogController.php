<?php

namespace App\Http\Controllers\Api\V1;

use App\Http\Controllers\Controller;
use App\Http\Resources\ProductCategoryResource;
use App\Http\Resources\ProductResource;
use App\Http\Resources\ServiceCategoryResource;
use App\Http\Resources\ServiceResource;
use App\Models\Product;
use App\Models\ProductCategory;
use App\Models\Service;
use App\Models\ServiceCategory;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;

class PublicCatalogController extends Controller
{
    public function services(Request $request): AnonymousResourceCollection
    {
        $query = Service::query()
            ->with('serviceCategory')
            ->where('services.is_active', true);

        if ($search = $request->string('search')->trim()->toString()) {
            $query->where(function ($builder) use ($search) {
                $builder
                    ->where('services.name', 'like', "%{$search}%")
                    ->orWhere('services.description', 'like', "%{$search}%")
                    ->orWhere('services.category', 'like', "%{$search}%")
                    ->orWhereHas('serviceCategory', fn ($q) => $q->where('name', 'like', "%{$search}%"));
            });
        }

        if ($request->filled('service_category_id')) {
            $query->where('services.service_category_id', $request->integer('service_category_id'));
        } elseif ($category = $request->string('category')->trim()->toString()) {
            $query->where(function ($builder) use ($category) {
                $builder->where('services.category', $category)
                    ->orWhereHas('serviceCategory', fn ($q) => $q->where('name', $category)->orWhere('slug', $category));
            });
        }

        if ($request->string('sort')->toString() === 'popular') {
            $query->withCount('appointments')->orderByDesc('appointments_count')->orderBy('services.name');
        } else {
            $query->leftJoin('service_categories', 'services.service_category_id', '=', 'service_categories.id')
                ->orderByRaw('COALESCE(service_categories.sort_order, 9999)')
                ->orderByRaw('COALESCE(service_categories.name, services.category)')
                ->orderBy('services.name')
                ->select('services.*');
        }

        $services = $query->paginate($request->integer('per_page', 12));

        return ServiceResource::collection($services);
    }

    public function products(Request $request): AnonymousResourceCollection
    {
        $query = Product::query()
            ->with('productCategory')
            ->where('is_active', true);

        if ($request->filled('product_category_id')) {
            $query->where('product_category_id', $request->integer('product_category_id'));
        } elseif ($category = $request->string('category')->trim()->toString()) {
            $query->where(function ($builder) use ($category) {
                $builder->where('category', $category)
                    ->orWhereHas('productCategory', fn ($q) => $q->where('name', $category)->orWhere('slug', $category));
            });
        }

        $products = $query->orderBy('name')->paginate($request->integer('per_page', 12));

        return ProductResource::collection($products);
    }

    public function serviceCategories(): AnonymousResourceCollection
    {
        $categories = ServiceCategory::query()
            ->where('is_active', true)
            ->withCount(['services' => fn ($q) => $q->where('is_active', true)])
            ->orderBy('sort_order')
            ->orderBy('name')
            ->get();

        return ServiceCategoryResource::collection($categories);
    }

    public function productCategories(): AnonymousResourceCollection
    {
        $categories = ProductCategory::query()
            ->where('is_active', true)
            ->withCount(['products' => fn ($q) => $q->where('is_active', true)])
            ->orderBy('sort_order')
            ->orderBy('name')
            ->get();

        return ProductCategoryResource::collection($categories);
    }
}

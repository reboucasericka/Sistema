<?php

namespace App\Services;

use App\Models\ProductCategory;
use App\Models\ServiceCategory;
use App\Support\CategorySlug;
use Illuminate\Database\Eloquent\Builder;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Facades\DB;
use Illuminate\Validation\ValidationException;

class CategoryService
{
    /**
     * @param  class-string<ServiceCategory|ProductCategory>  $modelClass
     * @param  array<string, mixed>  $data
     */
    public function create(string $modelClass, array $data): Model
    {
        $data['slug'] = $this->resolveUniqueSlug($modelClass, $data['slug'] ?? null, (string) $data['name']);
        $data['is_active'] = $data['is_active'] ?? true;
        $data['sort_order'] = $data['sort_order'] ?? 0;

        return $modelClass::query()->create($data);
    }

    /**
     * @param  array<string, mixed>  $data
     */
    public function update(Model $category, array $data): Model
    {
        if (array_key_exists('name', $data) && ! array_key_exists('slug', $data) && blank($category->slug)) {
            $data['slug'] = CategorySlug::from((string) $data['name']);
        }

        if (array_key_exists('slug', $data) || (array_key_exists('name', $data) && blank($data['slug'] ?? null))) {
            $name = (string) ($data['name'] ?? $category->getAttribute('name'));
            $slug = $data['slug'] ?? null;
            $data['slug'] = $this->resolveUniqueSlug($category::class, $slug, $name, $category->getKey());
        }

        $category->update($data);

        return $category->fresh();
    }

    public function delete(Model $category): void
    {
        $relation = $category instanceof ServiceCategory ? 'services' : 'products';
        $hasItems = $category->{$relation}()->exists();

        if ($hasItems) {
            throw ValidationException::withMessages([
                'category' => 'Esta categoria possui itens associados e não pode ser eliminada. Reatribua os itens ou desative a categoria.',
            ]);
        }

        DB::transaction(fn () => $category->delete());
    }

    /**
     * @param  class-string<ServiceCategory|ProductCategory>  $modelClass
     */
    public function resolveUniqueSlug(string $modelClass, ?string $slug, string $name, ?int $ignoreId = null): string
    {
        $base = CategorySlug::from($slug ?: $name);
        $candidate = $base;
        $suffix = 1;

        while ($this->slugExists($modelClass, $candidate, $ignoreId)) {
            $candidate = "{$base}-{$suffix}";
            $suffix++;
        }

        return $candidate;
    }

    /**
     * @param  class-string<ServiceCategory|ProductCategory>  $modelClass
     */
    private function slugExists(string $modelClass, string $slug, ?int $ignoreId = null): bool
    {
        /** @var Builder $query */
        $query = $modelClass::query()->where('slug', $slug);

        if ($ignoreId !== null) {
            $query->whereKeyNot($ignoreId);
        }

        return $query->exists();
    }
}

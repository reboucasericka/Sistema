<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Str;

/**
 * Estratégia de transição:
 * 1) Adiciona FKs nullable service_category_id / product_category_id.
 * 2) Mantém as colunas texto `category` intactas.
 * 3) Cria uma categoria por valor distinto não vazio e associa os registos.
 * 4) Valores vazios/null ficam sem categoria (FK null).
 * 5) Remoção dos campos texto fica para uma fase futura.
 */
return new class extends Migration
{
    public function up(): void
    {
        Schema::table('services', function (Blueprint $table) {
            $table->foreignId('service_category_id')
                ->nullable()
                ->after('category')
                ->constrained('service_categories')
                ->nullOnDelete();
        });

        Schema::table('products', function (Blueprint $table) {
            $table->foreignId('product_category_id')
                ->nullable()
                ->after('category')
                ->constrained('product_categories')
                ->nullOnDelete();
        });

        $this->migrateServiceCategories();
        $this->migrateProductCategories();
    }

    public function down(): void
    {
        Schema::table('services', function (Blueprint $table) {
            $table->dropConstrainedForeignId('service_category_id');
        });

        Schema::table('products', function (Blueprint $table) {
            $table->dropConstrainedForeignId('product_category_id');
        });
    }

    private function migrateServiceCategories(): void
    {
        $preferredOrder = [
            'Extensão de Cilios',
            'Tecnicas Semipermanentes',
            'Depilação',
            'Sobrancelha',
            'Depilação Masculina',
            'Beauty Treatment',
        ];

        $names = DB::table('services')
            ->whereNotNull('category')
            ->where('category', '!=', '')
            ->distinct()
            ->orderBy('category')
            ->pluck('category');

        $now = now();
        $slugCounts = [];

        foreach ($names as $index => $name) {
            $name = trim((string) $name);
            if ($name === '') {
                continue;
            }

            $slug = $this->uniqueSlug($name, $slugCounts);
            $sortOrder = array_search($name, $preferredOrder, true);
            $sortOrder = $sortOrder === false ? 100 + $index : $sortOrder;

            $categoryId = DB::table('service_categories')->insertGetId([
                'name' => $name,
                'slug' => $slug,
                'description' => null,
                'is_active' => true,
                'sort_order' => $sortOrder,
                'created_at' => $now,
                'updated_at' => $now,
            ]);

            DB::table('services')
                ->where('category', $name)
                ->update(['service_category_id' => $categoryId]);
        }
    }

    private function migrateProductCategories(): void
    {
        $names = DB::table('products')
            ->whereNotNull('category')
            ->where('category', '!=', '')
            ->distinct()
            ->orderBy('category')
            ->pluck('category');

        $now = now();
        $slugCounts = [];

        foreach ($names as $index => $name) {
            $name = trim((string) $name);
            if ($name === '') {
                continue;
            }

            $slug = $this->uniqueSlug($name, $slugCounts);

            $categoryId = DB::table('product_categories')->insertGetId([
                'name' => $name,
                'slug' => $slug,
                'description' => null,
                'is_active' => true,
                'sort_order' => $index,
                'created_at' => $now,
                'updated_at' => $now,
            ]);

            DB::table('products')
                ->where('category', $name)
                ->update(['product_category_id' => $categoryId]);
        }
    }

    /**
     * @param  array<string, int>  $slugCounts
     */
    private function uniqueSlug(string $name, array &$slugCounts): string
    {
        $base = Str::slug($name) ?: 'categoria';
        $count = $slugCounts[$base] ?? 0;
        $slugCounts[$base] = $count + 1;

        return $count === 0 ? $base : "{$base}-{$count}";
    }
};

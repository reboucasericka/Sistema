<?php

namespace Database\Seeders;

use App\Models\Service;
use App\Models\ServiceCategory;
use App\Support\CategorySlug;
use Illuminate\Database\Seeder;

class BookingServicesSeeder extends Seeder
{
    public function run(): void
    {
        $catalog = require database_path('data/booking_services.php');
        $preferredOrder = [
            'Extensão de Cilios',
            'Tecnicas Semipermanentes',
            'Depilação',
            'Sobrancelha',
            'Depilação Masculina',
            'Beauty Treatment',
        ];

        foreach ($catalog as $item) {
            $sortOrder = array_search($item['category'], $preferredOrder, true);
            $category = ServiceCategory::query()->firstOrCreate(
                ['slug' => CategorySlug::from($item['category'])],
                [
                    'name' => $item['category'],
                    'is_active' => true,
                    'sort_order' => $sortOrder === false ? 100 : $sortOrder,
                ],
            );

            Service::updateOrCreate(
                [
                    'name' => $item['name'],
                    'category' => $item['category'],
                ],
                [
                    'service_category_id' => $category->id,
                    'description' => $item['description'],
                    'price' => $item['price'],
                    'duration_minutes' => $item['duration_minutes'],
                    'is_active' => true,
                ],
            );
        }
    }
}

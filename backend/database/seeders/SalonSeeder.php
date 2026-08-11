<?php

namespace Database\Seeders;

use App\Enums\UserRole;
use App\Models\Appointment;
use App\Models\Client;
use App\Models\Notification;
use App\Models\Product;
use App\Models\ProductCategory;
use App\Models\Professional;
use App\Models\Service;
use App\Models\User;
use App\Support\CategorySlug;
use Carbon\Carbon;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Hash;

class SalonSeeder extends Seeder
{
    public function run(): void
    {
        $admin = User::create([
            'first_name' => 'Admin',
            'last_name' => 'Sistema',
            'email' => 'admin@salon.test',
            'phone' => '+351900000001',
            'role' => UserRole::Admin,
            'password' => Hash::make('password'),
        ]);

        $professionalUser = User::create([
            'first_name' => 'Ana',
            'last_name' => 'Costa',
            'email' => 'ana.costa@salon.test',
            'phone' => '+351900000002',
            'role' => UserRole::Professional,
            'password' => Hash::make('password'),
        ]);

        $clientUser = User::create([
            'first_name' => 'Maria',
            'last_name' => 'Silva',
            'email' => 'maria.silva@salon.test',
            'phone' => '+351900000003',
            'role' => UserRole::Client,
            'password' => Hash::make('password'),
        ]);

        $professional = Professional::create([
            'user_id' => $professionalUser->id,
            'name' => 'Ana Costa',
            'specialty' => 'Cabeleireiro',
            'phone' => '+351900000002',
            'email' => 'ana.costa@salon.test',
            'biography' => 'Especialista em cortes e coloração, com foco em atendimento personalizado.',
            'years_experience' => 8,
            'commission_percentage' => 30,
            'is_active' => true,
        ]);

        $client = Client::create([
            'user_id' => $clientUser->id,
            'name' => 'Maria Silva',
            'email' => 'maria.silva@salon.test',
            'phone' => '+351900000003',
        ]);

        $services = Service::whereIn('name', ['Corte Feminino', 'Coloração', 'Manicure'])->get();
        foreach ($services as $legacy) {
            $legacy->delete();
        }

        $this->call(BookingServicesSeeder::class);

        $services = Service::query()->orderBy('id')->get();

        $professional->services()->sync($services->where('is_active', true)->pluck('id')->all());

        $hairCategory = ProductCategory::query()->firstOrCreate(
            ['slug' => CategorySlug::from('Cabelo')],
            ['name' => 'Cabelo', 'is_active' => true, 'sort_order' => 0],
        );
        $nailsCategory = ProductCategory::query()->firstOrCreate(
            ['slug' => CategorySlug::from('Unhas')],
            ['name' => 'Unhas', 'is_active' => true, 'sort_order' => 1],
        );

        Product::insert([
            [
                'name' => 'Shampoo Hidratante',
                'description' => 'Shampoo profissional 500ml',
                'sku' => 'SHP-001',
                'category' => 'Cabelo',
                'product_category_id' => $hairCategory->id,
                'price' => 18.50,
                'cost_price' => 9.00,
                'stock_quantity' => 25,
                'min_stock' => 5,
                'image' => '/images/services/product_1.png',
                'is_active' => true,
                'created_at' => now(),
                'updated_at' => now(),
            ],
            [
                'name' => 'Máscara Capilar',
                'description' => 'Tratamento intensivo',
                'sku' => 'MSK-002',
                'category' => 'Cabelo',
                'product_category_id' => $hairCategory->id,
                'price' => 24.90,
                'cost_price' => 12.00,
                'stock_quantity' => 3,
                'min_stock' => 5,
                'image' => '/images/services/product_2.png',
                'is_active' => true,
                'created_at' => now(),
                'updated_at' => now(),
            ],
            [
                'name' => 'Esmalte Premium',
                'description' => 'Verniz de longa duração',
                'sku' => 'ESM-003',
                'category' => 'Unhas',
                'product_category_id' => $nailsCategory->id,
                'price' => 8.50,
                'cost_price' => 3.50,
                'stock_quantity' => 40,
                'min_stock' => 10,
                'image' => '/images/services/product_3.png',
                'is_active' => true,
                'created_at' => now(),
                'updated_at' => now(),
            ],
        ]);

        $start = Carbon::tomorrow()->setTime(10, 0);
        $service = $services->first();

        Appointment::create([
            'client_id' => $client->id,
            'service_id' => $service->id,
            'professional_id' => $professional->id,
            'start_time' => $start,
            'end_time' => $start->copy()->addMinutes($service->duration_minutes),
            'status' => Appointment::STATUS_CONFIRMED,
            'total_price' => $service->price,
            'notes' => 'Primeira visita',
        ]);

        foreach ([$admin, $professionalUser, $clientUser] as $user) {
            Notification::create([
                'user_id' => $user->id,
                'title' => 'Bem-vindo ao Salon Management System',
                'body' => 'A sua conta foi configurada com sucesso.',
                'type' => 'info',
            ]);
        }

        $this->call(ProfessionalScheduleSeeder::class);
    }
}

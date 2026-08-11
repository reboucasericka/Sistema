<?php

namespace App\Http\Controllers\Api\V1;

use App\Enums\UserRole;
use App\Http\Controllers\Controller;
use App\Http\Resources\AppointmentResource;
use App\Http\Resources\ProductResource;
use App\Models\Appointment;
use App\Models\Client;
use App\Models\Product;
use App\Models\Professional;
use App\Models\Service;
use App\Models\User;
use App\Services\CashService;
use App\Services\ProductService;
use App\Services\SaleService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class DashboardController extends Controller
{
    public function __construct(
        private readonly ProductService $productService,
        private readonly SaleService $saleService,
        private readonly CashService $cashService
    ) {}

    public function index(Request $request): JsonResponse
    {
        $user = $request->user();

        if ($user->role === UserRole::Client) {
            $clientId = $user->clientProfile?->id;

            return response()->json([
                'data' => [
                    'upcoming_appointments' => Appointment::query()
                        ->where('client_id', $clientId)
                        ->where('start_time', '>=', now())
                        ->whereNot('status', Appointment::STATUS_CANCELED)
                        ->count(),
                    'unread_notifications' => $user->notifications()->whereNull('read_at')->count(),
                ],
            ]);
        }

        $saleStats = $this->saleService->todayStats();

        $data = [
            'totals' => [
                'users' => User::count(),
                'clients' => Client::count(),
                'professionals' => Professional::where('is_active', true)->count(),
                'services' => Service::where('is_active', true)->count(),
                'products' => Product::where('is_active', true)->count(),
                'products_low_stock' => $this->productService->lowStockCount(),
                'appointments_today' => Appointment::whereDate('start_time', today())->count(),
                'appointments_pending' => Appointment::where('status', Appointment::STATUS_PENDING)->count(),
                'sales_today' => $saleStats['sales_count'],
                'revenue_today' => $saleStats['revenue'],
            ],
            'low_stock_products' => ProductResource::collection(
                $this->productService->lowStockProducts(5)
            ),
            'cash_register' => $this->cashService->currentSummary(),
            'recent_appointments' => AppointmentResource::collection(
                Appointment::query()
                    ->with(['client', 'service', 'professional'])
                    ->where('start_time', '>=', now()->startOfDay())
                    ->whereNot('status', Appointment::STATUS_CANCELED)
                    ->orderBy('start_time')
                    ->limit(8)
                    ->get()
            ),
            'unread_notifications' => $user->notifications()->whereNull('read_at')->count(),
        ];

        if ($user->role === UserRole::Professional) {
            $professionalId = $user->professionalProfile?->id;
            $data['professional'] = [
                'appointments_today' => Appointment::where('professional_id', $professionalId)
                    ->whereDate('start_time', today())
                    ->count(),
                'upcoming_appointments' => Appointment::where('professional_id', $professionalId)
                    ->where('start_time', '>=', now())
                    ->whereNot('status', Appointment::STATUS_CANCELED)
                    ->count(),
            ];
        }

        return response()->json(['data' => $data]);
    }
}

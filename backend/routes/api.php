<?php

use App\Http\Controllers\Api\V1\AppointmentController;
use App\Http\Controllers\Api\V1\AuthController;
use App\Http\Controllers\Api\V1\CashController;
use App\Http\Controllers\Api\V1\ClientController;
use App\Http\Controllers\Api\V1\ContactMessageController;
use App\Http\Controllers\Api\V1\DashboardController;
use App\Http\Controllers\Api\V1\NotificationController;
use App\Http\Controllers\Api\V1\ProductCategoryController;
use App\Http\Controllers\Api\V1\ProductController;
use App\Http\Controllers\Api\V1\ProfessionalController;
use App\Http\Controllers\Api\V1\ProfessionalScheduleController;
use App\Http\Controllers\Api\V1\ProfessionalServiceController;
use App\Http\Controllers\Api\V1\PublicBookingController;
use App\Http\Controllers\Api\V1\PublicCatalogController;
use App\Http\Controllers\Api\V1\SaleController;
use App\Http\Controllers\Api\V1\ServiceCategoryController;
use App\Http\Controllers\Api\V1\ServiceController;
use App\Http\Controllers\Api\V1\StockController;
use App\Http\Controllers\Api\V1\StockMovementController;
use App\Http\Controllers\Api\V1\UserController;
use Illuminate\Support\Facades\Route;

Route::prefix('v1')->group(function () {
    Route::prefix('public')->group(function () {
        Route::get('services', [PublicCatalogController::class, 'services']);
        Route::get('products', [PublicCatalogController::class, 'products']);
        Route::get('service-categories', [PublicCatalogController::class, 'serviceCategories']);
        Route::get('product-categories', [PublicCatalogController::class, 'productCategories']);
        Route::get('professionals', [PublicBookingController::class, 'professionals']);
        Route::get('professionals/{professional}', [PublicBookingController::class, 'showProfessional']);
        Route::get('services/{service}/professionals', [PublicBookingController::class, 'serviceProfessionals']);
        Route::get('availability', [PublicBookingController::class, 'availability']);
        Route::post('appointments', [PublicBookingController::class, 'store']);
        Route::post('contact', [ContactMessageController::class, 'store']);
    });

    Route::prefix('auth')->group(function () {
        Route::post('register', [AuthController::class, 'register']);
        Route::post('login', [AuthController::class, 'login']);

        Route::middleware('auth:sanctum')->group(function () {
            Route::get('me', [AuthController::class, 'me']);
            Route::post('logout', [AuthController::class, 'logout']);
        });
    });

    Route::middleware('auth:sanctum')->group(function () {
        Route::get('dashboard', [DashboardController::class, 'index']);

        Route::middleware('role:admin')->group(function () {
            Route::apiResource('users', UserController::class);
            Route::apiResource('service-categories', ServiceCategoryController::class);
            Route::apiResource('product-categories', ProductCategoryController::class);
            Route::get('contact-messages', [ContactMessageController::class, 'index']);
            Route::patch('contact-messages/{contactMessage}/read', [ContactMessageController::class, 'markAsRead']);
        });

        Route::apiResource('clients', ClientController::class);
        Route::apiResource('professionals', ProfessionalController::class);
        Route::get('professionals/{professional}/services', [ProfessionalServiceController::class, 'index']);
        Route::put('professionals/{professional}/services', [ProfessionalServiceController::class, 'sync']);
        Route::apiResource('services', ServiceController::class);
        Route::apiResource('appointments', AppointmentController::class);

        Route::middleware('role:admin,professional')->group(function () {
            Route::get('professional-schedules', [ProfessionalScheduleController::class, 'index']);
            Route::get('professional-schedules/{professional_schedule}', [ProfessionalScheduleController::class, 'show']);
        });

        Route::middleware('role:admin')->group(function () {
            Route::post('professional-schedules', [ProfessionalScheduleController::class, 'store']);
            Route::put('professional-schedules/{professional_schedule}', [ProfessionalScheduleController::class, 'update']);
            Route::patch('professional-schedules/{professional_schedule}', [ProfessionalScheduleController::class, 'update']);
            Route::delete('professional-schedules/{professional_schedule}', [ProfessionalScheduleController::class, 'destroy']);
        });

        Route::middleware('role:admin,professional')->group(function () {
            Route::apiResource('products', ProductController::class);
            Route::prefix('stock')->group(function () {
                Route::post('entry', [StockController::class, 'entry']);
                Route::post('exit', [StockController::class, 'exit']);
                Route::post('adjustment', [StockController::class, 'adjustment']);
                Route::get('history', [StockController::class, 'history']);
                Route::get('low', [StockController::class, 'lowStock']);
            });
            Route::get('stock-movements', [StockMovementController::class, 'index']);
            Route::post('products/{product}/stock/in', [StockMovementController::class, 'stockIn']);
            Route::post('products/{product}/stock/out', [StockMovementController::class, 'stockOut']);
            Route::post('products/{product}/stock/adjust', [StockMovementController::class, 'adjust']);
            Route::prefix('cash')->group(function () {
                Route::post('open', [CashController::class, 'open']);
                Route::post('close', [CashController::class, 'close']);
                Route::post('income', [CashController::class, 'income']);
                Route::post('expense', [CashController::class, 'expense']);
                Route::get('current', [CashController::class, 'current']);
                Route::get('report', [CashController::class, 'report']);
            });
            Route::get('sales', [SaleController::class, 'index']);
            Route::post('sales', [SaleController::class, 'store']);
            Route::get('sales/{sale}', [SaleController::class, 'show']);
            Route::post('sales/{sale}/cancel', [SaleController::class, 'cancel']);
        });

        Route::get('notifications', [NotificationController::class, 'index']);
        Route::patch('notifications/{notification}/read', [NotificationController::class, 'markAsRead']);
        Route::delete('notifications/{notification}', [NotificationController::class, 'destroy']);
    });
});

<?php

namespace App\Models;

use App\Services\MediaUploadService;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\BelongsToMany;
use Illuminate\Database\Eloquent\Relations\HasMany;

class Service extends Model
{
    use HasFactory;

    protected $fillable = [
        'name',
        'category',
        'service_category_id',
        'description',
        'image',
        'price',
        'duration_minutes',
        'is_active',
    ];

    protected function casts(): array
    {
        return [
            'price' => 'decimal:2',
            'duration_minutes' => 'integer',
            'is_active' => 'boolean',
        ];
    }

    public function serviceCategory(): BelongsTo
    {
        return $this->belongsTo(ServiceCategory::class);
    }

    public function appointments(): HasMany
    {
        return $this->hasMany(Appointment::class);
    }

    public function professionals(): BelongsToMany
    {
        return $this->belongsToMany(Professional::class)
            ->withTimestamps();
    }

    public function categoryLabel(): ?string
    {
        return $this->serviceCategory?->name ?? $this->category;
    }

    public function imageUrl(): ?string
    {
        return app(MediaUploadService::class)->url($this->image);
    }
}

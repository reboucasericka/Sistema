<?php

namespace App\Models;

use App\Services\MediaUploadService;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\BelongsToMany;
use Illuminate\Database\Eloquent\Relations\HasMany;

class Professional extends Model
{
    /** @use HasFactory<\Database\Factories\ProfessionalFactory> */
    use HasFactory;

    protected $fillable = [
        'user_id',
        'name',
        'specialty',
        'photo',
        'biography',
        'phone',
        'email',
        'instagram',
        'facebook',
        'years_experience',
        'commission_percentage',
        'is_active',
    ];

    protected function casts(): array
    {
        return [
            'commission_percentage' => 'decimal:2',
            'years_experience' => 'integer',
            'is_active' => 'boolean',
        ];
    }

    public function user(): BelongsTo
    {
        return $this->belongsTo(User::class);
    }

    public function appointments(): HasMany
    {
        return $this->hasMany(Appointment::class);
    }

    public function schedules(): HasMany
    {
        return $this->hasMany(ProfessionalSchedule::class);
    }

    public function services(): BelongsToMany
    {
        return $this->belongsToMany(Service::class)
            ->withTimestamps();
    }

    public function imageUrl(): ?string
    {
        return app(MediaUploadService::class)->url($this->photo);
    }
}

<?php

namespace App\Models;

use App\Enums\DayOfWeek;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class ProfessionalSchedule extends Model
{
    /** @use HasFactory<\Database\Factories\ProfessionalScheduleFactory> */
    use HasFactory;

    protected $fillable = [
        'professional_id',
        'day_of_week',
        'start_time',
        'end_time',
    ];

    protected function casts(): array
    {
        return [
            'day_of_week' => DayOfWeek::class,
        ];
    }

    public function professional(): BelongsTo
    {
        return $this->belongsTo(Professional::class);
    }

    public function creator(): BelongsTo
    {
        return $this->belongsTo(User::class, 'created_by');
    }

    public function startTimeHi(): string
    {
        return $this->formatTimeValue($this->attributes['start_time'] ?? $this->start_time);
    }

    public function endTimeHi(): string
    {
        return $this->formatTimeValue($this->attributes['end_time'] ?? $this->end_time);
    }

    private function formatTimeValue(mixed $value): string
    {
        if ($value instanceof \DateTimeInterface) {
            return $value->format('H:i');
        }

        $raw = (string) $value;

        return strlen($raw) >= 5 ? substr($raw, 0, 5) : $raw;
    }
}

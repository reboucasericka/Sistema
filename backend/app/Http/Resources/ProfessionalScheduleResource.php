<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

/** @mixin \App\Models\ProfessionalSchedule */
class ProfessionalScheduleResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        return [
            'id' => $this->id,
            'professional_id' => $this->professional_id,
            'professional' => new ProfessionalResource($this->whenLoaded('professional')),
            'day_of_week' => $this->day_of_week->value,
            'day_name' => $this->day_of_week->label(),
            'start_time' => $this->startTimeHi(),
            'end_time' => $this->endTimeHi(),
            'created_by' => $this->created_by,
            'creator' => new UserResource($this->whenLoaded('creator')),
            'created_at' => $this->created_at?->toIso8601String(),
            'updated_at' => $this->updated_at?->toIso8601String(),
        ];
    }
}

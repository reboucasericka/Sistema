<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

/** @mixin \App\Models\Professional */
class ProfessionalResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        return [
            'id' => $this->id,
            'user_id' => $this->user_id,
            'name' => $this->name,
            'specialty' => $this->specialty,
            'photo' => $this->photo,
            'image_url' => $this->imageUrl(),
            'biography' => $this->biography,
            'phone' => $this->phone,
            'email' => $this->email,
            'instagram' => $this->instagram,
            'facebook' => $this->facebook,
            'years_experience' => $this->years_experience,
            'commission_percentage' => $this->commission_percentage,
            'is_active' => $this->is_active,
            'services_count' => $this->whenCounted('services'),
            'services' => ServiceResource::collection($this->whenLoaded('services')),
            'user' => $this->when(
                $this->relationLoaded('user') && $this->user,
                fn () => new UserResource($this->user),
            ),
            'created_at' => $this->created_at?->toIso8601String(),
            'updated_at' => $this->updated_at?->toIso8601String(),
        ];
    }
}

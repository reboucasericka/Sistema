<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

/** @mixin \App\Models\Professional */
class PublicProfessionalResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        return [
            'id' => $this->id,
            'name' => $this->name,
            'specialty' => $this->specialty,
            'biography' => $this->biography,
            'photo' => $this->photo,
            'image_url' => $this->imageUrl(),
            'phone' => $this->phone,
            'email' => $this->email,
            'instagram' => $this->instagram,
            'facebook' => $this->facebook,
            'years_experience' => $this->years_experience,
        ];
    }
}

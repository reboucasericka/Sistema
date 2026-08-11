<?php

namespace App\Http\Requests\Api\V1;

use App\Support\ImageUploadRules;
use Illuminate\Foundation\Http\FormRequest;

class UpdateProfessionalRequest extends FormRequest
{
    public function authorize(): bool
    {
        return true;
    }

    public function rules(): array
    {
        return [
            'user_id' => ['sometimes', 'nullable', 'integer', 'exists:users,id'],
            'name' => ['sometimes', 'string', 'max:100'],
            'specialty' => ['sometimes', 'string', 'max:100'],
            'photo' => ImageUploadRules::file('photo'),
            'remove_photo' => ['sometimes', 'boolean'],
            'biography' => ['nullable', 'string', 'max:2000'],
            'phone' => ['nullable', 'string', 'max:20'],
            'email' => ['nullable', 'email', 'max:255'],
            'instagram' => ['nullable', 'string', 'max:255'],
            'facebook' => ['nullable', 'string', 'max:255'],
            'years_experience' => ['nullable', 'integer', 'min:0', 'max:80'],
            'commission_percentage' => ['nullable', 'numeric', 'min:0', 'max:100'],
            'is_active' => ['sometimes', 'boolean'],
        ];
    }

    public function messages(): array
    {
        return ImageUploadRules::messages('photo');
    }
}

<?php

namespace App\Http\Requests\Api\V1;

use Illuminate\Foundation\Http\FormRequest;
use Illuminate\Validation\Rule;

class SyncProfessionalServicesRequest extends FormRequest
{
    public function authorize(): bool
    {
        return true;
    }

    public function rules(): array
    {
        return [
            'service_ids' => ['required', 'array'],
            'service_ids.*' => [
                'integer',
                'distinct',
                Rule::exists('services', 'id')->where(fn ($query) => $query->where('is_active', true)),
            ],
        ];
    }

    public function messages(): array
    {
        return [
            'service_ids.required' => 'A lista de serviços é obrigatória.',
            'service_ids.*.exists' => 'Um ou mais serviços são inválidos ou estão inativos.',
            'service_ids.*.distinct' => 'A lista de serviços não pode conter duplicados.',
        ];
    }
}

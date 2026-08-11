<?php

namespace App\Http\Requests\Api\V1;

use Illuminate\Foundation\Http\FormRequest;

class StorePublicAppointmentRequest extends FormRequest
{
    public function authorize(): bool
    {
        return true;
    }

    public function rules(): array
    {
        $guestRules = $this->user() ? [] : [
            'client_name' => ['required', 'string', 'max:200'],
            'client_email' => ['required', 'email', 'max:255'],
            'client_phone' => ['nullable', 'string', 'max:20'],
        ];

        return [
            'service_id' => ['required', 'exists:services,id'],
            'professional_id' => ['required', 'exists:professionals,id'],
            'date' => ['required', 'date', 'after_or_equal:today'],
            'time' => ['required', 'date_format:H:i'],
            'notes' => ['nullable', 'string', 'max:500'],
            ...$guestRules,
        ];
    }
}

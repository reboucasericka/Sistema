<?php

namespace App\Http\Requests\Api\V1;

use Illuminate\Foundation\Http\FormRequest;
use Illuminate\Validation\Rule;

class UpdateProfessionalScheduleRequest extends FormRequest
{
    public function authorize(): bool
    {
        return true;
    }

    public function rules(): array
    {
        return [
            'professional_id' => [
                'sometimes',
                'integer',
                Rule::exists('professionals', 'id')->where(fn ($query) => $query->where('is_active', true)),
            ],
            'day_of_week' => ['sometimes', 'integer', 'between:0,6'],
            'start_time' => ['sometimes', 'date_format:H:i'],
            'end_time' => ['sometimes', 'date_format:H:i', 'after:start_time'],
            'created_by' => ['prohibited'],
        ];
    }

    public function withValidator($validator): void
    {
        $validator->after(function ($validator) {
            $start = $this->input('start_time', $this->route('professional_schedule')?->startTimeHi());
            $end = $this->input('end_time', $this->route('professional_schedule')?->endTimeHi());

            if ($start && $end && $end <= $start) {
                $validator->errors()->add('end_time', 'A hora de fim deve ser posterior à hora de início.');
            }
        });
    }
}

<?php

namespace App\Support;

final class ImageUploadRules
{
    /**
     * @return list<string|\Illuminate\Contracts\Validation\ValidationRule>
     */
    public static function file(string $field = 'image'): array
    {
        return ['nullable', 'image', 'mimes:jpeg,jpg,png,webp', 'max:2048'];
    }

    /**
     * @return array<string, string>
     */
    public static function messages(string $field = 'image'): array
    {
        return [
            "{$field}.image" => 'O ficheiro deve ser uma imagem válida.',
            "{$field}.mimes" => 'A imagem deve ser JPG, PNG ou WebP.',
            "{$field}.max" => 'A imagem não pode ultrapassar 2 MB.',
        ];
    }
}

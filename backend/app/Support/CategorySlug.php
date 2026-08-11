<?php

namespace App\Support;

use Illuminate\Support\Str;

class CategorySlug
{
    public static function from(string $name, ?string $fallback = 'categoria'): string
    {
        $slug = Str::slug(trim($name));

        return $slug !== '' ? $slug : ($fallback ?? 'categoria');
    }
}

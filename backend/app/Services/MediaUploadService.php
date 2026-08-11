<?php

namespace App\Services;

use Illuminate\Http\UploadedFile;
use Illuminate\Support\Facades\Storage;
use Illuminate\Support\Str;

class MediaUploadService
{
    private const DISK = 'public';

    private const ALLOWED_EXTENSIONS = ['jpg', 'jpeg', 'png', 'webp'];

    /**
     * Guarda a imagem e devolve o path relativo (ex.: professionals/uuid.jpg).
     */
    public function store(UploadedFile $file, string $directory): string
    {
        $extension = strtolower($file->getClientOriginalExtension() ?: $file->extension() ?: 'jpg');

        if (! in_array($extension, self::ALLOWED_EXTENSIONS, true)) {
            $extension = 'jpg';
        }

        $filename = Str::uuid()->toString().'.'.$extension;

        return $file->storeAs($directory, $filename, self::DISK);
    }

    public function url(?string $path): ?string
    {
        if ($path === null || trim($path) === '') {
            return null;
        }

        // URLs absolutas ou caminhos públicos legados (/images/...)
        if (
            str_starts_with($path, 'http://')
            || str_starts_with($path, 'https://')
            || str_starts_with($path, '/')
        ) {
            return $path;
        }

        return Storage::disk(self::DISK)->url($path);
    }

    public function isManaged(?string $path): bool
    {
        if ($path === null || trim($path) === '') {
            return false;
        }

        if (
            str_starts_with($path, 'http://')
            || str_starts_with($path, 'https://')
            || str_starts_with($path, '/')
        ) {
            return false;
        }

        return Storage::disk(self::DISK)->exists($path);
    }

    public function delete(?string $path): void
    {
        if ($this->isManaged($path)) {
            Storage::disk(self::DISK)->delete($path);
        }
    }

    /**
     * Aplica upload ou remoção no array de dados.
     * Devolve o path novo guardado (para rollback) ou null.
     *
     * @param  array<string, mixed>  $data
     */
    public function applyToPayload(
        array &$data,
        string $field,
        string $directory,
        ?UploadedFile $file,
        bool $remove,
    ): ?string {
        if ($file) {
            $path = $this->store($file, $directory);
            $data[$field] = $path;

            return $path;
        }

        if ($remove) {
            $data[$field] = null;
        }

        return null;
    }
}

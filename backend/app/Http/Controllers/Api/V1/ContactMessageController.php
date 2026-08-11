<?php

namespace App\Http\Controllers\Api\V1;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\V1\StoreContactMessageRequest;
use App\Http\Resources\ContactMessageResource;
use App\Models\ContactMessage;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;

class ContactMessageController extends Controller
{
    public function store(StoreContactMessageRequest $request): JsonResponse
    {
        $message = ContactMessage::create($request->validated());

        return response()->json([
            'message' => 'Mensagem enviada com sucesso. Entraremos em contacto em breve.',
            'data' => new ContactMessageResource($message),
        ], 201);
    }

    public function index(Request $request): AnonymousResourceCollection
    {
        $this->authorize('viewAny', ContactMessage::class);

        $messages = ContactMessage::query()
            ->when($request->boolean('unread'), fn ($q) => $q->where('is_read', false))
            ->orderByDesc('created_at')
            ->paginate($request->integer('per_page', 15));

        return ContactMessageResource::collection($messages);
    }

    public function markAsRead(ContactMessage $contactMessage): JsonResponse
    {
        $this->authorize('update', $contactMessage);

        $contactMessage->update(['is_read' => true]);

        return response()->json([
            'message' => 'Mensagem marcada como lida.',
            'data' => new ContactMessageResource($contactMessage),
        ]);
    }
}

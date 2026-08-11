<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AppBadge from '@/components/ui/AppBadge.vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppCard from '@/components/ui/AppCard.vue'
import AppPagination from '@/components/ui/AppPagination.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingState from '@/components/ui/LoadingState.vue'
import { getErrorMessage } from '@/lib/api'
import { fetchContactMessages, markContactMessageRead } from '@/services/salonApi'
import type { ContactMessage } from '@/types/api'
import { useUiStore } from '@/stores/ui'

const ui = useUiStore()
const messages = ref<ContactMessage[]>([])
const loading = ref(true)
const page = ref(1)
const lastPage = ref(1)
const unreadOnly = ref(false)

async function load() {
  loading.value = true
  try {
    const response = await fetchContactMessages({
      page: page.value,
      per_page: 15,
      unread: unreadOnly.value ? 1 : undefined,
    })
    messages.value = response.data ?? []
    lastPage.value = response.meta?.last_page ?? 1
  } catch (err) {
    ui.error(getErrorMessage(err))
  } finally {
    loading.value = false
  }
}

async function markRead(message: ContactMessage) {
  if (message.is_read) return
  try {
    await markContactMessageRead(message.id)
    message.is_read = true
    ui.success('Mensagem marcada como lida.')
  } catch (err) {
    ui.error(getErrorMessage(err))
  }
}

function formatDate(value: string) {
  return new Date(value).toLocaleString('pt-PT')
}

onMounted(load)
</script>

<template>
  <div class="w-full space-y-4">
    <div class="flex flex-wrap items-center gap-3">
      <label class="flex items-center gap-2 text-sm text-zinc-600">
        <input v-model="unreadOnly" type="checkbox" @change="load" />
        Apenas não lidas
      </label>
      <AppButton variant="secondary" @click="load">Atualizar</AppButton>
    </div>

    <LoadingState v-if="loading" />
    <EmptyState v-else-if="!messages.length" title="Sem mensagens" description="Nenhuma mensagem de contacto recebida." />

    <div v-else class="space-y-3">
      <AppCard v-for="message in messages" :key="message.id" class="p-4">
        <div class="flex flex-wrap items-start justify-between gap-3">
          <div>
            <div class="flex items-center gap-2">
              <h3 class="font-semibold text-zinc-900">{{ message.name }}</h3>
              <AppBadge :tone="message.is_read ? 'default' : 'warning'">
                {{ message.is_read ? 'Lida' : 'Nova' }}
              </AppBadge>
            </div>
            <p class="text-sm text-zinc-500">{{ message.email }} · {{ formatDate(message.created_at) }}</p>
            <p v-if="message.phone" class="text-sm text-zinc-600">{{ message.phone }}</p>
            <p v-if="message.subject" class="mt-2 text-sm font-medium text-zinc-800">{{ message.subject }}</p>
            <p class="mt-2 whitespace-pre-wrap text-sm text-zinc-700">{{ message.message }}</p>
          </div>
          <AppButton
            v-if="!message.is_read"
            variant="secondary"
            @click="markRead(message)"
          >
            Marcar lida
          </AppButton>
        </div>
      </AppCard>
    </div>

    <AppPagination
      v-if="lastPage > 1"
      :current-page="page"
      :last-page="lastPage"
      @change="(p) => { page = p; load() }"
    />
  </div>
</template>

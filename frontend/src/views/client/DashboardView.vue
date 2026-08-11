<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import AppButton from '@/components/ui/AppButton.vue'
import AppCard from '@/components/ui/AppCard.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingState from '@/components/ui/LoadingState.vue'
import StatCard from '@/components/ui/StatCard.vue'
import { IMAGES } from '@/lib/images'
import { fetchDashboard, fetchNotifications, markNotificationRead } from '@/services/salonApi'
import type { DashboardData, Notification } from '@/types/api'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'

const auth = useAuthStore()
const ui = useUiStore()
const dashboard = ref<DashboardData | null>(null)
const notifications = ref<Notification[]>([])
const loading = ref(true)

async function load() {
  loading.value = true
  try {
    const [dash, notifs] = await Promise.all([fetchDashboard(), fetchNotifications(true)])
    dashboard.value = dash
    notifications.value = notifs.data ?? []
  } catch {
    ui.error('Não foi possível carregar os dados.')
  } finally {
    loading.value = false
  }
}

async function readNotification(id: number) {
  await markNotificationRead(id)
  ui.success('Notificação lida.')
  await load()
}

onMounted(load)
</script>

<template>
  <LoadingState v-if="loading" />

  <div v-else class="space-y-6">
    <div class="overflow-hidden rounded-2xl border border-zinc-200 bg-white shadow-sm">
      <div class="grid md:grid-cols-5">
        <div class="p-6 md:col-span-3">
          <p class="text-sm text-brand-600">Olá, {{ auth.fullName }}</p>
          <h2 class="mt-2 text-2xl font-bold text-zinc-900">A sua área no salão</h2>
          <p class="mt-2 text-sm text-zinc-600">
            Consulte agendamentos, confirme horários e acompanhe notificações.
          </p>
          <RouterLink to="/panel/client/appointments" class="mt-4 inline-block">
            <AppButton>Ver meus agendamentos</AppButton>
          </RouterLink>
        </div>
        <img
          :src="IMAGES.logoBar"
          alt="Salon"
          class="h-40 w-full object-contain bg-brand-50 p-6 md:col-span-2 md:h-auto"
        />
      </div>
    </div>

    <div class="grid gap-4 sm:grid-cols-2">
      <StatCard label="Próximos agendamentos" :value="dashboard?.upcoming_appointments ?? 0" />
      <StatCard label="Notificações por ler" :value="dashboard?.unread_notifications ?? 0" />
    </div>

    <AppCard title="Notificações">
      <EmptyState v-if="!notifications.length" title="Tudo em dia" description="Sem notificações por ler." />
      <ul v-else class="divide-y divide-zinc-100">
        <li
          v-for="item in notifications"
          :key="item.id"
          class="flex flex-col gap-2 py-3 sm:flex-row sm:items-center sm:justify-between"
        >
          <div>
            <p class="font-medium">{{ item.title }}</p>
            <p class="text-sm text-zinc-500">{{ item.body }}</p>
          </div>
          <button
            type="button"
            class="text-sm font-medium text-brand-700 hover:underline"
            @click="readNotification(item.id)"
          >
            Marcar como lida
          </button>
        </li>
      </ul>
    </AppCard>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AppBadge from '@/components/ui/AppBadge.vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppCard from '@/components/ui/AppCard.vue'
import AppPagination from '@/components/ui/AppPagination.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingState from '@/components/ui/LoadingState.vue'
import { getErrorMessage } from '@/lib/api'
import { serviceImage } from '@/lib/images'
import { cancelAppointment, fetchAppointments } from '@/services/salonApi'
import type { Appointment } from '@/types/api'
import { useUiStore } from '@/stores/ui'

const ui = useUiStore()
const appointments = ref<Appointment[]>([])
const loading = ref(true)
const page = ref(1)
const lastPage = ref(1)
const total = ref(0)

function statusTone(status: string) {
  if (status === 'confirmed') return 'success'
  if (status === 'pending') return 'warning'
  if (status === 'canceled') return 'danger'
  return 'default'
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('pt-PT', { dateStyle: 'full', timeStyle: 'short' }).format(new Date(value))
}

async function load() {
  loading.value = true
  try {
    const response = await fetchAppointments({ page: page.value, per_page: 8 })
    appointments.value = response.data ?? []
    lastPage.value = response.meta?.last_page ?? 1
    total.value = response.meta?.total ?? appointments.value.length
  } catch (err) {
    ui.error(getErrorMessage(err))
  } finally {
    loading.value = false
  }
}

async function cancel(item: Appointment) {
  const ok = await ui.confirm({
    title: 'Cancelar agendamento',
    message: 'Tem a certeza que deseja cancelar este agendamento?',
    confirmLabel: 'Sim, cancelar',
    variant: 'danger',
  })
  if (!ok) return
  try {
    await cancelAppointment(item.id)
    ui.success('Agendamento cancelado.')
    await load()
  } catch (err) {
    ui.error(getErrorMessage(err))
  }
}

function onPageChange(newPage: number) {
  page.value = newPage
  load()
}

onMounted(load)
</script>

<template>
  <AppCard>
    <LoadingState v-if="loading" />
    <EmptyState v-else-if="!appointments.length" title="Sem agendamentos" description="Ainda não tem marcações." />

    <div v-else class="space-y-4">
      <div
        v-for="(item, index) in appointments"
        :key="item.id"
        class="overflow-hidden rounded-xl border border-zinc-200"
      >
        <div class="grid sm:grid-cols-3">
          <img
            :src="serviceImage(item.service?.name, index)"
            :alt="item.service?.name"
            class="h-32 w-full object-cover sm:h-full"
          />
          <div class="p-4 sm:col-span-2">
            <div class="flex flex-wrap items-center gap-2">
              <h3 class="font-semibold text-zinc-900">{{ item.service?.name ?? 'Serviço' }}</h3>
              <AppBadge :tone="statusTone(item.status)">{{ item.status }}</AppBadge>
            </div>
            <p class="mt-2 text-sm text-zinc-600">Profissional: {{ item.professional?.name ?? '—' }}</p>
            <p class="text-sm text-zinc-500">{{ formatDate(item.start_time) }}</p>
            <p v-if="item.notes" class="mt-2 text-sm text-zinc-500">{{ item.notes }}</p>
            <AppButton
              v-if="item.status !== 'canceled' && item.status !== 'completed'"
              class="mt-4"
              variant="danger"
              @click="cancel(item)"
            >
              Cancelar
            </AppButton>
          </div>
        </div>
      </div>

      <AppPagination :current-page="page" :last-page="lastPage" :total="total" @change="onPageChange" />
    </div>
  </AppCard>
</template>

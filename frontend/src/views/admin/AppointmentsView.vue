<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppCard from '@/components/ui/AppCard.vue'
import StatusBadge from '@/components/ui/StatusBadge.vue'
import AppPagination from '@/components/ui/AppPagination.vue'
import AppSelect from '@/components/ui/AppSelect.vue'
import AppTextarea from '@/components/ui/AppTextarea.vue'
import AppModal from '@/components/ui/AppModal.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingState from '@/components/ui/LoadingState.vue'
import { getErrorMessage } from '@/lib/api'
import {
  cancelAppointment,
  createAppointment,
  fetchAppointments,
  fetchClients,
  fetchProfessionals,
  fetchServices,
  updateAppointment,
} from '@/services/salonApi'
import type { Appointment, Client, Professional, Service } from '@/types/api'
import { useUiStore } from '@/stores/ui'

const ui = useUiStore()
const appointments = ref<Appointment[]>([])
const clients = ref<Client[]>([])
const services = ref<Service[]>([])
const professionals = ref<Professional[]>([])
const loading = ref(true)
const saving = ref(false)
const page = ref(1)
const lastPage = ref(1)
const total = ref(0)
const modalOpen = ref(false)
const editing = ref<Appointment | null>(null)

const form = reactive({
  client_id: '',
  service_id: '',
  professional_id: '',
  start_time: '',
  notes: '',
  status: 'pending',
})


function formatDate(value: string) {
  return new Intl.DateTimeFormat('pt-PT', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value))
}

function resetForm() {
  form.client_id = ''
  form.service_id = ''
  form.professional_id = ''
  form.start_time = ''
  form.notes = ''
  form.status = 'pending'
  editing.value = null
}

function toLocalInput(iso: string) {
  const d = new Date(iso)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function openCreate() {
  resetForm()
  modalOpen.value = true
}

function openEdit(appointment: Appointment) {
  editing.value = appointment
  form.client_id = String(appointment.client_id)
  form.service_id = String(appointment.service_id)
  form.professional_id = String(appointment.professional_id)
  form.start_time = toLocalInput(appointment.start_time)
  form.notes = appointment.notes ?? ''
  form.status = appointment.status
  modalOpen.value = true
}

async function loadLookups() {
  try {
    const [clientList, serviceList, proList] = await Promise.all([
      fetchClients({ per_page: 100 }),
      fetchServices({ per_page: 100 }),
      fetchProfessionals({ is_active: true, per_page: 100 }),
    ])
    clients.value = clientList.data ?? []
    services.value = serviceList.data ?? []
    professionals.value = proList.data ?? []
  } catch (err) {
    ui.error(getErrorMessage(err))
  }
}

async function load() {
  loading.value = true
  try {
    const response = await fetchAppointments({ page: page.value, per_page: 10 })
    appointments.value = response.data ?? []
    lastPage.value = response.meta?.last_page ?? 1
    total.value = response.meta?.total ?? appointments.value.length
  } catch (err) {
    ui.error(getErrorMessage(err))
  } finally {
    loading.value = false
  }
}

async function submit() {
  saving.value = true
  try {
    const payload = {
      client_id: Number(form.client_id),
      service_id: Number(form.service_id),
      professional_id: Number(form.professional_id),
      start_time: new Date(form.start_time).toISOString(),
      notes: form.notes || undefined,
      status: form.status,
    }
    if (editing.value) {
      await updateAppointment(editing.value.id, payload)
      ui.success('Agendamento atualizado.')
    } else {
      await createAppointment(payload)
      ui.success('Agendamento criado.')
    }
    modalOpen.value = false
    resetForm()
    await load()
  } catch (err) {
    ui.error(getErrorMessage(err))
  } finally {
    saving.value = false
  }
}

async function confirm(item: Appointment) {
  try {
    await updateAppointment(item.id, { status: 'confirmed' })
    ui.success('Agendamento confirmado.')
    await load()
  } catch (err) {
    ui.error(getErrorMessage(err))
  }
}

async function cancel(item: Appointment) {
  const ok = await ui.confirm({
    title: 'Cancelar agendamento',
    message: `Cancelar o agendamento de ${item.client?.name ?? 'cliente'}?`,
    confirmLabel: 'Cancelar agendamento',
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

onMounted(async () => {
  await loadLookups()
  await load()
})
</script>

<template>
  <div class="w-full space-y-6">
    <div class="flex justify-end">
      <AppButton @click="openCreate">Novo agendamento</AppButton>
    </div>

    <AppCard>
      <LoadingState v-if="loading" />
      <EmptyState v-else-if="!appointments.length" title="Nenhum agendamento" />
      <div v-else class="space-y-4">
        <div
          v-for="item in appointments"
          :key="item.id"
          class="flex flex-col gap-3 rounded-lg border border-zinc-200 p-4 lg:flex-row lg:items-center lg:justify-between"
        >
          <div>
            <div class="flex flex-wrap items-center gap-2">
              <p class="font-semibold text-zinc-900">{{ item.client?.name ?? `Cliente #${item.client_id}` }}</p>
              <StatusBadge :status="item.status" />
            </div>
            <p class="mt-1 text-sm text-zinc-600">
              {{ item.service?.name }} · {{ item.professional?.name }}
            </p>
            <p class="text-sm text-zinc-500">{{ formatDate(item.start_time) }}</p>
          </div>
          <div class="flex flex-wrap gap-2">
            <AppButton variant="secondary" size="sm" @click="openEdit(item)">Editar</AppButton>
            <AppButton v-if="item.status === 'pending'" size="sm" @click="confirm(item)">
              Confirmar
            </AppButton>
            <AppButton v-if="item.status !== 'canceled'" variant="danger" size="sm" @click="cancel(item)">
              Cancelar
            </AppButton>
          </div>
        </div>
        <AppPagination :current-page="page" :last-page="lastPage" :total="total" @change="onPageChange" />
      </div>
    </AppCard>

    <AppModal
      :open="modalOpen"
      :title="editing ? 'Editar agendamento' : 'Novo agendamento'"
      @close="modalOpen = false"
    >
      <form class="grid gap-4" @submit.prevent="submit">
        <AppSelect
          v-model="form.client_id"
          label="Cliente"
          required
          :options="clients.map((c) => ({ value: c.id, label: c.name }))"
        />
        <AppSelect
          v-model="form.service_id"
          label="Serviço"
          required
          :options="services.map((s) => ({ value: s.id, label: s.name }))"
        />
        <AppSelect
          v-model="form.professional_id"
          label="Profissional"
          required
          :options="professionals.map((p) => ({ value: p.id, label: p.name }))"
        />
        <label class="block space-y-1">
          <span class="text-sm font-medium text-zinc-700">Data e hora</span>
          <input
            v-model="form.start_time"
            type="datetime-local"
            required
            class="w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm"
          />
        </label>
        <AppSelect
          v-if="editing"
          v-model="form.status"
          label="Estado"
          :options="[
            { value: 'pending', label: 'Pendente' },
            { value: 'confirmed', label: 'Confirmado' },
            { value: 'completed', label: 'Concluído' },
            { value: 'canceled', label: 'Cancelado' },
          ]"
        />
        <AppTextarea v-model="form.notes" label="Notas" :rows="2" />
        <div class="flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
          <AppButton variant="secondary" type="button" @click="modalOpen = false">Fechar</AppButton>
          <AppButton type="submit" :loading="saving">Guardar</AppButton>
        </div>
      </form>
    </AppModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppCard from '@/components/ui/AppCard.vue'
import AppInput from '@/components/ui/AppInput.vue'
import AppModal from '@/components/ui/AppModal.vue'
import AppPagination from '@/components/ui/AppPagination.vue'
import AppSelect from '@/components/ui/AppSelect.vue'
import AppTable from '@/components/ui/AppTable.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingState from '@/components/ui/LoadingState.vue'
import { getErrorMessage, getValidationErrors } from '@/lib/api'
import {
  createProfessionalSchedule,
  deleteProfessionalSchedule,
  fetchProfessionalSchedules,
  fetchProfessionals,
  updateProfessionalSchedule,
} from '@/services/salonApi'
import type { Professional, ProfessionalSchedule } from '@/types/api'
import { useUiStore } from '@/stores/ui'

const ui = useUiStore()

const DAY_OPTIONS = [
  { value: '0', label: 'Domingo' },
  { value: '1', label: 'Segunda-feira' },
  { value: '2', label: 'Terça-feira' },
  { value: '3', label: 'Quarta-feira' },
  { value: '4', label: 'Quinta-feira' },
  { value: '5', label: 'Sexta-feira' },
  { value: '6', label: 'Sábado' },
] as const

const schedules = ref<ProfessionalSchedule[]>([])
const professionals = ref<Professional[]>([])
const loading = ref(true)
const saving = ref(false)
const page = ref(1)
const lastPage = ref(1)
const total = ref(0)
const modalOpen = ref(false)
const editing = ref<ProfessionalSchedule | null>(null)

const filters = reactive({
  professional_id: '',
  day_of_week: '',
})

const form = reactive({
  professional_id: '',
  day_of_week: '',
  start_time: '09:00',
  end_time: '18:00',
})

const fieldErrors = reactive<Record<string, string>>({
  professional_id: '',
  day_of_week: '',
  start_time: '',
  end_time: '',
})

const hasFilters = computed(() => Boolean(filters.professional_id || filters.day_of_week))

const professionalOptions = computed(() =>
  professionals.value.map((p) => ({ value: String(p.id), label: p.name })),
)

const filterProfessionalOptions = computed(() => [
  { value: '', label: 'Todos os profissionais' },
  ...professionalOptions.value,
])

const filterDayOptions = computed(() => [
  { value: '', label: 'Todos os dias' },
  ...DAY_OPTIONS.map((d) => ({ value: d.value, label: d.label })),
])

const formDayOptions = computed(() => DAY_OPTIONS.map((d) => ({ value: d.value, label: d.label })))

function clearFieldErrors() {
  fieldErrors.professional_id = ''
  fieldErrors.day_of_week = ''
  fieldErrors.start_time = ''
  fieldErrors.end_time = ''
}

function resetForm() {
  form.professional_id = filters.professional_id || ''
  form.day_of_week = filters.day_of_week || '1'
  form.start_time = '09:00'
  form.end_time = '18:00'
  editing.value = null
  clearFieldErrors()
}

function openCreate() {
  resetForm()
  modalOpen.value = true
}

function openEdit(schedule: ProfessionalSchedule) {
  editing.value = schedule
  form.professional_id = String(schedule.professional_id)
  form.day_of_week = String(schedule.day_of_week)
  form.start_time = schedule.start_time
  form.end_time = schedule.end_time
  clearFieldErrors()
  modalOpen.value = true
}

function clearFilters() {
  filters.professional_id = ''
  filters.day_of_week = ''
  page.value = 1
}

function durationLabel(start: string, end: string): string {
  const [sh = 0, sm = 0] = start.split(':').map(Number)
  const [eh = 0, em = 0] = end.split(':').map(Number)
  const mins = eh * 60 + em - (sh * 60 + sm)
  if (mins <= 0) return '—'
  const h = Math.floor(mins / 60)
  const m = mins % 60
  if (h && m) return `${h}h ${m}min`
  if (h) return `${h}h`
  return `${m} min`
}

function validateFormLocally(): boolean {
  clearFieldErrors()
  let valid = true

  if (!form.professional_id) {
    fieldErrors.professional_id = 'Selecione um profissional.'
    valid = false
  }
  if (form.day_of_week === '') {
    fieldErrors.day_of_week = 'Selecione o dia da semana.'
    valid = false
  }
  if (!form.start_time) {
    fieldErrors.start_time = 'Indique a hora de início.'
    valid = false
  }
  if (!form.end_time) {
    fieldErrors.end_time = 'Indique a hora de fim.'
    valid = false
  }
  if (form.start_time && form.end_time && form.end_time <= form.start_time) {
    fieldErrors.end_time = 'A hora de fim deve ser posterior à hora de início.'
    valid = false
  }

  return valid
}

async function loadProfessionals() {
  try {
    const response = await fetchProfessionals({ is_active: true, per_page: 100 })
    professionals.value = response.data ?? []
  } catch (err) {
    ui.error(getErrorMessage(err, 'Não foi possível carregar os profissionais.'))
  }
}

async function load() {
  loading.value = true
  try {
    const response = await fetchProfessionalSchedules({
      page: page.value,
      per_page: 15,
      professional_id: filters.professional_id ? Number(filters.professional_id) : undefined,
      day_of_week: filters.day_of_week !== '' ? Number(filters.day_of_week) : undefined,
    })
    schedules.value = response.data ?? []
    lastPage.value = response.meta?.last_page ?? 1
    total.value = response.meta?.total ?? schedules.value.length
  } catch (err) {
    ui.error(getErrorMessage(err, 'Não foi possível carregar os horários.'))
  } finally {
    loading.value = false
  }
}

async function submit() {
  if (!validateFormLocally()) return

  saving.value = true
  clearFieldErrors()
  try {
    const payload = {
      professional_id: Number(form.professional_id),
      day_of_week: Number(form.day_of_week),
      start_time: form.start_time,
      end_time: form.end_time,
    }

    if (editing.value) {
      await updateProfessionalSchedule(editing.value.id, payload)
      ui.success('Horário atualizado com sucesso.')
    } else {
      await createProfessionalSchedule(payload)
      ui.success('Horário criado com sucesso.')
    }

    modalOpen.value = false
    resetForm()
    await load()
  } catch (err) {
    const errors = getValidationErrors(err)
    fieldErrors.professional_id = errors.professional_id ?? ''
    fieldErrors.day_of_week = errors.day_of_week ?? ''
    fieldErrors.start_time = errors.start_time ?? ''
    fieldErrors.end_time = errors.end_time ?? ''
    ui.error(getErrorMessage(err, 'Não foi possível guardar o horário.'))
  } finally {
    saving.value = false
  }
}

async function remove(schedule: ProfessionalSchedule) {
  const name = schedule.professional?.name ?? 'profissional'
  const ok = await ui.confirm({
    title: 'Eliminar horário',
    message: `Eliminar o horário de ${name}, ${schedule.day_name}, das ${schedule.start_time} às ${schedule.end_time}?`,
    confirmLabel: 'Eliminar',
    variant: 'danger',
  })
  if (!ok) return

  try {
    await deleteProfessionalSchedule(schedule.id)
    ui.success('Horário eliminado.')
    await load()
  } catch (err) {
    ui.error(getErrorMessage(err, 'Não foi possível eliminar o horário.'))
  }
}

function onPageChange(newPage: number) {
  page.value = newPage
  load()
}

watch(
  () => [filters.professional_id, filters.day_of_week] as const,
  () => {
    page.value = 1
    load()
  },
)

onMounted(async () => {
  await Promise.all([loadProfessionals(), load()])
})
</script>

<template>
  <div class="w-full space-y-6">
    <div class="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
      <div>
        <h1 class="text-2xl font-semibold text-zinc-900">Horários dos profissionais</h1>
        <p class="mt-1 max-w-2xl text-sm text-zinc-500">
          Configure os períodos semanais em que cada profissional está disponível para marcações.
        </p>
      </div>
      <AppButton class="shrink-0" @click="openCreate">Novo horário</AppButton>
    </div>

    <div class="flex flex-col gap-3 rounded-xl border border-zinc-200 bg-white p-4 sm:flex-row sm:flex-wrap sm:items-end">
      <AppSelect
        v-model="filters.professional_id"
        label="Profissional"
        :options="filterProfessionalOptions"
        class="min-w-[200px] flex-1"
      />
      <AppSelect
        v-model="filters.day_of_week"
        label="Dia da semana"
        :options="filterDayOptions"
        class="min-w-[180px] flex-1"
      />
      <AppButton
        variant="secondary"
        type="button"
        :disabled="!hasFilters"
        @click="clearFilters"
      >
        Limpar filtros
      </AppButton>
    </div>

    <AppCard>
      <LoadingState v-if="loading" />

      <EmptyState
        v-else-if="!schedules.length && hasFilters"
        title="Nenhum horário corresponde aos filtros selecionados."
        description="Ajuste o profissional ou o dia da semana e tente novamente."
      />

      <EmptyState
        v-else-if="!schedules.length"
        title="Nenhum horário configurado."
        description="Crie o primeiro período de disponibilidade para começar a disponibilizar marcações."
      />

      <div v-else class="space-y-4">
        <div class="hidden md:block">
          <AppTable>
            <thead>
              <tr>
                <th>Profissional</th>
                <th>Dia</th>
                <th>Início</th>
                <th>Fim</th>
                <th>Duração</th>
                <th class="text-right">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="schedule in schedules" :key="schedule.id">
                <td class="font-medium text-zinc-900">
                  {{ schedule.professional?.name ?? `Profissional #${schedule.professional_id}` }}
                </td>
                <td>{{ schedule.day_name }}</td>
                <td>{{ schedule.start_time }}</td>
                <td>{{ schedule.end_time }}</td>
                <td>{{ durationLabel(schedule.start_time, schedule.end_time) }}</td>
                <td>
                  <div class="flex justify-end gap-2">
                    <AppButton variant="secondary" size="sm" @click="openEdit(schedule)">
                      Editar
                    </AppButton>
                    <AppButton variant="danger" size="sm" @click="remove(schedule)">
                      Eliminar
                    </AppButton>
                  </div>
                </td>
              </tr>
            </tbody>
          </AppTable>
        </div>

        <div class="grid gap-3 md:hidden">
          <article
            v-for="schedule in schedules"
            :key="schedule.id"
            class="rounded-xl border border-zinc-200 bg-zinc-50/80 p-4"
          >
            <p class="font-semibold text-zinc-900">
              {{ schedule.professional?.name ?? `Profissional #${schedule.professional_id}` }}
            </p>
            <p class="mt-1 text-sm text-zinc-600">{{ schedule.day_name }}</p>
            <p class="mt-2 text-sm text-zinc-700">
              {{ schedule.start_time }} – {{ schedule.end_time }}
              <span class="text-zinc-400">
                · {{ durationLabel(schedule.start_time, schedule.end_time) }}
              </span>
            </p>
            <div class="mt-4 flex gap-2">
              <AppButton variant="secondary" class="flex-1" @click="openEdit(schedule)">
                Editar
              </AppButton>
              <AppButton variant="danger" class="flex-1" @click="remove(schedule)">
                Eliminar
              </AppButton>
            </div>
          </article>
        </div>

        <AppPagination
          v-if="lastPage > 1"
          :current-page="page"
          :last-page="lastPage"
          :total="total"
          @change="onPageChange"
        />
      </div>
    </AppCard>

    <AppModal
      :open="modalOpen"
      :title="editing ? 'Editar horário' : 'Novo horário'"
      @close="modalOpen = false"
    >
      <form class="grid gap-4 sm:grid-cols-2" @submit.prevent="submit">
        <div class="sm:col-span-2">
          <AppSelect
            v-model="form.professional_id"
            label="Profissional"
            :options="professionalOptions"
            required
          />
          <p v-if="fieldErrors.professional_id" class="mt-1 text-sm text-red-600">
            {{ fieldErrors.professional_id }}
          </p>
        </div>

        <div>
          <AppSelect
            v-model="form.day_of_week"
            label="Dia da semana"
            :options="formDayOptions"
            required
          />
          <p v-if="fieldErrors.day_of_week" class="mt-1 text-sm text-red-600">
            {{ fieldErrors.day_of_week }}
          </p>
        </div>

        <div class="hidden sm:block" />

        <div>
          <AppInput
            v-model="form.start_time"
            label="Hora de início"
            type="time"
            required
          />
          <p v-if="fieldErrors.start_time" class="mt-1 text-sm text-red-600">
            {{ fieldErrors.start_time }}
          </p>
        </div>

        <div>
          <AppInput
            v-model="form.end_time"
            label="Hora de fim"
            type="time"
            required
          />
          <p v-if="fieldErrors.end_time" class="mt-1 text-sm text-red-600">
            {{ fieldErrors.end_time }}
          </p>
        </div>

        <div class="flex flex-col-reverse gap-2 sm:col-span-2 sm:flex-row sm:justify-end">
          <AppButton variant="secondary" type="button" :disabled="saving" @click="modalOpen = false">
            Cancelar
          </AppButton>
          <AppButton type="submit" :loading="saving">
            {{ editing ? 'Guardar alterações' : 'Criar horário' }}
          </AppButton>
        </div>
      </form>
    </AppModal>
  </div>
</template>

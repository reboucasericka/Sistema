<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import AppBadge from '@/components/ui/AppBadge.vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppCard from '@/components/ui/AppCard.vue'
import AppImageUpload from '@/components/ui/AppImageUpload.vue'
import AppInput from '@/components/ui/AppInput.vue'
import AppModal from '@/components/ui/AppModal.vue'
import AppPagination from '@/components/ui/AppPagination.vue'
import AppSelect from '@/components/ui/AppSelect.vue'
import AppTable from '@/components/ui/AppTable.vue'
import AppTextarea from '@/components/ui/AppTextarea.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingState from '@/components/ui/LoadingState.vue'
import { getErrorMessage, getValidationErrors } from '@/lib/api'
import { needsMultipart, toFormData } from '@/lib/formData'
import { IMAGES, mediaUrl } from '@/lib/images'
import {
  createProfessional,
  deleteProfessional,
  fetchProfessionals,
  fetchProfessionalServices,
  fetchServices,
  syncProfessionalServices,
  toggleProfessionalStatus,
  updateProfessional,
  type ListParams,
} from '@/services/salonApi'
import type { Professional, ProfessionalPayload, Service } from '@/types/api'
import { useUiStore } from '@/stores/ui'

const ui = useUiStore()

const professionals = ref<Professional[]>([])
const loading = ref(true)
const saving = ref(false)
const deletingId = ref<number | null>(null)
const page = ref(1)
const lastPage = ref(1)
const total = ref(0)
const search = ref('')
const statusFilter = ref('')
const modalOpen = ref(false)
const editing = ref<Professional | null>(null)

const servicesModalOpen = ref(false)
const servicesTarget = ref<Professional | null>(null)
const allServices = ref<Service[]>([])
const selectedServiceIds = ref<number[]>([])
const servicesLoading = ref(false)
const servicesSaving = ref(false)
const servicesSearch = ref('')
const servicesError = ref('')
const servicesSelectionFilter = ref<'all' | 'selected' | 'unselected'>('all')
const collapsedCategories = ref<Record<string, boolean>>({})

const UNCATEGORIZED = 'Sem categoria'

const hasFilters = computed(() => Boolean(search.value.trim() || statusFilter.value))

const selectedServicesCount = computed(() => selectedServiceIds.value.length)
const totalServicesCount = computed(() => allServices.value.length)

const filteredServices = computed(() => {
  const term = servicesSearch.value.trim().toLowerCase()
  const selected = new Set(selectedServiceIds.value)

  return allServices.value
    .filter((service) => {
      if (servicesSelectionFilter.value === 'selected' && !selected.has(service.id)) return false
      if (servicesSelectionFilter.value === 'unselected' && selected.has(service.id)) return false

      if (!term) return true
      const category = (
        service.service_category?.name?.trim() ||
        service.category?.trim() ||
        UNCATEGORIZED
      ).toLowerCase()
      const haystack = `${service.name} ${category}`.toLowerCase()
      return haystack.includes(term)
    })
    .slice()
    .sort((a, b) => a.name.localeCompare(b.name, 'pt'))
})

const groupedServices = computed(() => {
  const groups = new Map<string, Service[]>()

  for (const service of filteredServices.value) {
    const key =
      service.service_category?.name?.trim() || service.category?.trim() || UNCATEGORIZED
    const list = groups.get(key) ?? []
    list.push(service)
    groups.set(key, list)
  }

  const named = [...groups.entries()]
    .filter(([name]) => name !== UNCATEGORIZED)
    .sort(([a], [b]) => a.localeCompare(b, 'pt'))

  const uncategorized = groups.get(UNCATEGORIZED)
  if (uncategorized?.length) {
    named.push([UNCATEGORIZED, uncategorized])
  }

  return named.map(([name, services]) => ({
    name,
    services,
    count: services.length,
  }))
})

const visibleServiceIds = computed(() => filteredServices.value.map((service) => service.id))

const allVisibleSelected = computed(() => {
  const ids = visibleServiceIds.value
  if (!ids.length) return false
  const selected = new Set(selectedServiceIds.value)
  return ids.every((id) => selected.has(id))
})

const noneVisibleSelected = computed(() => {
  const ids = visibleServiceIds.value
  if (!ids.length) return true
  const selected = new Set(selectedServiceIds.value)
  return ids.every((id) => !selected.has(id))
})

const form = reactive({
  name: '',
  specialty: '',
  biography: '',
  years_experience: '',
  commission_percentage: '0',
  email: '',
  phone: '',
  instagram: '',
  facebook: '',
  is_active: true,
})

const photoFile = ref<File | null>(null)
const removePhoto = ref(false)
const currentPhotoUrl = ref<string | null>(null)

const fieldErrors = reactive<Record<string, string>>({})

const statusOptions = [
  { value: '1', label: 'Ativos' },
  { value: '0', label: 'Inativos' },
]

function professionalImage(pro: Professional) {
  return mediaUrl(pro.image_url || pro.photo) || IMAGES.avatar
}

function clearFieldErrors() {
  Object.keys(fieldErrors).forEach((key) => {
    fieldErrors[key] = ''
  })
}

function resetForm() {
  form.name = ''
  form.specialty = ''
  form.biography = ''
  form.years_experience = ''
  form.commission_percentage = '0'
  form.email = ''
  form.phone = ''
  form.instagram = ''
  form.facebook = ''
  form.is_active = true
  photoFile.value = null
  removePhoto.value = false
  currentPhotoUrl.value = null
  editing.value = null
  clearFieldErrors()
}

function openCreate() {
  resetForm()
  modalOpen.value = true
}

function openEdit(pro: Professional) {
  editing.value = pro
  form.name = pro.name
  form.specialty = pro.specialty
  form.biography = pro.biography ?? ''
  form.years_experience = pro.years_experience != null ? String(pro.years_experience) : ''
  form.commission_percentage =
    pro.commission_percentage != null ? String(pro.commission_percentage) : '0'
  form.email = pro.email ?? ''
  form.phone = pro.phone ?? ''
  form.instagram = pro.instagram ?? ''
  form.facebook = pro.facebook ?? ''
  form.is_active = pro.is_active ?? true
  photoFile.value = null
  removePhoto.value = false
  currentPhotoUrl.value = mediaUrl(pro.image_url || pro.photo)
  clearFieldErrors()
  modalOpen.value = true
}

function closeModal() {
  if (saving.value) return
  modalOpen.value = false
}

function toggleServiceSelection(serviceId: number) {
  if (selectedServiceIds.value.includes(serviceId)) {
    selectedServiceIds.value = selectedServiceIds.value.filter((id) => id !== serviceId)
  } else {
    selectedServiceIds.value = [...selectedServiceIds.value, serviceId]
  }
}

function isServiceSelected(serviceId: number) {
  return selectedServiceIds.value.includes(serviceId)
}

function setSelectionFilter(next: 'selected' | 'unselected') {
  servicesSelectionFilter.value = servicesSelectionFilter.value === next ? 'all' : next
}

function isCategoryCollapsed(category: string) {
  return Boolean(collapsedCategories.value[category])
}

function toggleCategory(category: string) {
  collapsedCategories.value = {
    ...collapsedCategories.value,
    [category]: !collapsedCategories.value[category],
  }
}

function selectAllVisible() {
  const selected = new Set(selectedServiceIds.value)
  for (const id of visibleServiceIds.value) selected.add(id)
  selectedServiceIds.value = [...selected]
}

function clearVisibleSelection() {
  const visible = new Set(visibleServiceIds.value)
  selectedServiceIds.value = selectedServiceIds.value.filter((id) => !visible.has(id))
}

function onSelectAllVisibleChange(event: Event) {
  const checked = (event.target as HTMLInputElement).checked
  if (checked) selectAllVisible()
  else clearVisibleSelection()
}

function onClearVisibleChange(event: Event) {
  const input = event.target as HTMLInputElement
  clearVisibleSelection()
  input.checked = false
}

async function openServicesModal(pro: Professional) {
  servicesTarget.value = pro
  servicesModalOpen.value = true
  servicesSearch.value = ''
  servicesError.value = ''
  servicesSelectionFilter.value = 'all'
  collapsedCategories.value = {}
  servicesLoading.value = true
  selectedServiceIds.value = []

  try {
    const [catalog, assigned] = await Promise.all([
      fetchServices({ per_page: 500, is_active: true }),
      fetchProfessionalServices(pro.id),
    ])
    allServices.value = (catalog.data ?? []).filter((service) => service.is_active)
    selectedServiceIds.value = assigned.map((service) => service.id)
  } catch (err) {
    servicesError.value = getErrorMessage(err, 'Não foi possível carregar os serviços.')
    ui.error(servicesError.value)
  } finally {
    servicesLoading.value = false
  }
}

function closeServicesModal() {
  if (servicesSaving.value) return
  servicesModalOpen.value = false
  servicesTarget.value = null
  servicesSearch.value = ''
  servicesError.value = ''
  servicesSelectionFilter.value = 'all'
  collapsedCategories.value = {}
}

async function saveServices() {
  if (!servicesTarget.value || servicesSaving.value) return

  servicesSaving.value = true
  servicesError.value = ''
  try {
    const result = await syncProfessionalServices(servicesTarget.value.id, selectedServiceIds.value)
    ui.success('Serviços atualizados.')
    servicesModalOpen.value = false
    servicesTarget.value = null
    await load()
    // Keep count in sync even if list cache is stale.
    void result
  } catch (err) {
    if (import.meta.env.DEV) {
      console.error('Falha ao sincronizar serviços', err)
    }
    servicesError.value = getErrorMessage(err, 'Não foi possível guardar os serviços.')
    ui.error(servicesError.value)
  } finally {
    servicesSaving.value = false
  }
}

function buildListParams(): ListParams {
  const params: ListParams = {
    page: page.value,
    per_page: 12,
  }

  const term = search.value.trim()
  if (term) params.search = term

  if (statusFilter.value === '1') params.is_active = true
  if (statusFilter.value === '0') params.is_active = false

  return params
}

function buildPayload(): ProfessionalPayload {
  return {
    name: form.name.trim(),
    specialty: form.specialty.trim(),
    biography: form.biography.trim() || null,
    years_experience: form.years_experience === '' ? null : Number(form.years_experience),
    commission_percentage:
      form.commission_percentage === '' ? 0 : Number(form.commission_percentage),
    email: form.email.trim() || null,
    phone: form.phone.trim() || null,
    instagram: form.instagram.trim() || null,
    facebook: form.facebook.trim() || null,
    is_active: Boolean(form.is_active),
  }
}

async function load() {
  loading.value = true
  try {
    const response = await fetchProfessionals(buildListParams())
    professionals.value = response.data ?? []
    lastPage.value = response.meta?.last_page ?? 1
    total.value = response.meta?.total ?? professionals.value.length
  } catch (err) {
    ui.error(getErrorMessage(err, 'Não foi possível carregar os profissionais.'))
  } finally {
    loading.value = false
  }
}

async function submit() {
  if (saving.value) return

  clearFieldErrors()
  if (!form.name.trim() || !form.specialty.trim()) {
    if (!form.name.trim()) fieldErrors.name = 'O nome é obrigatório.'
    if (!form.specialty.trim()) fieldErrors.specialty = 'A especialidade é obrigatória.'
    return
  }

  saving.value = true
  try {
    const payload = buildPayload()
    const wasCreating = !editing.value
    const useMultipart = needsMultipart(
      { photo: photoFile.value },
      { remove_photo: removePhoto.value },
    )

    if (editing.value) {
      if (useMultipart) {
        await updateProfessional(
          editing.value.id,
          toFormData(
            { ...payload, remove_photo: removePhoto.value },
            { method: 'PUT', files: { photo: photoFile.value } },
          ),
        )
      } else {
        await updateProfessional(editing.value.id, payload)
      }
      ui.success('Profissional atualizado.')
    } else if (useMultipart) {
      await createProfessional(
        toFormData({ ...payload }, { files: { photo: photoFile.value } }),
      )
      ui.success('Profissional criado. Já pode aparecer no site público se estiver ativo.')
    } else {
      await createProfessional(payload)
      ui.success('Profissional criado. Já pode aparecer no site público se estiver ativo.')
    }

    modalOpen.value = false
    resetForm()

    if (wasCreating) {
      page.value = 1
      search.value = ''
      if (statusFilter.value === '0' && payload.is_active) statusFilter.value = ''
      if (statusFilter.value === '1' && !payload.is_active) statusFilter.value = ''
    }

    await load()
  } catch (err) {
    if (import.meta.env.DEV) {
      console.error('Falha ao guardar profissional', err)
    }
    const errors = getValidationErrors(err)
    Object.assign(fieldErrors, errors)
    ui.error(getErrorMessage(err, 'Não foi possível guardar o profissional.'))
  } finally {
    saving.value = false
  }
}

async function toggleStatus(pro: Professional) {
  const next = !(pro.is_active ?? true)
  if (!next) {
    const ok = await ui.confirm({
      title: 'Desativar profissional',
      message:
        'Ao desativar este profissional, ele deixará de aparecer no site público e não poderá receber novos agendamentos. Os dados históricos serão mantidos.',
      confirmLabel: 'Desativar',
      variant: 'danger',
    })
    if (!ok) return
  }

  try {
    await toggleProfessionalStatus(pro.id, next)
    ui.success(next ? 'Profissional ativado.' : 'Profissional desativado.')
    await load()
  } catch (err) {
    ui.error(getErrorMessage(err))
  }
}

async function remove(pro: Professional) {
  if (deletingId.value != null) return

  const ok = await ui.confirm({
    title: 'Eliminar profissional',
    message: `Eliminar ${pro.name}? Se houver agendamentos associados, a operação será recusada.`,
    confirmLabel: 'Eliminar',
    variant: 'danger',
  })
  if (!ok) return

  deletingId.value = pro.id
  try {
    await deleteProfessional(pro.id)
    ui.success('Profissional eliminado.')
    if (professionals.value.length <= 1 && page.value > 1) {
      page.value -= 1
    }
    await load()
  } catch (err) {
    ui.error(getErrorMessage(err, 'Não foi possível eliminar o profissional.'))
  } finally {
    deletingId.value = null
  }
}

function onPageChange(newPage: number) {
  page.value = newPage
  load()
}

let searchTimer: ReturnType<typeof setTimeout>
function onSearch() {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    page.value = 1
    load()
  }, 300)
}

watch(statusFilter, () => {
  page.value = 1
  load()
})

onMounted(load)
</script>

<template>
  <div class="w-full space-y-6">
    <div class="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
      <div>
        <h1 class="text-2xl font-semibold text-zinc-900">Profissionais</h1>
        <p class="mt-1 max-w-2xl text-sm text-zinc-500">
          Gira os profissionais do salão e controle quem aparece no site público e nas marcações.
        </p>
      </div>
      <AppButton class="shrink-0" @click="openCreate">Novo profissional</AppButton>
    </div>

    <div class="flex flex-col gap-3 rounded-xl border border-zinc-200 bg-white p-4 sm:flex-row sm:flex-wrap sm:items-end">
      <AppInput
        v-model="search"
        label="Pesquisar"
        placeholder="Nome, especialidade ou email"
        class="min-w-[220px] flex-1"
        @update:model-value="onSearch"
      />
      <AppSelect
        v-model="statusFilter"
        label="Estado"
        empty-label="Todos os estados"
        :options="statusOptions"
        class="min-w-[180px]"
      />
    </div>

    <AppCard>
      <LoadingState v-if="loading" />

      <EmptyState
        v-else-if="!professionals.length && hasFilters"
        title="Nenhum profissional corresponde aos filtros."
        description="Ajuste a pesquisa ou o estado e tente novamente."
      />

      <EmptyState
        v-else-if="!professionals.length"
        title="Nenhum profissional cadastrado."
        description="Crie o primeiro profissional para o apresentar no site e no agendamento."
      />

      <div v-else class="space-y-4">
        <div class="hidden md:block">
          <AppTable>
            <thead>
              <tr>
                <th>Profissional</th>
                <th>Especialidade</th>
                <th>Contacto</th>
                <th>Experiência</th>
                <th>Serviços</th>
                <th>Estado</th>
                <th>Site público</th>
                <th class="text-right">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="pro in professionals" :key="pro.id">
                <td>
                  <div class="flex items-center gap-3">
                    <img
                      :src="professionalImage(pro)"
                      :alt="pro.name"
                      class="h-10 w-10 rounded-full object-cover ring-1 ring-zinc-200"
                    />
                    <span class="font-medium text-zinc-900">{{ pro.name }}</span>
                  </div>
                </td>
                <td>{{ pro.specialty }}</td>
                <td>
                  <p>{{ pro.email ?? '—' }}</p>
                  <p class="text-xs text-zinc-500">{{ pro.phone ?? '—' }}</p>
                </td>
                <td>
                  {{ pro.years_experience != null ? `${pro.years_experience} anos` : '—' }}
                </td>
                <td>
                  <span class="text-sm text-zinc-700">{{ pro.services_count ?? 0 }}</span>
                </td>
                <td>
                  <AppBadge :tone="pro.is_active ? 'success' : 'default'">
                    {{ pro.is_active ? 'Ativo' : 'Inativo' }}
                  </AppBadge>
                </td>
                <td>{{ pro.is_active ? 'Visível' : 'Oculto' }}</td>
                <td>
                  <div class="flex flex-wrap justify-end gap-2">
                    <AppButton variant="secondary" size="sm" @click="openEdit(pro)">Editar</AppButton>
                    <AppButton variant="secondary" size="sm" @click="openServicesModal(pro)">
                      Gerir serviços
                    </AppButton>
                    <AppButton variant="secondary" size="sm" @click="toggleStatus(pro)">
                      {{ pro.is_active ? 'Desativar' : 'Ativar' }}
                    </AppButton>
                    <AppButton
                      variant="danger"
                      size="sm"
                      :loading="deletingId === pro.id"
                      :disabled="deletingId != null"
                      @click="remove(pro)"
                    >
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
            v-for="pro in professionals"
            :key="pro.id"
            class="rounded-xl border border-zinc-200 bg-zinc-50/80 p-4"
          >
            <div class="flex items-start gap-3">
              <img
                :src="professionalImage(pro)"
                :alt="pro.name"
                class="h-14 w-14 rounded-full object-cover ring-1 ring-zinc-200"
              />
              <div class="min-w-0 flex-1">
                <p class="font-semibold text-zinc-900">{{ pro.name }}</p>
                <p class="text-sm text-zinc-600">{{ pro.specialty }}</p>
                <p class="mt-1 text-xs text-zinc-500">Serviços: {{ pro.services_count ?? 0 }}</p>
                <AppBadge class="mt-2" :tone="pro.is_active ? 'success' : 'default'">
                  {{ pro.is_active ? 'Ativo' : 'Inativo' }}
                </AppBadge>
              </div>
            </div>
            <div class="mt-4 grid grid-cols-2 gap-2">
              <AppButton variant="secondary" @click="openEdit(pro)">Editar</AppButton>
              <AppButton variant="secondary" @click="openServicesModal(pro)">Serviços</AppButton>
              <AppButton variant="secondary" @click="toggleStatus(pro)">
                {{ pro.is_active ? 'Desativar' : 'Ativar' }}
              </AppButton>
              <AppButton
                variant="danger"
                :loading="deletingId === pro.id"
                :disabled="deletingId != null"
                @click="remove(pro)"
              >
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
      size="lg"
      :title="editing ? 'Editar profissional' : 'Novo profissional'"
      @close="closeModal"
    >
      <form id="professional-form" class="space-y-6" @submit.prevent="submit">
        <section class="space-y-3">
          <h4 class="text-sm font-semibold text-zinc-900">Dados profissionais</h4>
          <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
            <div>
              <AppInput v-model="form.name" label="Nome" required />
              <p v-if="fieldErrors.name" class="mt-1 text-sm text-red-600">{{ fieldErrors.name }}</p>
            </div>
            <div>
              <AppInput v-model="form.specialty" label="Especialidade" required />
              <p v-if="fieldErrors.specialty" class="mt-1 text-sm text-red-600">
                {{ fieldErrors.specialty }}
              </p>
            </div>
            <div>
              <AppInput
                v-model="form.years_experience"
                label="Anos de experiência"
                type="number"
                min="0"
              />
              <p v-if="fieldErrors.years_experience" class="mt-1 text-sm text-red-600">
                {{ fieldErrors.years_experience }}
              </p>
            </div>
            <div>
              <AppInput
                v-model="form.commission_percentage"
                label="Comissão (%)"
                type="number"
                min="0"
                max="100"
                step="0.01"
              />
              <p v-if="fieldErrors.commission_percentage" class="mt-1 text-sm text-red-600">
                {{ fieldErrors.commission_percentage }}
              </p>
            </div>
          </div>
        </section>

        <section class="space-y-3">
          <h4 class="text-sm font-semibold text-zinc-900">Contacto</h4>
          <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
            <div>
              <AppInput v-model="form.email" label="Email" type="email" />
              <p v-if="fieldErrors.email" class="mt-1 text-sm text-red-600">{{ fieldErrors.email }}</p>
            </div>
            <div>
              <AppInput v-model="form.phone" label="Telefone" />
              <p v-if="fieldErrors.phone" class="mt-1 text-sm text-red-600">{{ fieldErrors.phone }}</p>
            </div>
            <div>
              <AppInput v-model="form.instagram" label="Instagram" placeholder="@utilizador ou URL" />
              <p v-if="fieldErrors.instagram" class="mt-1 text-sm text-red-600">
                {{ fieldErrors.instagram }}
              </p>
            </div>
            <div>
              <AppInput v-model="form.facebook" label="Facebook" placeholder="URL ou utilizador" />
              <p v-if="fieldErrors.facebook" class="mt-1 text-sm text-red-600">
                {{ fieldErrors.facebook }}
              </p>
            </div>
          </div>
        </section>

        <section class="space-y-3">
          <h4 class="text-sm font-semibold text-zinc-900">Apresentação pública</h4>
          <div>
            <AppTextarea v-model="form.biography" label="Biografia" :rows="4" />
            <p v-if="fieldErrors.biography" class="mt-1 text-sm text-red-600">
              {{ fieldErrors.biography }}
            </p>
          </div>
          <div>
            <AppImageUpload
              v-model="photoFile"
              v-model:remove="removePhoto"
              label="Fotografia"
              :current-url="currentPhotoUrl"
              :placeholder-src="IMAGES.avatar"
              :error="fieldErrors.photo"
            />
          </div>
          <label
            class="flex items-start gap-3 rounded-xl border border-zinc-200 bg-zinc-50 px-4 py-3"
          >
            <input
              v-model="form.is_active"
              type="checkbox"
              class="mt-1 rounded border-zinc-300"
            />
            <span>
              <span class="block text-sm font-medium text-zinc-800">
                Ativo no sistema e visível no site público
              </span>
              <span class="mt-0.5 block text-xs text-zinc-500">
                Inativos ficam guardados, mas não aparecem no site nem em novos agendamentos.
              </span>
            </span>
          </label>
        </section>
      </form>

      <template #footer>
        <AppButton variant="secondary" type="button" :disabled="saving" @click="closeModal">
          Cancelar
        </AppButton>
        <AppButton type="submit" form="professional-form" :loading="saving" class="min-w-[10rem]">
          {{
            saving
              ? editing
                ? 'A guardar...'
                : 'A criar...'
              : editing
                ? 'Guardar alterações'
                : 'Criar profissional'
          }}
        </AppButton>
      </template>
    </AppModal>

    <AppModal
      :open="servicesModalOpen"
      size="lg"
      :title="servicesTarget ? `Serviços de ${servicesTarget.name}` : 'Gerir serviços'"
      @close="closeServicesModal"
    >
      <div class="flex min-h-0 flex-col gap-0">
        <div
          class="sticky top-0 z-10 -mx-5 -mt-4 space-y-3 border-b border-zinc-100 bg-white px-5 pb-3 pt-4 sm:-mx-6 sm:px-6"
        >
          <p class="text-sm text-zinc-600">
            <span class="font-medium text-zinc-900">{{ selectedServicesCount }}</span>
            de
            <span class="font-medium text-zinc-900">{{ totalServicesCount }}</span>
            serviços selecionados
          </p>

          <AppInput
            v-model="servicesSearch"
            label="Pesquisar serviço"
            placeholder="Nome ou categoria"
          />

          <div class="flex flex-col gap-2 sm:flex-row sm:flex-wrap sm:items-center sm:justify-between">
            <div class="flex flex-wrap gap-x-4 gap-y-2">
              <label class="inline-flex items-center gap-2 text-sm text-zinc-700">
                <input
                  type="checkbox"
                  class="rounded border-zinc-300 text-brand-600 focus:ring-brand-500"
                  :checked="servicesSelectionFilter === 'selected'"
                  @change="setSelectionFilter('selected')"
                />
                Apenas selecionados
              </label>
              <label class="inline-flex items-center gap-2 text-sm text-zinc-700">
                <input
                  type="checkbox"
                  class="rounded border-zinc-300 text-brand-600 focus:ring-brand-500"
                  :checked="servicesSelectionFilter === 'unselected'"
                  @change="setSelectionFilter('unselected')"
                />
                Apenas não selecionados
              </label>
            </div>

            <div class="flex flex-wrap gap-x-4 gap-y-2">
              <label class="inline-flex items-center gap-2 text-sm text-zinc-700">
                <input
                  type="checkbox"
                  class="rounded border-zinc-300 text-brand-600 focus:ring-brand-500"
                  :checked="allVisibleSelected"
                  :disabled="!visibleServiceIds.length || servicesLoading"
                  @change="onSelectAllVisibleChange"
                />
                Selecionar todos
              </label>
              <label class="inline-flex items-center gap-2 text-sm text-zinc-700">
                <input
                  type="checkbox"
                  class="rounded border-zinc-300 text-brand-600 focus:ring-brand-500"
                  :checked="false"
                  :disabled="!visibleServiceIds.length || servicesLoading || noneVisibleSelected"
                  @change="onClearVisibleChange"
                />
                Remover todos
              </label>
            </div>
          </div>
        </div>

        <div class="min-h-0 pt-3">
          <LoadingState v-if="servicesLoading" />

          <EmptyState
            v-else-if="!allServices.length"
            title="Nenhum serviço disponível."
            description="Crie serviços ativos no catálogo para os associar a profissionais."
          />

          <EmptyState
            v-else-if="!groupedServices.length"
            title="Nenhum serviço corresponde aos filtros."
            description="Ajuste a pesquisa ou os filtros de seleção."
          />

          <div v-else class="divide-y divide-zinc-100">
            <section v-for="group in groupedServices" :key="group.name" class="py-2">
              <button
                type="button"
                class="flex w-full items-center gap-2 rounded-md px-1 py-2 text-left text-sm font-semibold text-zinc-900 hover:bg-zinc-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500/40"
                :aria-expanded="!isCategoryCollapsed(group.name)"
                @click="toggleCategory(group.name)"
              >
                <span class="w-4 shrink-0 text-zinc-500" aria-hidden="true">
                  {{ isCategoryCollapsed(group.name) ? '▶' : '▼' }}
                </span>
                <span class="min-w-0 flex-1">{{ group.name }}</span>
                <span class="text-xs font-medium text-zinc-500">({{ group.count }})</span>
              </button>

              <ul v-show="!isCategoryCollapsed(group.name)" class="mt-1 space-y-0.5">
                <li v-for="service in group.services" :key="service.id">
                  <label
                    class="flex cursor-pointer items-center gap-3 rounded-md px-2 py-2 hover:bg-zinc-50 focus-within:ring-2 focus-within:ring-brand-500/30"
                  >
                    <input
                      type="checkbox"
                      class="rounded border-zinc-300 text-brand-600 focus:ring-brand-500"
                      :checked="isServiceSelected(service.id)"
                      :aria-label="`Selecionar ${service.name}`"
                      @change="toggleServiceSelection(service.id)"
                    />
                    <span class="min-w-0 flex-1 text-sm text-zinc-900">{{ service.name }}</span>
                    <span class="shrink-0 text-xs tabular-nums text-zinc-500">
                      {{ service.duration_minutes }} min
                    </span>
                  </label>
                </li>
              </ul>
            </section>
          </div>
        </div>

        <p v-if="servicesError" class="mt-3 text-sm text-red-600">{{ servicesError }}</p>
      </div>

      <template #footer>
        <AppButton
          variant="secondary"
          type="button"
          :disabled="servicesSaving"
          @click="closeServicesModal"
        >
          Cancelar
        </AppButton>
        <AppButton
          type="button"
          class="min-w-[10rem]"
          :loading="servicesSaving"
          :disabled="servicesLoading"
          @click="saveServices"
        >
          {{ servicesSaving ? 'A guardar...' : 'Guardar' }}
        </AppButton>
      </template>
    </AppModal>
  </div>
</template>

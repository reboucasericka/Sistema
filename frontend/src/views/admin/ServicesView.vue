<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppCard from '@/components/ui/AppCard.vue'
import AppImageUpload from '@/components/ui/AppImageUpload.vue'
import AppInput from '@/components/ui/AppInput.vue'
import AppModal from '@/components/ui/AppModal.vue'
import AppSelect from '@/components/ui/AppSelect.vue'
import AppTextarea from '@/components/ui/AppTextarea.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingState from '@/components/ui/LoadingState.vue'
import { getErrorMessage } from '@/lib/api'
import { needsMultipart, toFormData } from '@/lib/formData'
import { mediaUrl, serviceImage } from '@/lib/images'
import {
  createService,
  deleteService,
  fetchServiceCategories,
  fetchServices,
  updateService,
} from '@/services/salonApi'
import type { Service, ServiceCategory } from '@/types/api'
import { useUiStore } from '@/stores/ui'

const ui = useUiStore()
const services = ref<Service[]>([])
const categories = ref<ServiceCategory[]>([])
const loading = ref(true)
const saving = ref(false)
const modalOpen = ref(false)
const editing = ref<Service | null>(null)
const categoryFilter = ref('')
const imageFile = ref<File | null>(null)
const removeImage = ref(false)
const currentImageUrl = ref<string | null>(null)

const form = reactive({
  name: '',
  service_category_id: '',
  description: '',
  price: '',
  duration_minutes: '60',
})

function categoryName(service: Service) {
  return service.service_category?.name?.trim() || service.category?.trim() || 'Sem categoria'
}

const categoryOptions = computed(() => [
  { value: '', label: 'Todas as categorias' },
  { value: '__none__', label: 'Sem categoria' },
  ...categories.value.map((c) => ({ value: String(c.id), label: c.name })),
])

const formCategoryOptions = computed(() =>
  categories.value
    .filter((c) => c.is_active || c.id === editing.value?.service_category_id)
    .map((c) => ({ value: String(c.id), label: c.name })),
)

const filteredServices = computed(() => {
  if (!categoryFilter.value) return services.value
  if (categoryFilter.value === '__none__') {
    return services.value.filter((s) => !s.service_category_id && !s.category?.trim())
  }
  const id = Number(categoryFilter.value)
  return services.value.filter((s) => s.service_category_id === id)
})

const groupedServices = computed(() => {
  const map = new Map<string, Service[]>()
  for (const service of filteredServices.value) {
    const key = categoryName(service)
    const list = map.get(key) ?? []
    list.push(service)
    map.set(key, list)
  }
  return [...map.entries()].sort(([a], [b]) => a.localeCompare(b, 'pt'))
})

function resetForm() {
  form.name = ''
  form.service_category_id = ''
  form.description = ''
  form.price = ''
  form.duration_minutes = '60'
  imageFile.value = null
  removeImage.value = false
  currentImageUrl.value = null
  editing.value = null
}

function openCreate() {
  resetForm()
  if (categoryFilter.value && categoryFilter.value !== '__none__') {
    form.service_category_id = categoryFilter.value
  }
  modalOpen.value = true
}

function openEdit(service: Service) {
  editing.value = service
  form.name = service.name
  form.service_category_id = service.service_category_id ? String(service.service_category_id) : ''
  form.description = service.description ?? ''
  form.price = String(service.price)
  form.duration_minutes = String(service.duration_minutes)
  imageFile.value = null
  removeImage.value = false
  currentImageUrl.value = mediaUrl(service.image_url || service.image)
  modalOpen.value = true
}

async function load() {
  loading.value = true
  try {
    const [servicesResponse, categoriesResponse] = await Promise.all([
      fetchServices({ per_page: 500 }),
      fetchServiceCategories({ per_page: 200 }),
    ])
    services.value = servicesResponse.data ?? []
    categories.value = categoriesResponse.data ?? []
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
      name: form.name,
      service_category_id: form.service_category_id ? Number(form.service_category_id) : null,
      description: form.description || null,
      price: form.price,
      duration_minutes: Number(form.duration_minutes),
      is_active: true,
    }
    const useMultipart = needsMultipart(
      { image: imageFile.value },
      { remove_image: removeImage.value },
    )

    if (editing.value) {
      if (useMultipart) {
        await updateService(
          editing.value.id,
          toFormData(
            { ...payload, remove_image: removeImage.value },
            { method: 'PUT', files: { image: imageFile.value } },
          ),
        )
      } else {
        await updateService(editing.value.id, payload)
      }
      ui.success('Serviço atualizado. Alterações visíveis no agendamento online.')
    } else if (useMultipart) {
      await createService(toFormData({ ...payload }, { files: { image: imageFile.value } }))
      ui.success('Serviço criado. Já aparece no agendamento online.')
    } else {
      await createService(payload)
      ui.success('Serviço criado. Já aparece no agendamento online.')
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

async function remove(service: Service) {
  const ok = await ui.confirm({
    title: 'Remover serviço',
    message: `Remover "${service.name}"? Deixará de aparecer no agendamento online.`,
    confirmLabel: 'Remover',
    variant: 'danger',
  })
  if (!ok) return
  try {
    await deleteService(service.id)
    ui.success('Serviço removido.')
    await load()
  } catch (err) {
    ui.error(getErrorMessage(err))
  }
}

function formatPrice(value: string) {
  return new Intl.NumberFormat('pt-PT', { style: 'currency', currency: 'EUR' }).format(Number(value))
}

onMounted(load)
</script>

<template>
  <div class="w-full space-y-6">
    <div class="rounded-xl border border-brand-200 bg-brand-50 px-4 py-3 text-sm text-brand-900">
      Os serviços cadastrados aqui aparecem automaticamente no
      <strong>Agendamento Online</strong>. Ao alterar preço, descrição ou categoria, a página pública
      é atualizada na próxima visita.
    </div>

    <div class="flex flex-wrap items-end justify-between gap-4">
      <AppSelect
        v-model="categoryFilter"
        label="Filtrar por categoria"
        :options="categoryOptions"
        class="min-w-[220px]"
      />
      <AppButton @click="openCreate">Novo serviço</AppButton>
    </div>

    <LoadingState v-if="loading" />
    <EmptyState v-else-if="!services.length" title="Nenhum serviço cadastrado" />

    <div v-else class="space-y-8">
      <section v-for="[category, items] in groupedServices" :key="category">
        <h2 class="mb-3 text-lg font-semibold text-zinc-900">{{ category }}</h2>
        <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          <AppCard v-for="(service, index) in items" :key="service.id">
            <img
              :src="serviceImage(service.image_url || service.image, index)"
              :alt="service.name"
              class="mb-4 h-36 w-full rounded-lg object-cover"
            />
            <div class="flex items-start justify-between gap-2">
              <div>
                <h3 class="font-semibold text-zinc-900">{{ service.name }}</h3>
                <p class="mt-1 text-xs font-medium text-brand-700">{{ categoryName(service) }}</p>
                <p class="mt-1 line-clamp-3 text-sm text-zinc-500">
                  {{ service.description || 'Sem descrição' }}
                </p>
              </div>
              <span class="shrink-0 rounded-full bg-brand-100 px-2 py-1 text-xs font-medium text-brand-800">
                {{ service.duration_minutes }} min
              </span>
            </div>
            <p class="mt-4 text-lg font-bold">{{ formatPrice(service.price) }}</p>
            <div class="mt-4 flex gap-2">
              <AppButton variant="secondary" class="flex-1" @click="openEdit(service)">Editar</AppButton>
              <AppButton variant="danger" @click="remove(service)">Apagar</AppButton>
            </div>
          </AppCard>
        </div>
      </section>
    </div>

    <AppModal :open="modalOpen" :title="editing ? 'Editar serviço' : 'Novo serviço'" @close="modalOpen = false">
      <form class="grid gap-4 sm:grid-cols-2" @submit.prevent="submit">
        <AppInput v-model="form.name" label="Nome" required class="sm:col-span-2" />
        <AppSelect
          v-model="form.service_category_id"
          label="Categoria"
          :options="formCategoryOptions"
          empty-label="Sem categoria"
          class="sm:col-span-2"
        />
        <AppInput v-model="form.price" label="Preço (€)" type="number" step="0.01" required />
        <AppInput v-model="form.duration_minutes" label="Duração (min)" type="number" required />
        <AppTextarea
          v-model="form.description"
          label="Descrição (aparece no agendamento online)"
          :rows="5"
          class="sm:col-span-2"
        />
        <AppImageUpload
          v-model="imageFile"
          v-model:remove="removeImage"
          label="Imagem"
          :current-url="currentImageUrl"
          class="sm:col-span-2"
        />
        <div class="flex flex-col-reverse gap-2 sm:col-span-2 sm:flex-row sm:justify-end">
          <AppButton variant="secondary" type="button" @click="modalOpen = false">Cancelar</AppButton>
          <AppButton type="submit" :loading="saving">Guardar</AppButton>
        </div>
      </form>
    </AppModal>
  </div>
</template>

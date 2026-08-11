<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import AppBadge from '@/components/ui/AppBadge.vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppInput from '@/components/ui/AppInput.vue'
import AppModal from '@/components/ui/AppModal.vue'
import AppSelect from '@/components/ui/AppSelect.vue'
import AppTable from '@/components/ui/AppTable.vue'
import AppTextarea from '@/components/ui/AppTextarea.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingState from '@/components/ui/LoadingState.vue'
import { getErrorMessage } from '@/lib/api'
import {
  createProductCategory,
  createServiceCategory,
  deleteProductCategory,
  deleteServiceCategory,
  fetchProductCategories,
  fetchServiceCategories,
  updateProductCategory,
  updateServiceCategory,
} from '@/services/salonApi'
import type { ProductCategory, ServiceCategory } from '@/types/api'
import { useUiStore } from '@/stores/ui'

type TabKey = 'services' | 'products'
type CategoryRow = ServiceCategory | ProductCategory

const ui = useUiStore()
const activeTab = ref<TabKey>('services')
const categories = ref<CategoryRow[]>([])
const loading = ref(true)
const saving = ref(false)
const search = ref('')
const statusFilter = ref('')
const modalOpen = ref(false)
const editing = ref<CategoryRow | null>(null)
const editingKind = ref<TabKey | null>(null)
let loadRequestId = 0

const form = reactive({
  name: '',
  slug: '',
  description: '',
  sort_order: '0',
  is_active: true,
})

const tabs = [
  { key: 'services' as const, label: 'Serviços' },
  { key: 'products' as const, label: 'Produtos' },
]

const statusOptions = [
  { value: '1', label: 'Ativas' },
  { value: '0', label: 'Inativas' },
]

const itemCountLabel = computed(() =>
  activeTab.value === 'services' ? 'Serviços' : 'Produtos',
)

const tabSubtitle = computed(() =>
  activeTab.value === 'services'
    ? 'Categorias de serviços do catálogo'
    : 'Categorias de produtos do catálogo',
)

const pageTitle = computed(() => {
  const kind = editingKind.value ?? activeTab.value
  const noun = kind === 'services' ? 'serviço' : 'produto'
  return editing.value ? `Editar categoria de ${noun}` : `Nova categoria de ${noun}`
})

const emptyTitle = computed(() =>
  activeTab.value === 'services'
    ? 'Nenhuma categoria de serviço encontrada'
    : 'Nenhuma categoria de produto encontrada',
)

function itemCount(row: CategoryRow, kind: TabKey = activeTab.value) {
  if (kind === 'services') {
    return (row as ServiceCategory).services_count ?? 0
  }
  return (row as ProductCategory).products_count ?? 0
}

function resetForm() {
  form.name = ''
  form.slug = ''
  form.description = ''
  form.sort_order = '0'
  form.is_active = true
  editing.value = null
  editingKind.value = null
}

function closeModal() {
  modalOpen.value = false
  resetForm()
}

function openCreate() {
  resetForm()
  editingKind.value = activeTab.value
  modalOpen.value = true
}

function openEdit(row: CategoryRow) {
  editing.value = row
  editingKind.value = activeTab.value
  form.name = row.name
  form.slug = row.slug
  form.description = row.description ?? ''
  form.sort_order = String(row.sort_order ?? 0)
  form.is_active = row.is_active
  modalOpen.value = true
}

function switchTab(tab: TabKey) {
  if (activeTab.value === tab) return
  activeTab.value = tab
}

async function load() {
  const requestId = ++loadRequestId
  const tab = activeTab.value
  loading.value = true
  categories.value = []

  try {
    const params = {
      per_page: 200,
      search: search.value || undefined,
      is_active: statusFilter.value === '' ? undefined : statusFilter.value === '1',
    }
    const response =
      tab === 'services'
        ? await fetchServiceCategories(params)
        : await fetchProductCategories(params)

    if (requestId !== loadRequestId || activeTab.value !== tab) return

    categories.value = response.data ?? []
  } catch (err) {
    if (requestId !== loadRequestId || activeTab.value !== tab) return
    categories.value = []
    ui.error(getErrorMessage(err))
  } finally {
    if (requestId === loadRequestId) {
      loading.value = false
    }
  }
}

async function submit() {
  const kind = editingKind.value ?? activeTab.value
  saving.value = true
  try {
    const payload = {
      name: form.name,
      slug: form.slug || undefined,
      description: form.description || null,
      sort_order: Number(form.sort_order) || 0,
      is_active: Boolean(form.is_active),
    }

    if (kind === 'services') {
      if (editing.value) {
        await updateServiceCategory(editing.value.id, payload)
        ui.success('Categoria de serviço atualizada.')
      } else {
        await createServiceCategory(payload)
        ui.success('Categoria de serviço criada.')
      }
    } else if (editing.value) {
      await updateProductCategory(editing.value.id, payload)
      ui.success('Categoria de produto atualizada.')
    } else {
      await createProductCategory(payload)
      ui.success('Categoria de produto criada.')
    }

    closeModal()
    if (activeTab.value !== kind) {
      activeTab.value = kind
    } else {
      await load()
    }
  } catch (err) {
    ui.error(getErrorMessage(err))
  } finally {
    saving.value = false
  }
}

async function toggleActive(row: CategoryRow) {
  const kind = activeTab.value
  const next = !row.is_active
  try {
    if (kind === 'services') {
      await updateServiceCategory(row.id, { is_active: next })
    } else {
      await updateProductCategory(row.id, { is_active: next })
    }
    ui.success(next ? 'Categoria ativada.' : 'Categoria desativada.')
    if (activeTab.value === kind) await load()
  } catch (err) {
    ui.error(getErrorMessage(err))
  }
}

async function remove(row: CategoryRow) {
  const kind = activeTab.value
  const count = itemCount(row, kind)
  if (count > 0) {
    ui.error(
      'Esta categoria possui itens associados e não pode ser eliminada. Reatribua os itens ou desative a categoria.',
    )
    return
  }

  const ok = await ui.confirm({
    title: 'Eliminar categoria',
    message: `Eliminar permanentemente "${row.name}"?`,
    confirmLabel: 'Eliminar',
    variant: 'danger',
  })
  if (!ok) return

  try {
    if (kind === 'services') {
      await deleteServiceCategory(row.id)
    } else {
      await deleteProductCategory(row.id)
    }
    ui.success('Categoria eliminada.')
    if (activeTab.value === kind) await load()
  } catch (err) {
    ui.error(getErrorMessage(err))
  }
}

function onSearch() {
  load()
}

watch(activeTab, () => {
  search.value = ''
  statusFilter.value = ''
  closeModal()
  load()
})

onMounted(load)
</script>

<template>
  <div class="w-full space-y-6">
    <div>
      <h1 class="text-2xl font-semibold text-zinc-900">Categorias</h1>
      <p class="mt-1 text-sm text-zinc-600">{{ tabSubtitle }}</p>
    </div>

    <div class="rounded-xl border border-brand-200 bg-brand-50 px-4 py-3 text-sm text-brand-900">
      Categorias estruturadas para serviços e produtos. Itens associados mantêm a relação; categorias
      inativas deixam de aparecer em formulários públicos.
    </div>

    <div class="flex flex-wrap gap-2 border-b border-zinc-200 pb-1">
      <button
        v-for="tab in tabs"
        :key="tab.key"
        type="button"
        class="rounded-t-lg px-4 py-2 text-sm font-medium transition"
        :class="
          activeTab === tab.key
            ? 'bg-white text-brand-800 ring-1 ring-brand-200'
            : 'text-zinc-500 hover:text-zinc-800'
        "
        @click="switchTab(tab.key)"
      >
        {{ tab.label }}
      </button>
    </div>

    <div class="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
      <form class="flex flex-1 flex-wrap gap-2" @submit.prevent="onSearch">
        <AppInput
          v-model="search"
          label="Pesquisar"
          placeholder="Nome ou slug"
          class="min-w-[200px] flex-1"
        />
        <AppSelect
          v-model="statusFilter"
          label="Estado"
          empty-label="Todos os estados"
          :options="statusOptions"
          class="min-w-[160px]"
        />
        <AppButton type="submit" variant="secondary" class="self-end">Filtrar</AppButton>
      </form>
      <AppButton @click="openCreate">Nova categoria</AppButton>
    </div>

    <LoadingState v-if="loading" />
    <EmptyState
      v-else-if="!categories.length"
      :title="emptyTitle"
      description="Crie a primeira categoria para organizar o catálogo."
    />

    <template v-else>
      <div class="hidden md:block">
        <AppTable>
          <thead>
            <tr>
              <th>Ordem</th>
              <th>Nome</th>
              <th>Slug</th>
              <th>{{ itemCountLabel }}</th>
              <th>Estado</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in categories" :key="`${activeTab}-${row.id}`">
              <td>{{ row.sort_order }}</td>
              <td>
                <div class="font-medium text-zinc-900">{{ row.name }}</div>
                <div v-if="row.description" class="mt-0.5 line-clamp-1 text-xs text-zinc-500">
                  {{ row.description }}
                </div>
              </td>
              <td class="font-mono text-xs text-zinc-500">{{ row.slug }}</td>
              <td>{{ itemCount(row) }}</td>
              <td>
                <AppBadge :tone="row.is_active ? 'success' : 'default'">
                  {{ row.is_active ? 'Ativa' : 'Inativa' }}
                </AppBadge>
              </td>
              <td>
                <div class="flex flex-wrap gap-2">
                  <AppButton variant="secondary" @click="openEdit(row)">Editar</AppButton>
                  <AppButton variant="secondary" @click="toggleActive(row)">
                    {{ row.is_active ? 'Desativar' : 'Ativar' }}
                  </AppButton>
                  <AppButton variant="danger" @click="remove(row)">Eliminar</AppButton>
                </div>
              </td>
            </tr>
          </tbody>
        </AppTable>
      </div>

      <div class="grid gap-3 md:hidden">
        <article
          v-for="row in categories"
          :key="`${activeTab}-${row.id}`"
          class="rounded-xl border border-zinc-200 bg-white p-4 shadow-sm"
        >
          <div class="flex items-start justify-between gap-2">
            <div>
              <h3 class="font-semibold text-zinc-900">{{ row.name }}</h3>
              <p class="mt-1 font-mono text-xs text-zinc-500">{{ row.slug }}</p>
            </div>
            <AppBadge :tone="row.is_active ? 'success' : 'default'">
              {{ row.is_active ? 'Ativa' : 'Inativa' }}
            </AppBadge>
          </div>
          <p class="mt-3 text-sm text-zinc-600">
            Ordem {{ row.sort_order }} · {{ itemCount(row) }} {{ itemCountLabel.toLowerCase() }}
          </p>
          <div class="mt-4 flex flex-wrap gap-2">
            <AppButton variant="secondary" class="flex-1" @click="openEdit(row)">Editar</AppButton>
            <AppButton variant="secondary" @click="toggleActive(row)">
              {{ row.is_active ? 'Desativar' : 'Ativar' }}
            </AppButton>
            <AppButton variant="danger" @click="remove(row)">Eliminar</AppButton>
          </div>
        </article>
      </div>
    </template>

    <AppModal :open="modalOpen" :title="pageTitle" @close="closeModal">
      <form class="grid gap-4 sm:grid-cols-2" @submit.prevent="submit">
        <AppInput v-model="form.name" label="Nome" required class="sm:col-span-2" />
        <AppInput
          v-model="form.slug"
          label="Slug (opcional)"
          placeholder="Gerado a partir do nome"
          class="sm:col-span-2"
        />
        <AppInput v-model="form.sort_order" label="Ordem" type="number" min="0" />
        <label class="flex items-end gap-2 pb-2 text-sm text-zinc-700">
          <input v-model="form.is_active" type="checkbox" class="rounded border-zinc-300" />
          Categoria ativa
        </label>
        <AppTextarea
          v-model="form.description"
          label="Descrição"
          :rows="3"
          class="sm:col-span-2"
        />
        <div class="flex flex-col-reverse gap-2 sm:col-span-2 sm:flex-row sm:justify-end">
          <AppButton variant="secondary" type="button" @click="closeModal">Cancelar</AppButton>
          <AppButton type="submit" :loading="saving">Guardar</AppButton>
        </div>
      </form>
    </AppModal>
  </div>
</template>

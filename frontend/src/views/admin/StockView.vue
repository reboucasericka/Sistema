<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import AppBadge from '@/components/ui/AppBadge.vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppCard from '@/components/ui/AppCard.vue'
import AppTable from '@/components/ui/AppTable.vue'
import AppInput from '@/components/ui/AppInput.vue'
import AppModal from '@/components/ui/AppModal.vue'
import AppPagination from '@/components/ui/AppPagination.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingState from '@/components/ui/LoadingState.vue'
import { getErrorMessage } from '@/lib/api'
import {
  fetchProducts,
  fetchStockHistory,
  stockAdjustment,
  stockEntry,
  stockExit,
} from '@/services/salonApi'
import type { Product, StockMovement } from '@/types/api'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'

const auth = useAuthStore()
const ui = useUiStore()
const isAdmin = computed(() => auth.role === 'admin')

const movements = ref<StockMovement[]>([])
const products = ref<Product[]>([])
const loading = ref(true)
const saving = ref(false)
const page = ref(1)
const lastPage = ref(1)
const total = ref(0)
const productFilter = ref('')
const modalOpen = ref(false)
const modalMode = ref<'in' | 'out' | 'adjust'>('in')

const form = reactive({
  product_id: '',
  quantity: '1',
  new_quantity: '0',
  reason: '',
  notes: '',
})

const modalTitles = {
  in: 'Entrada de stock',
  out: 'Saída de stock',
  adjust: 'Ajuste manual de stock',
}

async function loadProducts() {
  try {
    const response = await fetchProducts({ per_page: 100, is_active: true })
    products.value = response.data ?? []
  } catch (err) {
    products.value = []
    ui.error(getErrorMessage(err))
  }
}

async function load() {
  loading.value = true
  try {
    const response = await fetchStockHistory({
      page: page.value,
      per_page: 15,
      product_id: productFilter.value || undefined,
    })
    movements.value = response.data ?? []
    lastPage.value = response.meta?.last_page ?? 1
    total.value = response.meta?.total ?? movements.value.length
  } catch (err) {
    ui.error(getErrorMessage(err))
  } finally {
    loading.value = false
  }
}

function openModal(mode: 'in' | 'out' | 'adjust') {
  modalMode.value = mode
  form.product_id = productFilter.value || (products.value[0]?.id ? String(products.value[0].id) : '')
  form.quantity = '1'
  form.new_quantity = products.value.find((p) => String(p.id) === form.product_id)?.stock_quantity?.toString() ?? '0'
  form.reason = ''
  form.notes = ''
  modalOpen.value = true
}

async function submit() {
  if (!form.product_id) {
    ui.error('Selecione um produto.')
    return
  }
  saving.value = true
  try {
    const productId = Number(form.product_id)
    const payload = {
      product_id: productId,
      reason: form.reason || undefined,
      notes: form.notes || undefined,
    }
    if (modalMode.value === 'in') {
      await stockEntry({ ...payload, quantity: Number(form.quantity) })
      ui.success('Entrada registada.')
    } else if (modalMode.value === 'out') {
      await stockExit({ ...payload, quantity: Number(form.quantity) })
      ui.success('Saída registada.')
    } else {
      await stockAdjustment({ ...payload, new_quantity: Number(form.new_quantity) })
      ui.success('Stock ajustado.')
    }
    modalOpen.value = false
    await loadProducts()
    await load()
  } catch (err) {
    ui.error(getErrorMessage(err))
  } finally {
    saving.value = false
  }
}

function onPageChange(newPage: number) {
  page.value = newPage
  load()
}

function onFilter() {
  page.value = 1
  load()
}

onMounted(async () => {
  await loadProducts()
  await load()
})
</script>

<template>
  <div class="w-full space-y-6">
    <div class="flex flex-col gap-3 lg:flex-row lg:items-end lg:justify-between">
      <form class="flex flex-1 flex-col gap-2 sm:flex-row sm:items-end" @submit.prevent="onFilter">
        <label class="flex-1 text-sm">
          <span class="mb-1 block font-medium text-zinc-700">Filtrar por produto</span>
          <select
            v-model="productFilter"
            class="w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm"
          >
            <option value="">Todos os produtos</option>
            <option v-for="product in products" :key="product.id" :value="product.id">
              {{ product.name }} ({{ product.stock_quantity }})
            </option>
          </select>
        </label>
        <AppButton type="submit" variant="secondary">Aplicar</AppButton>
      </form>
      <div v-if="isAdmin" class="flex flex-wrap gap-2">
        <AppButton variant="secondary" @click="openModal('in')">Entrada</AppButton>
        <AppButton variant="secondary" @click="openModal('out')">Saída</AppButton>
        <AppButton @click="openModal('adjust')">Ajuste</AppButton>
      </div>
    </div>

    <LoadingState v-if="loading" />
    <EmptyState v-else-if="!movements.length" title="Sem movimentos de stock" />

    <AppCard v-else title="Histórico de movimentos">
      <AppTable>
          <thead>
            <tr>
              <th>Data</th>
              <th>Produto</th>
              <th>Tipo</th>
              <th>Qtd</th>
              <th class="hidden sm:table-cell">Antes</th>
              <th>Depois</th>
              <th class="hidden md:table-cell">Motivo</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="movement in movements" :key="movement.id">
              <td class="whitespace-nowrap">{{ new Date(movement.created_at).toLocaleString('pt-PT') }}</td>
              <td>{{ movement.product?.name ?? `#${movement.product_id}` }}</td>
              <td>
                <AppBadge :tone="movement.type === 'in' ? 'success' : movement.type === 'out' ? 'warning' : 'default'">
                  {{ movement.type_label }}
                </AppBadge>
              </td>
              <td>{{ movement.quantity }}</td>
              <td class="hidden sm:table-cell">{{ movement.previous_quantity }}</td>
              <td class="font-medium text-[#1F2937]">{{ movement.new_quantity }}</td>
              <td class="hidden md:table-cell">{{ movement.reason || '—' }}</td>
            </tr>
          </tbody>
        </AppTable>
    </AppCard>

    <AppPagination
      v-if="!loading && movements.length"
      :current-page="page"
      :last-page="lastPage"
      :total="total"
      @change="onPageChange"
    />

    <AppModal :open="modalOpen" :title="modalTitles[modalMode]" @close="modalOpen = false">
      <form class="grid gap-4" @submit.prevent="submit">
        <label class="text-sm">
          <span class="mb-1 block font-medium text-zinc-700">Produto</span>
          <select v-model="form.product_id" class="w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm" required>
            <option value="" disabled>Selecionar...</option>
            <option v-for="product in products" :key="product.id" :value="product.id">
              {{ product.name }} — stock: {{ product.stock_quantity }}
            </option>
          </select>
        </label>
        <AppInput
          v-if="modalMode !== 'adjust'"
          v-model="form.quantity"
          label="Quantidade"
          type="number"
          min="1"
          required
        />
        <AppInput
          v-else
          v-model="form.new_quantity"
          label="Nova quantidade"
          type="number"
          min="0"
          required
        />
        <AppInput v-model="form.reason" label="Motivo" />
        <AppInput v-model="form.notes" label="Notas" />
        <div class="flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
          <AppButton variant="secondary" type="button" @click="modalOpen = false">Cancelar</AppButton>
          <AppButton type="submit" :loading="saving">Confirmar</AppButton>
        </div>
      </form>
    </AppModal>
  </div>
</template>

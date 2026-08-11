<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppCard from '@/components/ui/AppCard.vue'
import AppTable from '@/components/ui/AppTable.vue'
import StatusBadge from '@/components/ui/StatusBadge.vue'
import AppInput from '@/components/ui/AppInput.vue'
import AppModal from '@/components/ui/AppModal.vue'
import AppPagination from '@/components/ui/AppPagination.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingState from '@/components/ui/LoadingState.vue'
import { getErrorMessage } from '@/lib/api'
import {
  cancelSale,
  createSale,
  fetchClients,
  fetchProducts,
  fetchSales,
  fetchServices,
} from '@/services/salonApi'
import type { Client, Product, Sale, Service } from '@/types/api'
import { useUiStore } from '@/stores/ui'

const ui = useUiStore()

const sales = ref<Sale[]>([])
const clients = ref<Client[]>([])
const products = ref<Product[]>([])
const services = ref<Service[]>([])
const loading = ref(true)
const saving = ref(false)
const page = ref(1)
const lastPage = ref(1)
const total = ref(0)
const modalOpen = ref(false)

type LineItem = {
  kind: 'product' | 'service'
  ref_id: string
  quantity: string
  unit_price: string
}

const form = reactive({
  client_id: '',
  payment_method: 'cash',
  status: 'paid',
  notes: '',
  items: [{ kind: 'product' as const, ref_id: '', quantity: '1', unit_price: '' }] as LineItem[],
})

const totalAmount = computed(() =>
  form.items.reduce((sum, item) => {
    const qty = Number(item.quantity) || 0
    const price = Number(item.unit_price) || 0
    return sum + qty * price
  }, 0),
)

function formatPrice(value: string | number) {
  return new Intl.NumberFormat('pt-PT', { style: 'currency', currency: 'EUR' }).format(Number(value))
}

function addLine() {
  form.items.push({ kind: 'product', ref_id: '', quantity: '1', unit_price: '' })
}

function removeLine(index: number) {
  if (form.items.length > 1) form.items.splice(index, 1)
}

function onLineKindChange(item: LineItem) {
  item.ref_id = ''
  item.unit_price = ''
}

function onLineRefChange(item: LineItem) {
  if (item.kind === 'product') {
    const product = products.value.find((p) => String(p.id) === item.ref_id)
    if (product) item.unit_price = String(product.price)
  } else {
    const service = services.value.find((s) => String(s.id) === item.ref_id)
    if (service) item.unit_price = String(service.price)
  }
}

async function loadOptions() {
  try {
    const [clientsRes, productsRes, servicesRes] = await Promise.all([
      fetchClients({ per_page: 100 }),
      fetchProducts({ per_page: 100, is_active: true }),
      fetchServices({ per_page: 100, is_active: true }),
    ])
    clients.value = clientsRes.data ?? []
    products.value = productsRes.data ?? []
    services.value = servicesRes.data ?? []
  } catch (err) {
    ui.error(getErrorMessage(err))
  }
}

async function load() {
  loading.value = true
  try {
    const response = await fetchSales({ page: page.value, per_page: 15 })
    sales.value = response.data ?? []
    lastPage.value = response.meta?.last_page ?? 1
    total.value = response.meta?.total ?? sales.value.length
  } catch (err) {
    ui.error(getErrorMessage(err))
  } finally {
    loading.value = false
  }
}

function openCreate() {
  form.client_id = ''
  form.payment_method = 'cash'
  form.status = 'paid'
  form.notes = ''
  form.items = [{ kind: 'product', ref_id: '', quantity: '1', unit_price: '' }]
  modalOpen.value = true
}

async function submit() {
  const items = form.items
    .filter((item) => item.ref_id)
    .map((item) => ({
      product_id: item.kind === 'product' ? Number(item.ref_id) : undefined,
      service_id: item.kind === 'service' ? Number(item.ref_id) : undefined,
      quantity: Number(item.quantity) || 1,
      unit_price: item.unit_price || undefined,
    }))

  if (!items.length) {
    ui.error('Adicione pelo menos um item à venda.')
    return
  }

  saving.value = true
  try {
    await createSale({
      client_id: form.client_id ? Number(form.client_id) : null,
      payment_method: form.payment_method,
      status: form.status,
      notes: form.notes || undefined,
      items,
    })
    ui.success('Venda registada.')
    modalOpen.value = false
    await load()
  } catch (err) {
    ui.error(getErrorMessage(err))
  } finally {
    saving.value = false
  }
}

async function cancel(sale: Sale) {
  const ok = await ui.confirm({
    title: 'Cancelar venda',
    message: `Cancelar venda #${sale.id}? O stock dos produtos será reposto.`,
    confirmLabel: 'Cancelar venda',
    variant: 'danger',
  })
  if (!ok) return
  try {
    await cancelSale(sale.id)
    ui.success('Venda cancelada.')
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
  await loadOptions()
  await load()
})
</script>

<template>
  <div class="w-full space-y-6">
    <div class="flex justify-end">
      <AppButton @click="openCreate">Nova venda</AppButton>
    </div>

    <LoadingState v-if="loading" />
    <EmptyState v-else-if="!sales.length" title="Nenhuma venda registada" />

    <AppCard v-else title="Vendas">
      <AppTable>
          <thead>
            <tr>
              <th>#</th>
              <th>Data</th>
              <th class="hidden sm:table-cell">Cliente</th>
              <th>Total</th>
              <th class="hidden md:table-cell">Pagamento</th>
              <th>Estado</th>
              <th class="text-right">Ações</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="sale in sales" :key="sale.id">
              <td>{{ sale.id }}</td>
              <td class="whitespace-nowrap">{{ new Date(sale.created_at).toLocaleString('pt-PT') }}</td>
              <td class="hidden sm:table-cell">{{ sale.client?.name ?? '—' }}</td>
              <td class="font-semibold text-[#1F2937]">{{ formatPrice(sale.total_amount) }}</td>
              <td class="hidden md:table-cell">{{ sale.payment_method_label }}</td>
              <td><StatusBadge :status="sale.status" :label="sale.status_label" /></td>
              <td>
                <div class="flex justify-end">
                  <AppButton
                    v-if="sale.status !== 'cancelled'"
                    variant="danger"
                    size="sm"
                    @click="cancel(sale)"
                  >
                    Cancelar
                  </AppButton>
                </div>
              </td>
            </tr>
          </tbody>
        </AppTable>
    </AppCard>

    <AppPagination
      v-if="!loading && sales.length"
      :current-page="page"
      :last-page="lastPage"
      :total="total"
      @change="onPageChange"
    />

    <AppModal :open="modalOpen" title="Nova venda" @close="modalOpen = false">
      <form class="space-y-4" @submit.prevent="submit">
        <div class="grid gap-4 sm:grid-cols-2">
          <label class="text-sm">
            <span class="mb-1 block font-medium text-zinc-700">Cliente (opcional)</span>
            <select v-model="form.client_id" class="w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm">
              <option value="">Sem cliente</option>
              <option v-for="client in clients" :key="client.id" :value="client.id">{{ client.name }}</option>
            </select>
          </label>
          <label class="text-sm">
            <span class="mb-1 block font-medium text-zinc-700">Método de pagamento</span>
            <select v-model="form.payment_method" class="w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm">
              <option value="cash">Dinheiro</option>
              <option value="card">Cartão</option>
              <option value="transfer">Transferência</option>
              <option value="other">Outro</option>
            </select>
          </label>
        </div>

        <div class="space-y-3">
          <div class="flex items-center justify-between">
            <h3 class="text-sm font-semibold text-zinc-800">Itens</h3>
            <AppButton type="button" variant="secondary" @click="addLine">Adicionar linha</AppButton>
          </div>
          <div
            v-for="(item, index) in form.items"
            :key="index"
            class="grid gap-2 rounded-lg border border-zinc-200 p-3 sm:grid-cols-5"
          >
            <select
              v-model="item.kind"
              class="rounded-lg border border-zinc-300 px-2 py-2 text-sm"
              @change="onLineKindChange(item)"
            >
              <option value="product">Produto</option>
              <option value="service">Serviço</option>
            </select>
            <select
              v-model="item.ref_id"
              class="rounded-lg border border-zinc-300 px-2 py-2 text-sm sm:col-span-2"
              required
              @change="onLineRefChange(item)"
            >
              <option value="" disabled>Selecionar...</option>
              <template v-if="item.kind === 'product'">
                <option v-for="product in products" :key="product.id" :value="product.id">
                  {{ product.name }} (stock: {{ product.stock_quantity }})
                </option>
              </template>
              <template v-else>
                <option v-for="service in services" :key="service.id" :value="service.id">
                  {{ service.name }}
                </option>
              </template>
            </select>
            <AppInput v-model="item.quantity" label="Qtd" type="number" min="1" />
            <div class="flex items-end gap-2">
              <AppInput v-model="item.unit_price" label="Preço" type="number" step="0.01" class="flex-1" />
              <AppButton v-if="form.items.length > 1" type="button" variant="ghost" @click="removeLine(index)">×</AppButton>
            </div>
          </div>
        </div>

        <div class="flex items-center justify-between rounded-lg bg-zinc-50 px-4 py-3">
          <span class="font-medium text-zinc-700">Total</span>
          <span class="text-xl font-bold text-zinc-900">{{ formatPrice(totalAmount) }}</span>
        </div>

        <AppInput v-model="form.notes" label="Notas" />

        <div class="flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
          <AppButton variant="secondary" type="button" @click="modalOpen = false">Fechar</AppButton>
          <AppButton type="submit" :loading="saving">Registar venda</AppButton>
        </div>
      </form>
    </AppModal>
  </div>
</template>

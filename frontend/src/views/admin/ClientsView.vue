<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import AppBadge from '@/components/ui/AppBadge.vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppCard from '@/components/ui/AppCard.vue'
import AppInput from '@/components/ui/AppInput.vue'
import AppTable from '@/components/ui/AppTable.vue'
import AppModal from '@/components/ui/AppModal.vue'
import AppPagination from '@/components/ui/AppPagination.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingState from '@/components/ui/LoadingState.vue'
import { getErrorMessage } from '@/lib/api'
import { IMAGES } from '@/lib/images'
import {
  createClient,
  deleteClient,
  fetchClients,
  updateClient,
} from '@/services/salonApi'
import type { Client } from '@/types/api'
import { useUiStore } from '@/stores/ui'

const ui = useUiStore()
const clients = ref<Client[]>([])
const loading = ref(true)
const saving = ref(false)
const page = ref(1)
const lastPage = ref(1)
const total = ref(0)
const search = ref('')
const modalOpen = ref(false)
const editing = ref<Client | null>(null)

const form = reactive({
  name: '',
  email: '',
  phone: '',
  address: '',
  notes: '',
})

function resetForm() {
  form.name = ''
  form.email = ''
  form.phone = ''
  form.address = ''
  form.notes = ''
  editing.value = null
}

function openCreate() {
  resetForm()
  modalOpen.value = true
}

function openEdit(client: Client) {
  editing.value = client
  form.name = client.name
  form.email = client.email ?? ''
  form.phone = client.phone ?? ''
  form.address = client.address ?? ''
  form.notes = client.notes ?? ''
  modalOpen.value = true
}

async function load() {
  loading.value = true
  try {
    const response = await fetchClients({
      page: page.value,
      per_page: 10,
      search: search.value || undefined,
    })
    clients.value = response.data ?? []
    lastPage.value = response.meta?.last_page ?? 1
    total.value = response.meta?.total ?? clients.value.length
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
      email: form.email || null,
      phone: form.phone || null,
      address: form.address || null,
      notes: form.notes || null,
    }
    if (editing.value) {
      await updateClient(editing.value.id, payload)
      ui.success('Cliente atualizado com sucesso.')
    } else {
      await createClient(payload)
      ui.success('Cliente criado com sucesso.')
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

async function remove(client: Client) {
  const ok = await ui.confirm({
    title: 'Remover cliente',
    message: `Tem a certeza que deseja remover ${client.name}?`,
    confirmLabel: 'Remover',
    variant: 'danger',
  })
  if (!ok) return
  try {
    await deleteClient(client.id)
    ui.success('Cliente removido.')
    await load()
  } catch (err) {
    ui.error(getErrorMessage(err))
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

onMounted(load)
</script>

<template>
  <div class="w-full space-y-6">
    <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      <AppInput v-model="search" label="Pesquisar" placeholder="Nome, email ou telefone" @update:model-value="onSearch" />
      <AppButton class="shrink-0" @click="openCreate">Novo cliente</AppButton>
    </div>

    <AppCard>
      <LoadingState v-if="loading" />
      <EmptyState v-else-if="!clients.length" title="Nenhum cliente encontrado" />
      <AppTable v-else>
          <thead>
            <tr>
              <th>Cliente</th>
              <th>Contacto</th>
              <th class="hidden sm:table-cell">Estado</th>
              <th class="text-right">Ações</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="client in clients" :key="client.id">
              <td>
                <div class="flex items-center gap-3">
                  <img :src="IMAGES.avatar" alt="" class="h-8 w-8 rounded-full object-cover" />
                  <span>{{ client.name }}</span>
                </div>
              </td>
              <td>
                <p>{{ client.email ?? '—' }}</p>
                <p class="text-xs">{{ client.phone ?? '—' }}</p>
              </td>
              <td class="hidden sm:table-cell">
                <AppBadge :tone="client.is_active ? 'success' : 'default'">
                  {{ client.is_active ? 'Ativo' : 'Inativo' }}
                </AppBadge>
              </td>
              <td>
                <div class="flex justify-end gap-1.5">
                  <AppButton variant="secondary" size="sm" @click="openEdit(client)">Editar</AppButton>
                  <AppButton variant="danger" size="sm" @click="remove(client)">Apagar</AppButton>
                </div>
              </td>
            </tr>
          </tbody>
        </AppTable>
      <AppPagination
        v-if="!loading && clients.length"
        :current-page="page"
        :last-page="lastPage"
        :total="total"
        @change="onPageChange"
      />
    </AppCard>

    <AppModal
      :open="modalOpen"
      :title="editing ? 'Editar cliente' : 'Novo cliente'"
      @close="modalOpen = false"
    >
      <form class="grid gap-4" @submit.prevent="submit">
        <AppInput v-model="form.name" label="Nome" required />
        <AppInput v-model="form.email" label="Email" type="email" />
        <AppInput v-model="form.phone" label="Telefone" />
        <AppInput v-model="form.address" label="Morada" />
        <AppInput v-model="form.notes" label="Notas" />
        <div class="flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
          <AppButton variant="secondary" type="button" @click="modalOpen = false">Cancelar</AppButton>
          <AppButton type="submit" :loading="saving">Guardar</AppButton>
        </div>
      </form>
    </AppModal>
  </div>
</template>

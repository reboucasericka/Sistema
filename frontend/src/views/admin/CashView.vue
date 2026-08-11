<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppCard from '@/components/ui/AppCard.vue'
import AppInput from '@/components/ui/AppInput.vue'
import AppModal from '@/components/ui/AppModal.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingState from '@/components/ui/LoadingState.vue'
import { getErrorMessage } from '@/lib/api'
import {
  closeCashRegister,
  fetchCashReport,
  fetchCurrentCashRegister,
  openCashRegister,
  recordCashExpense,
  recordCashIncome,
} from '@/services/salonApi'
import type { CashRegisterSummary, CashReport } from '@/types/api'
import { useUiStore } from '@/stores/ui'

const ui = useUiStore()

const loading = ref(true)
const saving = ref(false)
const current = ref<CashRegisterSummary | null>(null)
const report = ref<CashReport | null>(null)
const openModal = ref(false)
const closeModal = ref(false)
const incomeModal = ref(false)
const expenseModal = ref(false)

const openForm = reactive({ opening_amount: '100' })
const closeForm = reactive({ closing_amount: '' })
const incomeForm = reactive({ amount: '', description: '' })
const expenseForm = reactive({ amount: '', description: '' })

const isOpen = computed(() => current.value?.status === 'open')

function formatMoney(value: number | string | null | undefined) {
  return new Intl.NumberFormat('pt-PT', { style: 'currency', currency: 'EUR' }).format(Number(value ?? 0))
}

async function load() {
  loading.value = true
  try {
    current.value = await fetchCurrentCashRegister()
    report.value = await fetchCashReport()
    if (current.value) {
      closeForm.closing_amount = String(current.value.expected_balance)
    }
  } catch (err) {
    ui.error(getErrorMessage(err))
  } finally {
    loading.value = false
  }
}

async function submitOpen() {
  saving.value = true
  try {
    await openCashRegister(Number(openForm.opening_amount))
    ui.success('Caixa aberto com sucesso.')
    openModal.value = false
    await load()
  } catch (err) {
    ui.error(getErrorMessage(err))
  } finally {
    saving.value = false
  }
}

async function submitClose() {
  saving.value = true
  try {
    await closeCashRegister(Number(closeForm.closing_amount))
    ui.success('Caixa fechado com sucesso.')
    closeModal.value = false
    await load()
  } catch (err) {
    ui.error(getErrorMessage(err))
  } finally {
    saving.value = false
  }
}

async function submitIncome() {
  saving.value = true
  try {
    await recordCashIncome(Number(incomeForm.amount), incomeForm.description || undefined)
    ui.success('Receita registada.')
    incomeModal.value = false
    incomeForm.amount = ''
    incomeForm.description = ''
    await load()
  } catch (err) {
    ui.error(getErrorMessage(err))
  } finally {
    saving.value = false
  }
}

async function submitExpense() {
  saving.value = true
  try {
    await recordCashExpense(Number(expenseForm.amount), expenseForm.description || undefined)
    ui.success('Despesa registada.')
    expenseModal.value = false
    expenseForm.amount = ''
    expenseForm.description = ''
    await load()
  } catch (err) {
    ui.error(getErrorMessage(err))
  } finally {
    saving.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="w-full space-y-6">
    <div class="flex flex-wrap gap-3">
      <AppButton v-if="!isOpen" @click="openModal = true">Abrir caixa</AppButton>
      <template v-else>
        <AppButton @click="incomeModal = true">Registar receita</AppButton>
        <AppButton variant="secondary" @click="expenseModal = true">Registar despesa</AppButton>
        <AppButton variant="danger" @click="closeModal = true">Fechar caixa</AppButton>
      </template>
      <AppButton variant="ghost" @click="load">Atualizar</AppButton>
    </div>

    <LoadingState v-if="loading" />

    <template v-else>
      <AppCard v-if="current" class="p-5">
        <div class="flex flex-wrap items-start justify-between gap-4">
          <div>
            <p class="text-sm text-zinc-500">Estado do caixa</p>
            <h2 class="text-2xl font-bold text-zinc-900">{{ current.status_label }}</h2>
            <p class="mt-2 text-sm text-zinc-600">Aberto por {{ current.opened_by }} · {{ current.opened_at }}</p>
          </div>
          <div class="grid grid-cols-2 gap-4 sm:grid-cols-4">
            <div>
              <p class="text-xs text-zinc-500">Abertura</p>
              <p class="font-semibold">{{ formatMoney(current.opening_amount) }}</p>
            </div>
            <div>
              <p class="text-xs text-zinc-500">Receitas</p>
              <p class="font-semibold text-emerald-700">{{ formatMoney(current.income_total) }}</p>
            </div>
            <div>
              <p class="text-xs text-zinc-500">Despesas</p>
              <p class="font-semibold text-red-700">{{ formatMoney(current.expense_total) }}</p>
            </div>
            <div>
              <p class="text-xs text-zinc-500">Saldo esperado</p>
              <p class="font-semibold text-brand-700">{{ formatMoney(current.expected_balance) }}</p>
            </div>
          </div>
        </div>
      </AppCard>

      <EmptyState
        v-else
        title="Caixa fechado"
        description="Abra o caixa para registar receitas e despesas do dia."
      />

      <AppCard v-if="report" class="p-5">
        <h3 class="mb-4 text-lg font-semibold text-zinc-900">Resumo do dia ({{ report.date }})</h3>
        <div class="grid grid-cols-2 gap-4 sm:grid-cols-5">
          <div>
            <p class="text-xs text-zinc-500">Aberturas</p>
            <p class="font-semibold">{{ formatMoney(report.totals.opening_amount) }}</p>
          </div>
          <div>
            <p class="text-xs text-zinc-500">Receitas</p>
            <p class="font-semibold">{{ formatMoney(report.totals.income) }}</p>
          </div>
          <div>
            <p class="text-xs text-zinc-500">Despesas</p>
            <p class="font-semibold">{{ formatMoney(report.totals.expense) }}</p>
          </div>
          <div>
            <p class="text-xs text-zinc-500">Saldo esperado</p>
            <p class="font-semibold">{{ formatMoney(report.totals.expected_balance) }}</p>
          </div>
          <div>
            <p class="text-xs text-zinc-500">Fecho</p>
            <p class="font-semibold">{{ formatMoney(report.totals.closing_amount) }}</p>
          </div>
        </div>
      </AppCard>
    </template>

    <AppModal :open="openModal" title="Abrir caixa" @close="openModal = false">
      <form class="space-y-4" @submit.prevent="submitOpen">
        <AppInput v-model="openForm.opening_amount" label="Valor de abertura (€)" type="number" step="0.01" required />
        <AppButton type="submit" :loading="saving">Confirmar abertura</AppButton>
      </form>
    </AppModal>

    <AppModal :open="closeModal" title="Fechar caixa" @close="closeModal = false">
      <form class="space-y-4" @submit.prevent="submitClose">
        <AppInput v-model="closeForm.closing_amount" label="Valor em caixa (€)" type="number" step="0.01" required />
        <AppButton type="submit" variant="danger" :loading="saving">Confirmar fecho</AppButton>
      </form>
    </AppModal>

    <AppModal :open="incomeModal" title="Registar receita" @close="incomeModal = false">
      <form class="space-y-4" @submit.prevent="submitIncome">
        <AppInput v-model="incomeForm.amount" label="Valor (€)" type="number" step="0.01" required />
        <AppInput v-model="incomeForm.description" label="Descrição" />
        <AppButton type="submit" :loading="saving">Registar</AppButton>
      </form>
    </AppModal>

    <AppModal :open="expenseModal" title="Registar despesa" @close="expenseModal = false">
      <form class="space-y-4" @submit.prevent="submitExpense">
        <AppInput v-model="expenseForm.amount" label="Valor (€)" type="number" step="0.01" required />
        <AppInput v-model="expenseForm.description" label="Descrição" />
        <AppButton type="submit" variant="danger" :loading="saving">Registar</AppButton>
      </form>
    </AppModal>
  </div>
</template>

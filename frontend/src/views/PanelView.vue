<script setup lang="ts">
import { RouterLink } from 'vue-router'
import AppButton from '@/components/ui/AppButton.vue'
import { IMAGES } from '@/lib/images'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'
import '@/styles/admin.css'

const auth = useAuthStore()
const ui = useUiStore()

const modules = [
  {
    to: { name: 'admin-dashboard' },
    title: 'Dashboard',
    description: 'Visão geral, KPIs, agendamentos e alertas.',
    roles: ['admin', 'professional'],
  },
  {
    to: { name: 'admin-professionals' },
    title: 'Profissionais',
    description: 'Equipa, especialidades e serviços associados.',
    roles: ['admin'],
  },
  {
    to: { name: 'admin-appointments' },
    title: 'Agendamentos',
    description: 'Calendário, confirmações e novos agendamentos.',
    roles: ['admin', 'professional'],
  },
  {
    to: { name: 'admin-professional-schedules' },
    title: 'Horários',
    description: 'Disponibilidade semanal dos profissionais.',
    roles: ['admin'],
  },
  {
    to: { name: 'admin-clients' },
    title: 'Clientes',
    description: 'Base de clientes e histórico.',
    roles: ['admin', 'professional'],
  },
  {
    to: { name: 'admin-categories' },
    title: 'Categorias',
    description: 'Categorias de serviços e produtos.',
    roles: ['admin'],
  },
  {
    to: { name: 'admin-services' },
    title: 'Serviços',
    description: 'Catálogo do agendamento online.',
    roles: ['admin'],
  },
  {
    to: { name: 'admin-products' },
    title: 'Produtos',
    description: 'Catálogo de produtos e preços.',
    roles: ['admin', 'professional'],
  },
  {
    to: { name: 'admin-stock' },
    title: 'Stock',
    description: 'Entradas, saídas e movimentações.',
    roles: ['admin', 'professional'],
  },
  {
    to: { name: 'admin-sales' },
    title: 'Vendas',
    description: 'PDV e histórico de vendas.',
    roles: ['admin', 'professional'],
  },
  {
    to: { name: 'admin-cash' },
    title: 'Caixa',
    description: 'Abertura, fecho e movimentos.',
    roles: ['admin', 'professional'],
  },
  {
    to: { name: 'admin-messages' },
    title: 'Mensagens',
    description: 'Contactos do site público.',
    roles: ['admin'],
  },
  {
    to: '/booking',
    title: 'Agendamento online',
    description: 'Ver página pública de marcações.',
    roles: ['admin', 'professional'],
    external: true,
  },
]

const visibleModules = modules.filter((m) => auth.role && m.roles.includes(auth.role))

async function handleLogout() {
  await auth.logout()
  ui.info('Sessão terminada.')
  window.location.href = '/login'
}
</script>

<template>
  <div class="admin-panel min-h-screen bg-[#F8F9FA]">
    <header class="border-b border-[#E5E7EB] bg-white">
      <div class="mx-auto flex max-w-6xl items-center justify-between gap-4 px-4 py-3 sm:px-6">
        <div class="flex items-center gap-3">
          <img :src="IMAGES.logo" alt="Salon System" class="h-8 w-auto" />
          <div>
            <p class="text-sm font-semibold text-[#1F2937]">Salon System</p>
            <p class="text-xs text-[#6B7280]">Centro de controlo</p>
          </div>
        </div>
        <div class="flex items-center gap-2">
          <span class="hidden text-sm text-[#6B7280] sm:inline">{{ auth.fullName }}</span>
          <RouterLink to="/">
            <AppButton variant="secondary" size="sm">Site</AppButton>
          </RouterLink>
          <AppButton variant="ghost" size="sm" @click="handleLogout">Sair</AppButton>
        </div>
      </div>
    </header>

    <main class="mx-auto max-w-6xl px-4 py-8 sm:px-6">
      <div class="mb-8 rounded-xl border border-[#E5E7EB] bg-white p-6 shadow-sm">
        <p class="text-sm text-[#6B7280]">Olá, {{ auth.fullName }}</p>
        <h1 class="mt-1 text-2xl font-semibold text-[#1F2937]">Escolha um módulo</h1>
        <p class="mt-2 text-sm text-[#6B7280]">
          Perfil: <span class="font-medium text-[#374151]">{{ auth.user?.role_label }}</span>
          · {{ auth.user?.email }}
        </p>
      </div>

      <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <RouterLink
          v-for="mod in visibleModules.filter((m) => !m.external)"
          :key="mod.title"
          :to="mod.to"
          class="group rounded-xl border border-[#E5E7EB] bg-white p-5 shadow-sm transition hover:border-[#E6C6B6] hover:shadow-md"
        >
          <h2 class="text-base font-semibold text-[#1F2937] group-hover:text-brand-700">
            {{ mod.title }}
          </h2>
          <p class="mt-1 text-sm text-[#6B7280]">{{ mod.description }}</p>
          <p class="mt-4 text-xs font-medium text-brand-600">Abrir →</p>
        </RouterLink>
        <a
          v-for="mod in visibleModules.filter((m) => m.external)"
          :key="mod.title"
          :href="typeof mod.to === 'string' ? mod.to : '/booking'"
          target="_blank"
          rel="noopener"
          class="admin-btn-link group rounded-xl border border-[#E5E7EB] bg-white p-5 shadow-sm transition hover:border-[#E6C6B6] hover:shadow-md"
        >
          <h2 class="text-base font-semibold text-[#1F2937] group-hover:text-brand-700">
            {{ mod.title }}
          </h2>
          <p class="mt-1 text-sm text-[#6B7280]">{{ mod.description }}</p>
          <p class="mt-4 text-xs font-medium text-brand-600">Abrir ↗</p>
        </a>
      </div>
    </main>
  </div>
</template>

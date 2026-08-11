<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink, useRoute, useRouter, type RouteLocationRaw } from 'vue-router'
import AppButton from '@/components/ui/AppButton.vue'
import { IMAGES } from '@/lib/images'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'

type NavItem = {
  to: RouteLocationRaw
  label: string
  roles: string[]
  exact?: boolean
}

type NavGroup = {
  label: string
  items: NavItem[]
}

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const ui = useUiStore()
const mobileOpen = ref(false)

const dashboardLink: NavItem = {
  to: { name: 'admin-dashboard' },
  label: 'Dashboard',
  roles: ['admin', 'professional'],
  exact: true,
}

const navGroups: NavGroup[] = [
  {
    label: 'Administração',
    items: [{ to: { name: 'admin-professionals' }, label: 'Profissionais', roles: ['admin'] }],
  },
  {
    label: 'Operacional',
    items: [
      { to: { name: 'admin-appointments' }, label: 'Agendamentos', roles: ['admin', 'professional'] },
      { to: { name: 'admin-professional-schedules' }, label: 'Horários', roles: ['admin'] },
      { to: { name: 'admin-clients' }, label: 'Clientes', roles: ['admin', 'professional'] },
    ],
  },
  {
    label: 'Catálogo',
    items: [
      { to: { name: 'admin-categories' }, label: 'Categorias', roles: ['admin'] },
      { to: { name: 'admin-services' }, label: 'Serviços', roles: ['admin'] },
      { to: { name: 'admin-products' }, label: 'Produtos', roles: ['admin', 'professional'] },
      { to: { name: 'admin-stock' }, label: 'Stock', roles: ['admin', 'professional'] },
    ],
  },
  {
    label: 'Comercial',
    items: [
      { to: { name: 'admin-sales' }, label: 'Vendas', roles: ['admin', 'professional'] },
      { to: { name: 'admin-cash' }, label: 'Caixa', roles: ['admin', 'professional'] },
    ],
  },
  {
    label: 'Comunicação',
    items: [{ to: { name: 'admin-messages' }, label: 'Mensagens', roles: ['admin'] }],
  },
]

const showDashboard = computed(() => auth.role && dashboardLink.roles.includes(auth.role))

const visibleGroups = computed(() =>
  navGroups
    .map((group) => ({
      ...group,
      items: group.items.filter((item) => auth.role && item.roles.includes(auth.role)),
    }))
    .filter((group) => group.items.length > 0),
)

function linkKey(link: NavItem) {
  if (typeof link.to === 'string') return link.to
  if (typeof link.to === 'object' && link.to && 'name' in link.to && link.to.name != null) {
    return String(link.to.name)
  }
  return JSON.stringify(link.to)
}

function isActive(link: NavItem) {
  const path = router.resolve(link.to).path
  if (link.exact) return route.path === path
  return route.path === path || route.path.startsWith(`${path}/`)
}

async function handleLogout() {
  await auth.logout()
  ui.info('Sessão terminada.')
  window.location.href = '/login'
}
</script>

<template>
  <div class="admin-panel flex min-h-screen w-full overflow-x-hidden bg-[#F8F9FA]">
    <aside
      class="admin-sidebar hidden h-screen w-[280px] shrink-0 flex-col overflow-y-auto border-r border-[#E6C6B6]/40 bg-[#F7F5F2] lg:sticky lg:top-0 lg:flex"
    >
      <div class="shrink-0 border-b border-[#E6C6B6]/30 px-5 py-5">
        <RouterLink :to="{ name: 'admin-dashboard' }" class="admin-nav-link block">
          <img :src="IMAGES.logo" alt="Salon" class="h-9 w-auto" />
        </RouterLink>
      </div>

      <nav class="min-h-0 flex-1 px-4 py-5 text-left">
        <ul v-if="showDashboard" class="mb-6 space-y-0.5">
          <li>
            <RouterLink
              :to="dashboardLink.to"
              class="admin-nav-link block rounded-[10px] px-3 py-2.5 text-[15px] font-medium"
              :class="isActive(dashboardLink) ? 'is-active' : ''"
            >
              {{ dashboardLink.label }}
            </RouterLink>
          </li>
        </ul>

        <div v-for="group in visibleGroups" :key="group.label" class="mb-6">
          <p class="admin-nav-group mb-2 px-3">
            {{ group.label }}
          </p>
          <ul class="space-y-0.5">
            <li v-for="item in group.items" :key="linkKey(item)">
              <RouterLink
                :to="item.to"
                class="admin-nav-link block rounded-[10px] px-3 py-2.5 text-[15px] font-medium"
                :class="isActive(item) ? 'is-active' : ''"
              >
                {{ item.label }}
              </RouterLink>
            </li>
          </ul>
        </div>
      </nav>

      <div class="shrink-0 border-t border-[#E6C6B6]/30 p-4">
        <div
          class="flex items-center gap-3 rounded-xl border border-[#E6C6B6]/40 bg-white/70 p-3 text-center"
        >
          <img
            :src="IMAGES.avatar"
            alt=""
            class="h-10 w-10 rounded-full object-cover ring-1 ring-[#E6C6B6]/50"
          />
          <div class="min-w-0 flex-1">
            <p class="truncate text-sm font-semibold text-[#1F2937]">{{ auth.fullName }}</p>
            <p class="truncate text-xs text-[#6B7280]">{{ auth.user?.role_label }}</p>
          </div>
        </div>
        <div class="mt-3 grid grid-cols-2 gap-2">
          <RouterLink
            to="/"
            class="rounded-lg border border-[#E6C6B6]/50 bg-white/80 px-2 py-2.5 text-center text-sm font-medium text-[#6B7280] transition hover:border-[#C97D5D]/40 hover:bg-white hover:text-[#C97D5D]"
          >
            Home
          </RouterLink>
          <button
            type="button"
            class="rounded-lg border border-[#E6C6B6]/50 bg-white/80 px-2 py-2.5 text-sm font-medium text-[#6B7280] transition hover:border-red-200 hover:bg-white hover:text-red-600"
            @click="handleLogout"
          >
            Sair
          </button>
        </div>
      </div>
    </aside>

    <div class="admin-content-area flex min-w-0 flex-1 flex-col">
      <header
        class="sticky top-0 z-20 border-b border-[#E5E7EB] bg-[#F7F5F2]/95 backdrop-blur lg:hidden"
      >
        <div class="flex items-center justify-between px-4 py-2.5">
          <img :src="IMAGES.logo" alt="Salon" class="h-7 w-auto" />
          <button
            type="button"
            class="rounded-lg border border-[#E6C6B6]/50 bg-white px-3 py-1.5 text-sm font-medium text-[#374151]"
            @click="mobileOpen = !mobileOpen"
          >
            Menu
          </button>
        </div>
        <nav
          v-if="mobileOpen"
          class="max-h-[70vh] overflow-y-auto border-t border-[#E6C6B6]/30 bg-[#F7F5F2] px-3 py-3"
        >
          <RouterLink
            v-if="showDashboard"
            :to="dashboardLink.to"
            class="admin-nav-link block rounded-[10px] px-3 py-2.5 text-[15px] font-medium"
            :class="isActive(dashboardLink) ? 'is-active' : ''"
            @click="mobileOpen = false"
          >
            {{ dashboardLink.label }}
          </RouterLink>
          <template v-for="group in visibleGroups" :key="group.label">
            <p class="admin-nav-group px-3 pb-1 pt-4">
              {{ group.label }}
            </p>
            <RouterLink
              v-for="item in group.items"
              :key="linkKey(item)"
              :to="item.to"
              class="admin-nav-link block rounded-[10px] px-3 py-2.5 text-[15px] font-medium"
              :class="isActive(item) ? 'is-active' : ''"
              @click="mobileOpen = false"
            >
              {{ item.label }}
            </RouterLink>
          </template>
          <div class="mt-3 grid grid-cols-2 gap-2">
            <RouterLink
              to="/"
              class="admin-btn-link rounded-lg border border-[#E6C6B6]/50 bg-white py-2 text-center text-sm font-medium text-[#6B7280]"
              @click="mobileOpen = false"
            >
              Site
            </RouterLink>
            <AppButton variant="ghost" size="sm" class="w-full" @click="handleLogout"
              >Sair</AppButton
            >
          </div>
        </nav>
      </header>

      <main class="admin-content-bg min-w-0 flex-1 px-6 py-6 lg:px-10 lg:py-8">
        <slot />
      </main>
    </div>
  </div>
</template>

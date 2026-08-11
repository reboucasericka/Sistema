<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import AppButton from '@/components/ui/AppButton.vue'
import { IMAGES } from '@/lib/images'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'

defineProps<{
  title: string
}>()

const route = useRoute()
const auth = useAuthStore()
const ui = useUiStore()
const menuOpen = ref(false)

const links = [
  { to: '/panel/client/dashboard', label: 'Início' },
  { to: '/panel/client/appointments', label: 'Meus agendamentos' },
]

async function handleLogout() {
  await auth.logout()
  ui.info('Sessão terminada.')
  window.location.href = '/login'
}
</script>

<template>
  <div class="min-h-screen bg-gradient-to-br from-brand-50 via-white to-zinc-50">
    <header class="sticky top-0 z-20 border-b border-zinc-200 bg-white/90 backdrop-blur">
      <div class="mx-auto flex max-w-5xl items-center justify-between gap-4 px-4 py-3 sm:px-6">
        <div class="flex items-center gap-3">
          <img :src="IMAGES.logo" alt="Salon System" class="h-9 w-auto" />
          <div class="hidden sm:block">
            <p class="text-sm font-bold text-brand-700">Salon System</p>
            <p class="text-xs text-zinc-500">Área do cliente</p>
          </div>
        </div>
        <button
          type="button"
          class="rounded-lg px-3 py-2 text-sm text-zinc-700 hover:bg-zinc-100 sm:hidden"
          @click="menuOpen = !menuOpen"
        >
          Menu
        </button>
        <nav class="hidden gap-1 sm:flex">
          <RouterLink
            v-for="link in links"
            :key="link.to"
            :to="link.to"
            class="rounded-lg px-3 py-2 text-sm font-medium"
            :class="route.path === link.to ? 'bg-brand-100 text-brand-800' : 'text-zinc-600 hover:bg-zinc-100'"
          >
            {{ link.label }}
          </RouterLink>
          <AppButton variant="ghost" @click="handleLogout">Sair</AppButton>
        </nav>
      </div>
      <nav v-if="menuOpen" class="border-t border-zinc-100 px-4 py-2 sm:hidden">
        <RouterLink
          v-for="link in links"
          :key="link.to"
          :to="link.to"
          class="block rounded-lg px-3 py-2 text-sm font-medium"
          :class="route.path === link.to ? 'bg-brand-50 text-brand-700' : 'text-zinc-600'"
          @click="menuOpen = false"
        >
          {{ link.label }}
        </RouterLink>
        <AppButton class="mt-2 w-full" variant="ghost" @click="handleLogout">Sair</AppButton>
      </nav>
    </header>

    <main class="mx-auto max-w-5xl px-4 py-6 sm:px-6">
      <h1 class="mb-6 text-xl font-bold text-zinc-900 sm:text-2xl">{{ title }}</h1>
      <slot />
    </main>
  </div>
</template>

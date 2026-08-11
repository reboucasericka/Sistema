<script setup lang="ts">
import { RouterView, useRoute } from 'vue-router'
import ClientShell from '@/components/layout/ClientShell.vue'
import { computed } from 'vue'

const route = useRoute()

const title = computed(() => {
  const map: Record<string, string> = {
    'client-dashboard': 'Bem-vindo',
    'client-appointments': 'Meus agendamentos',
  }
  return map[route.name as string] ?? 'Área do cliente'
})
</script>

<template>
  <ClientShell :title="title">
    <RouterView />
  </ClientShell>
</template>

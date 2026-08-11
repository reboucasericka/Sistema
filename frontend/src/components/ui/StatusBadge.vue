<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  status: string
  label?: string
}>()

const config: Record<string, { label: string; class: string }> = {
  pending: { label: 'Pendente', class: 'bg-amber-50 text-amber-800 ring-amber-600/15' },
  confirmed: { label: 'Confirmado', class: 'bg-emerald-50 text-emerald-800 ring-emerald-600/15' },
  completed: { label: 'Concluído', class: 'bg-brand-50 text-brand-800 ring-brand-600/15' },
  canceled: { label: 'Cancelado', class: 'bg-red-50 text-red-700 ring-red-600/15' },
  cancelled: { label: 'Cancelado', class: 'bg-red-50 text-red-700 ring-red-600/15' },
  paid: { label: 'Pago', class: 'bg-emerald-50 text-emerald-800 ring-emerald-600/15' },
}

const resolved = computed(() => config[props.status] ?? { label: props.label ?? props.status, class: 'bg-zinc-100 text-zinc-600 ring-zinc-500/10' })
</script>

<template>
  <span
    class="inline-flex rounded-md px-2 py-0.5 text-xs font-medium capitalize ring-1 ring-inset"
    :class="resolved.class"
  >
    {{ label ?? resolved.label }}
  </span>
</template>

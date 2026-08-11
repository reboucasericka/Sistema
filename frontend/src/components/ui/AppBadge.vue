<script setup lang="ts">
defineProps<{
  tone?: 'default' | 'success' | 'warning' | 'danger' | 'brand'
}>()
</script>

<template>
  <span
    class="inline-flex rounded-md px-2 py-0.5 text-xs font-medium capitalize ring-1 ring-inset"
    :class="{
      'bg-zinc-100 text-zinc-700 ring-zinc-500/10': (tone ?? 'default') === 'default',
      'bg-emerald-50 text-emerald-800 ring-emerald-600/15': tone === 'success',
      'bg-amber-50 text-amber-800 ring-amber-600/15': tone === 'warning',
      'bg-red-50 text-red-700 ring-red-600/15': tone === 'danger',
      'bg-brand-50 text-brand-800 ring-brand-600/15': tone === 'brand',
    }"
  >
    <slot />
  </span>
</template>

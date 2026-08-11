<script setup lang="ts">
defineProps<{
  title: string
  description?: string
}>()
</script>

<template>
  <div class="rounded-xl border border-dashed border-zinc-300 bg-zinc-50 px-6 py-12 text-center">
    <p class="font-medium text-zinc-700">{{ title }}</p>
    <p v-if="description" class="mt-1 text-sm text-zinc-500">{{ description }}</p>
  </div>
</template>

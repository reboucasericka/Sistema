<script setup lang="ts">
defineProps<{
  title?: string
  padding?: 'default' | 'none'
}>()
</script>

<template>
  <div
    class="rounded-xl border border-[#E5E7EB] bg-white shadow-sm"
    :class="padding === 'none' ? '' : 'p-5'"
  >
    <h3 v-if="title" class="mb-4 text-base font-semibold text-[#1F2937]">{{ title }}</h3>
    <slot />
  </div>
</template>

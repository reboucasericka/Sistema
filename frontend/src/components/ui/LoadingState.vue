<script setup lang="ts">
defineProps<{
  message?: string
}>()
</script>

<template>
  <div class="flex items-center justify-center py-12 text-sm text-zinc-500">
    <span v-if="message">{{ message }}</span>
    <span v-else>A carregar...</span>
  </div>
</template>

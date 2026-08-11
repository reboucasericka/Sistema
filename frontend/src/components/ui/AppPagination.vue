<script setup lang="ts">
import AppButton from '@/components/ui/AppButton.vue'

defineProps<{
  currentPage: number
  lastPage: number
  total?: number
}>()

const emit = defineEmits<{
  change: [page: number]
}>()
</script>

<template>
  <div
    v-if="lastPage > 1"
    class="flex flex-col items-center justify-between gap-3 border-t border-zinc-100 pt-4 sm:flex-row"
  >
    <p v-if="total !== undefined" class="text-sm text-zinc-500">
      {{ total }} registo(s) · página {{ currentPage }} de {{ lastPage }}
    </p>
    <div class="flex gap-2">
      <AppButton
        variant="secondary"
        :disabled="currentPage <= 1"
        @click="emit('change', currentPage - 1)"
      >
        Anterior
      </AppButton>
      <AppButton
        variant="secondary"
        :disabled="currentPage >= lastPage"
        @click="emit('change', currentPage + 1)"
      >
        Seguinte
      </AppButton>
    </div>
  </div>
</template>

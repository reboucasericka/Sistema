<script setup lang="ts">
defineProps<{
  modelValue?: string
  label?: string
  rows?: number
}>()

defineEmits<{
  'update:modelValue': [value: string]
}>()
</script>

<template>
  <label class="block space-y-1">
    <span v-if="label" class="text-sm font-medium text-zinc-700">{{ label }}</span>
    <textarea
      :value="modelValue"
      :rows="rows ?? 3"
      class="w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 text-sm outline-none ring-brand-500 focus:ring-2"
      @input="$emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)"
    />
  </label>
</template>

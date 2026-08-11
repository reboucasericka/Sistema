<script setup lang="ts">
withDefaults(
  defineProps<{
    modelValue?: string | number
    label?: string
    required?: boolean
    emptyLabel?: string
    options: { value: string | number; label: string }[]
  }>(),
  { emptyLabel: 'Selecionar' },
)

defineEmits<{
  'update:modelValue': [value: string]
}>()
</script>

<template>
  <label class="block space-y-1">
    <span v-if="label" class="text-sm font-medium text-zinc-700">{{ label }}</span>
    <select
      :value="modelValue"
      :required="required"
      class="w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 text-sm outline-none ring-brand-500 focus:ring-2"
      @change="$emit('update:modelValue', ($event.target as HTMLSelectElement).value)"
    >
      <option value="">{{ emptyLabel }}</option>
      <option
        v-for="opt in options.filter((item) => String(item.value) !== '')"
        :key="String(opt.value)"
        :value="opt.value"
      >
        {{ opt.label }}
      </option>
    </select>
  </label>
</template>

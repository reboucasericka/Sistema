<script setup lang="ts">
import { RouterLink } from 'vue-router'

defineProps<{
  label: string
  value: string | number
  hint?: string
  variant?: 'default' | 'info' | 'success' | 'warning' | 'danger' | 'brand'
  to?: string
  compact?: boolean
}>()

const accentBar: Record<string, string> = {
  default: 'bg-zinc-300',
  info: 'bg-[#9CA3AF]',
  success: 'bg-emerald-500',
  warning: 'bg-amber-500',
  danger: 'bg-red-500',
  brand: 'bg-brand-600',
}
</script>

<template>
  <component
    :is="to ? RouterLink : 'div'"
    :to="to"
    class="group relative flex h-full min-w-0 flex-col rounded-xl border border-[#E5E7EB] bg-white pl-4 shadow-sm transition"
    :class="[
      to ? 'cursor-pointer hover:border-[#E6C6B6] hover:shadow-md' : '',
      compact ? 'min-h-[104px] py-4 pr-4' : 'min-h-[118px] p-4 sm:p-5 sm:pl-5',
    ]"
  >
    <div
      class="absolute inset-y-3 left-0 w-1 rounded-full"
      :class="accentBar[variant ?? 'default']"
      aria-hidden="true"
    />

    <p class="break-words text-xs font-medium leading-snug text-[#6B7280]">
      {{ label }}
    </p>

    <p
      class="mt-1.5 font-semibold tabular-nums text-[#1F2937]"
      :class="compact ? 'text-xl' : 'text-2xl'"
    >
      {{ value }}
    </p>

    <p v-if="hint" class="mt-0.5 text-xs text-[#9CA3AF]">{{ hint }}</p>

    <span
      v-if="to && !compact"
      class="mt-auto pt-2 text-xs font-medium text-[#9CA3AF] transition group-hover:text-brand-600"
    >
      Ver detalhes →
    </span>
  </component>
</template>

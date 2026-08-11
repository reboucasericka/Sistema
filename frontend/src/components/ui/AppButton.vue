<script setup lang="ts">
defineProps<{
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost'
  type?: 'button' | 'submit'
  size?: 'sm' | 'md'
  disabled?: boolean
  loading?: boolean
}>()
</script>

<template>
  <button
    :type="type ?? 'button'"
    :disabled="disabled || loading"
    class="inline-flex items-center justify-center gap-2 rounded-lg font-medium transition focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500/40 focus-visible:ring-offset-1 disabled:cursor-not-allowed disabled:opacity-50"
    :class="[
      (size ?? 'md') === 'sm' ? 'px-2.5 py-1.5 text-xs' : 'px-4 py-2 text-sm',
      {
        'bg-brand-600 text-white shadow-sm hover:bg-brand-700': (variant ?? 'primary') === 'primary',
        'border border-[#E5E7EB] bg-white text-[#374151] hover:bg-[#F9FAFB]': variant === 'secondary',
        'bg-red-600 text-white hover:bg-red-700': variant === 'danger',
        'text-[#6B7280] hover:bg-[#F3F4F6] hover:text-[#374151]': variant === 'ghost',
      },
    ]"
  >
    <span
      v-if="loading"
      class="h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent"
      aria-hidden="true"
    />
    <slot />
  </button>
</template>

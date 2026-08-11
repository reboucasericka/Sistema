<script setup lang="ts">
withDefaults(
  defineProps<{
    open: boolean
    title: string
    size?: 'md' | 'lg'
  }>(),
  { size: 'md' },
)

defineEmits<{
  close: []
}>()
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="fixed inset-0 z-[80] flex items-end justify-center bg-black/40 p-0 sm:items-center sm:p-4"
      @click.self="$emit('close')"
    >
      <div
        class="flex w-full flex-col rounded-t-2xl bg-white shadow-xl sm:rounded-2xl"
        :class="
          size === 'lg'
            ? 'max-h-[95vh] sm:max-h-[90vh] sm:w-[min(820px,calc(100vw-32px))]'
            : 'max-h-[95vh] sm:max-h-[90vh] sm:max-w-lg'
        "
        role="dialog"
        aria-modal="true"
      >
        <div class="flex shrink-0 items-center justify-between border-b border-zinc-100 px-5 py-4 sm:px-6">
          <h3 class="text-lg font-semibold text-zinc-900">{{ title }}</h3>
          <button
            type="button"
            class="rounded-lg px-2 py-1 text-zinc-500 hover:bg-zinc-100"
            aria-label="Fechar"
            @click="$emit('close')"
          >
            ✕
          </button>
        </div>

        <div class="min-h-0 flex-1 overflow-y-auto px-5 py-4 sm:px-6">
          <slot />
        </div>

        <div
          v-if="$slots.footer"
          class="flex shrink-0 flex-col-reverse gap-2 border-t border-zinc-100 px-5 py-4 sm:flex-row sm:justify-end sm:px-6"
        >
          <slot name="footer" />
        </div>
      </div>
    </div>
  </Teleport>
</template>

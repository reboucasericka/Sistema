<script setup lang="ts">
import AppButton from '@/components/ui/AppButton.vue'
import { useUiStore } from '@/stores/ui'

const ui = useUiStore()
</script>

<template>
  <Teleport to="body">
    <div
      v-if="ui.confirmOpen"
      class="fixed inset-0 z-[90] flex items-center justify-center bg-black/40 p-4"
      @click.self="ui.resolveConfirm(false)"
    >
      <div class="w-full max-w-md rounded-xl bg-white p-6 shadow-xl" role="dialog" aria-modal="true">
        <h3 class="text-lg font-semibold text-zinc-900">{{ ui.confirmOptions.title }}</h3>
        <p class="mt-2 text-sm text-zinc-600">{{ ui.confirmOptions.message }}</p>
        <div class="mt-6 flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
          <AppButton variant="secondary" @click="ui.resolveConfirm(false)">
            {{ ui.confirmOptions.cancelLabel }}
          </AppButton>
          <AppButton
            :variant="ui.confirmOptions.variant === 'primary' ? 'primary' : 'danger'"
            @click="ui.resolveConfirm(true)"
          >
            {{ ui.confirmOptions.confirmLabel }}
          </AppButton>
        </div>
      </div>
    </div>
  </Teleport>
</template>

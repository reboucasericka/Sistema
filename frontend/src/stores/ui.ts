import { defineStore } from 'pinia'
import { ref } from 'vue'

export type ToastType = 'success' | 'error' | 'info'

export interface ToastItem {
  id: number
  message: string
  type: ToastType
}

export interface ConfirmOptions {
  title: string
  message: string
  confirmLabel?: string
  cancelLabel?: string
  variant?: 'danger' | 'primary'
}

let toastId = 0

export const useUiStore = defineStore('ui', () => {
  const toasts = ref<ToastItem[]>([])
  const confirmOpen = ref(false)
  const confirmOptions = ref<ConfirmOptions>({
    title: 'Confirmar',
    message: 'Tem a certeza?',
  })
  let confirmResolver: ((value: boolean) => void) | null = null

  function pushToast(message: string, type: ToastType) {
    const id = ++toastId
    toasts.value.push({ id, message, type })
    setTimeout(() => removeToast(id), 4000)
  }

  function removeToast(id: number) {
    toasts.value = toasts.value.filter((t) => t.id !== id)
  }

  function success(message: string) {
    pushToast(message, 'success')
  }

  function error(message: string) {
    pushToast(message, 'error')
  }

  function info(message: string) {
    pushToast(message, 'info')
  }

  function confirm(options: ConfirmOptions): Promise<boolean> {
    confirmOptions.value = {
      confirmLabel: 'Confirmar',
      cancelLabel: 'Cancelar',
      variant: 'danger',
      ...options,
    }
    confirmOpen.value = true
    return new Promise((resolve) => {
      confirmResolver = resolve
    })
  }

  function resolveConfirm(value: boolean) {
    confirmOpen.value = false
    confirmResolver?.(value)
    confirmResolver = null
  }

  return {
    toasts,
    confirmOpen,
    confirmOptions,
    success,
    error,
    info,
    removeToast,
    confirm,
    resolveConfirm,
  }
})

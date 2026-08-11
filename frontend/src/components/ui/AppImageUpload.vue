<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import { IMAGES } from '@/lib/images'

const props = withDefaults(
  defineProps<{
    label?: string
    currentUrl?: string | null
    modelValue?: File | null
    remove?: boolean
    error?: string
    placeholderSrc?: string
  }>(),
  {
    label: 'Imagem',
    modelValue: null,
    remove: false,
    placeholderSrc: IMAGES.placeholder,
  },
)

const emit = defineEmits<{
  'update:modelValue': [value: File | null]
  'update:remove': [value: boolean]
}>()

const inputRef = ref<HTMLInputElement | null>(null)
const objectUrl = ref<string | null>(null)

watch(
  () => props.modelValue,
  (file) => {
    if (objectUrl.value) {
      URL.revokeObjectURL(objectUrl.value)
      objectUrl.value = null
    }
    if (file) objectUrl.value = URL.createObjectURL(file)
  },
)

onBeforeUnmount(() => {
  if (objectUrl.value) URL.revokeObjectURL(objectUrl.value)
})

const previewSrc = computed(() => {
  if (objectUrl.value) return objectUrl.value
  if (props.remove) return props.placeholderSrc
  return props.currentUrl || props.placeholderSrc
})

const fileLabel = computed(() => {
  if (props.modelValue) return props.modelValue.name
  if (props.remove) return 'Imagem será removida ao guardar'
  if (props.currentUrl) return 'Imagem atual'
  return 'Nenhuma imagem selecionada'
})

function openPicker() {
  inputRef.value?.click()
}

function onFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0] ?? null
  emit('update:remove', false)
  emit('update:modelValue', file)
}

function clearSelection() {
  emit('update:modelValue', null)
  if (inputRef.value) inputRef.value.value = ''
}

function removeImage() {
  clearSelection()
  emit('update:remove', true)
}

function keepCurrent() {
  clearSelection()
  emit('update:remove', false)
}
</script>

<template>
  <div class="space-y-2">
    <span v-if="label" class="block text-sm font-medium text-zinc-700">{{ label }}</span>

    <div class="flex flex-col gap-3 sm:flex-row sm:items-start">
      <button
        type="button"
        class="h-28 w-28 shrink-0 overflow-hidden rounded-xl border border-zinc-200 bg-zinc-50"
        @click="openPicker"
      >
        <img :src="previewSrc" alt="" class="h-full w-full object-cover" />
      </button>

      <div class="min-w-0 flex-1 space-y-2">
        <input
          ref="inputRef"
          type="file"
          accept="image/jpeg,image/png,image/webp,.jpg,.jpeg,.png,.webp"
          class="hidden"
          @change="onFileChange"
        />

        <p class="truncate text-sm text-zinc-700">{{ fileLabel }}</p>
        <p class="text-xs text-zinc-500">JPG, PNG ou WebP até 2 MB. Opcional.</p>

        <div class="flex flex-wrap gap-2">
          <button
            type="button"
            class="rounded-lg border border-zinc-300 bg-white px-3 py-1.5 text-xs font-medium text-zinc-700 hover:bg-zinc-50"
            @click="openPicker"
          >
            Escolher ficheiro
          </button>
          <button
            v-if="modelValue"
            type="button"
            class="rounded-lg border border-zinc-300 bg-white px-3 py-1.5 text-xs font-medium text-zinc-700 hover:bg-zinc-50"
            @click="clearSelection"
          >
            Cancelar seleção
          </button>
          <button
            v-if="currentUrl && !remove && !modelValue"
            type="button"
            class="rounded-lg border border-red-200 bg-white px-3 py-1.5 text-xs font-medium text-red-600 hover:bg-red-50"
            @click="removeImage"
          >
            Remover imagem
          </button>
          <button
            v-if="remove"
            type="button"
            class="rounded-lg border border-zinc-300 bg-white px-3 py-1.5 text-xs font-medium text-zinc-700 hover:bg-zinc-50"
            @click="keepCurrent"
          >
            Manter imagem atual
          </button>
        </div>

        <p v-if="error" class="text-sm text-red-600">{{ error }}</p>
      </div>
    </div>
  </div>
</template>

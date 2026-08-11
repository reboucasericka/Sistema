<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'

const props = defineProps<{
  modelValue: string
  minDate?: string
  daysCount?: number
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const windowStart = ref(0)
const DAYS = computed(() => props.daysCount ?? 21)

const weekdayShort = ['DOM', 'SEG', 'TER', 'QUA', 'QUI', 'SEX', 'SÁB']

const dates = computed(() => {
  const min = props.minDate ? new Date(`${props.minDate}T00:00:00`) : new Date()
  min.setHours(0, 0, 0, 0)

  return Array.from({ length: DAYS.value }, (_, index) => {
    const current = new Date(min)
    current.setDate(min.getDate() + index)
    return {
      iso: toIsoDate(current),
      weekday: weekdayShort[current.getDay()] ?? '',
      label: formatDayLabel(current),
      disabled: false,
    }
  })
})

const visibleDates = computed(() => {
  const start = windowStart.value
  const end = start + 7
  return dates.value.slice(start, end)
})

const canPrev = computed(() => windowStart.value > 0)
const canNext = computed(() => windowStart.value + 7 < dates.value.length)

watch(
  () => props.modelValue,
  (value) => {
    if (!value) return
    const index = dates.value.findIndex((d) => d.iso === value)
    if (index >= 0 && (index < windowStart.value || index >= windowStart.value + 7)) {
      windowStart.value = Math.max(0, Math.min(index, dates.value.length - 7))
    }
  },
  { immediate: true },
)

function toIsoDate(date: Date) {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

function formatDayLabel(date: Date) {
  const day = String(date.getDate()).padStart(2, '0')
  const month = String(date.getMonth() + 1).padStart(2, '0')
  return `${day}/${month}`
}

function selectDate(iso: string) {
  emit('update:modelValue', iso)
}

function prev() {
  if (!canPrev.value) return
  windowStart.value = Math.max(0, windowStart.value - 7)
}

function next() {
  if (!canNext.value) return
  windowStart.value = Math.min(dates.value.length - 7, windowStart.value + 7)
}

function initDefault() {
  if (!props.modelValue && dates.value[0]) {
    emit('update:modelValue', dates.value[0].iso)
  }
}

onMounted(initDefault)
</script>

<template>
  <div class="date-strip">
    <div class="date-strip__header">
      <button type="button" class="date-strip__link" :disabled="!canPrev" @click="prev">
        ← Anterior
      </button>
      <button type="button" class="date-strip__link" :disabled="!canNext" @click="next">
        Próximo →
      </button>
    </div>

    <div class="date-strip__track">
      <button
        v-for="day in visibleDates"
        :key="day.iso"
        type="button"
        class="date-strip__day"
        :class="{ 'is-selected': modelValue === day.iso }"
        @click="selectDate(day.iso)"
      >
        <span class="date-strip__weekday">{{ day.weekday }}</span>
        <span class="date-strip__date">{{ day.label }}</span>
      </button>
    </div>

    <div class="date-strip__bar">
      <div
        class="date-strip__bar-fill"
        :style="{ width: `${((windowStart + 7) / dates.length) * 100}%` }"
      />
    </div>
  </div>
</template>

<style scoped>
.date-strip__header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.75rem;
}

.date-strip__link {
  border: none;
  background: none;
  color: #ff9459;
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  padding: 0;
}

.date-strip__link:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.date-strip__track {
  display: flex;
  gap: 0;
  overflow-x: auto;
  scrollbar-width: none;
  border-bottom: 1px solid #e5e5e5;
}

.date-strip__track::-webkit-scrollbar {
  display: none;
}

.date-strip__day {
  flex: 1;
  min-width: 72px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.15rem;
  padding: 0.75rem 0.5rem;
  border: none;
  background: none;
  cursor: pointer;
  color: #333;
  transition: background 0.2s ease;
}

.date-strip__day.is-selected {
  background: #fff3ec;
  color: #ff9459;
}

.date-strip__weekday {
  font-size: 0.75rem;
  font-weight: 600;
  letter-spacing: 0.02em;
}

.date-strip__date {
  font-size: 0.85rem;
}

.date-strip__bar {
  height: 3px;
  background: #eee;
  margin-top: 0.5rem;
  border-radius: 2px;
  overflow: hidden;
}

.date-strip__bar-fill {
  height: 100%;
  background: #bbb;
  border-radius: 2px;
  transition: width 0.3s ease;
}
</style>

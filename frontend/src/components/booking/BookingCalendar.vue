<script setup lang="ts">
import { computed, ref, watch } from 'vue'

const props = defineProps<{
  modelValue: string
  minDate?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const viewDate = ref(new Date())

const weekdayLabels = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb']

const monthLabel = computed(() =>
  viewDate.value.toLocaleDateString('pt-PT', { month: 'long', year: 'numeric' }),
)

const calendarDays = computed(() => {
  const year = viewDate.value.getFullYear()
  const month = viewDate.value.getMonth()
  const firstDay = new Date(year, month, 1)
  const startOffset = firstDay.getDay()
  const daysInMonth = new Date(year, month + 1, 0).getDate()

  const cells: Array<{ date: string; day: number; disabled: boolean; isToday: boolean } | null> =
    []

  for (let i = 0; i < startOffset; i++) cells.push(null)

  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const min = props.minDate ? new Date(`${props.minDate}T00:00:00`) : today

  for (let day = 1; day <= daysInMonth; day++) {
    const current = new Date(year, month, day)
    const iso = toIsoDate(current)
    cells.push({
      date: iso,
      day,
      disabled: current < min,
      isToday: current.getTime() === today.getTime(),
    })
  }

  return cells
})

watch(
  () => props.modelValue,
  (value) => {
    if (!value) return
    viewDate.value = new Date(`${value}T00:00:00`)
  },
  { immediate: true },
)

function toIsoDate(date: Date) {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

function prevMonth() {
  viewDate.value = new Date(viewDate.value.getFullYear(), viewDate.value.getMonth() - 1, 1)
}

function nextMonth() {
  viewDate.value = new Date(viewDate.value.getFullYear(), viewDate.value.getMonth() + 1, 1)
}

function selectDate(date: string) {
  emit('update:modelValue', date)
}
</script>

<template>
  <div class="booking-calendar">
    <div class="booking-calendar__header">
      <button type="button" class="booking-calendar__nav" aria-label="Mês anterior" @click="prevMonth">
        ‹
      </button>
      <h3 class="booking-calendar__month">{{ monthLabel }}</h3>
      <button type="button" class="booking-calendar__nav" aria-label="Próximo mês" @click="nextMonth">
        ›
      </button>
    </div>

    <div class="booking-calendar__weekdays">
      <span v-for="label in weekdayLabels" :key="label">{{ label }}</span>
    </div>

    <div class="booking-calendar__grid">
      <template v-for="(cell, index) in calendarDays" :key="index">
        <span v-if="!cell" class="booking-calendar__empty" />
        <button
          v-else
          type="button"
          class="booking-calendar__day"
          :class="{
            'is-selected': modelValue === cell.date,
            'is-today': cell.isToday,
            'is-disabled': cell.disabled,
          }"
          :disabled="cell.disabled"
          @click="selectDate(cell.date)"
        >
          {{ cell.day }}
        </button>
      </template>
    </div>
  </div>
</template>

<style scoped>
.booking-calendar {
  border: 1px solid #ececec;
  border-radius: 14px;
  padding: 1rem;
  background: #fff;
}

.booking-calendar__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.75rem;
}

.booking-calendar__month {
  font-size: 1rem;
  font-weight: 600;
  text-transform: capitalize;
  color: var(--neutral-dark);
}

.booking-calendar__nav {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: 1px solid #e5e5e5;
  background: #fff;
  color: var(--accent-terracotta);
  font-size: 1.2rem;
  cursor: pointer;
}

.booking-calendar__weekdays,
.booking-calendar__grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 0.25rem;
}

.booking-calendar__weekdays {
  margin-bottom: 0.35rem;
  font-size: 0.75rem;
  color: #999;
  text-align: center;
}

.booking-calendar__day,
.booking-calendar__empty {
  aspect-ratio: 1;
}

.booking-calendar__day {
  border: none;
  border-radius: 50%;
  background: transparent;
  font-size: 0.88rem;
  cursor: pointer;
  color: var(--neutral-dark);
}

.booking-calendar__day:hover:not(:disabled) {
  background: #f5ebe6;
}

.booking-calendar__day.is-today {
  border: 1px solid var(--accent-peach);
}

.booking-calendar__day.is-selected {
  background: var(--accent-terracotta);
  color: #fff;
}

.booking-calendar__day.is-disabled {
  opacity: 0.35;
  cursor: not-allowed;
}
</style>

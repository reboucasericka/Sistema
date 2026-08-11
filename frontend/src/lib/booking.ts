export function formatPrice(value: string | number, currency = 'EUR'): string {
  return new Intl.NumberFormat('pt-PT', { style: 'currency', currency }).format(Number(value))
}

export function formatDuration(minutes: number): string {
  if (minutes < 60) return `${minutes}min`
  const hours = Math.floor(minutes / 60)
  const rest = minutes % 60
  return rest ? `${hours}h ${rest}min` : `${hours}h`
}

export type TimePeriod = 'morning' | 'afternoon' | 'evening'

export const TIME_PERIOD_LABELS: Record<TimePeriod, string> = {
  morning: 'Manhã',
  afternoon: 'Tarde',
  evening: 'Noite',
}

export function groupSlotsByPeriod(slots: string[]): Record<TimePeriod, string[]> {
  const grouped: Record<TimePeriod, string[]> = {
    morning: [],
    afternoon: [],
    evening: [],
  }

  for (const slot of slots) {
    const hour = Number.parseInt(slot.split(':')[0] ?? '0', 10)
    if (hour < 12) grouped.morning.push(slot)
    else if (hour < 18) grouped.afternoon.push(slot)
    else grouped.evening.push(slot)
  }

  return grouped
}

export function uncategorizedLabel(category: string | null | undefined): string {
  return category?.trim() || 'Outros serviços'
}

/** Ordem das categorias no agendamento online (como no site de referência). */
export const BOOKING_CATEGORY_ORDER = [
  'Extensão de Cilios',
  'Tecnicas Semipermanentes',
  'Depilação',
  'Sobrancelha',
  'Depilação Masculina',
  'Beauty Treatment',
] as const

export function sortBookingCategories<T extends { name: string }>(groups: T[]): T[] {
  return [...groups].sort((a, b) => {
    const indexA = BOOKING_CATEGORY_ORDER.indexOf(a.name as (typeof BOOKING_CATEGORY_ORDER)[number])
    const indexB = BOOKING_CATEGORY_ORDER.indexOf(b.name as (typeof BOOKING_CATEGORY_ORDER)[number])
    const orderA = indexA === -1 ? 999 : indexA
    const orderB = indexB === -1 ? 999 : indexB
    if (orderA !== orderB) return orderA - orderB
    return a.name.localeCompare(b.name, 'pt')
  })
}

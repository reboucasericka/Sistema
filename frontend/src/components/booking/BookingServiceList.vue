<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { formatDuration, formatPrice, sortBookingCategories, uncategorizedLabel } from '@/lib/booking'
import type { Service } from '@/types/api'

const props = defineProps<{
  services: Service[]
  loading?: boolean
}>()

const searchByCategory = ref<Record<string, string>>({})
const expandedIds = ref<Set<number>>(new Set())
const openCategories = ref<Set<string>>(new Set())

const DESCRIPTION_LIMIT = 120

const categories = computed(() => {
  const map = new Map<string, Service[]>()

  for (const service of props.services) {
    const label = uncategorizedLabel(service.service_category?.name ?? service.category)
    const list = map.get(label) ?? []
    list.push(service)
    map.set(label, list)
  }

  return sortBookingCategories(
    [...map.entries()].map(([name, items]) => ({ name, items })),
  )
})

const filteredCategories = computed(() => {
  return categories.value
    .map((group) => {
      const query = (searchByCategory.value[group.name] ?? '').trim().toLowerCase()
      if (!query) return group

      return {
        ...group,
        items: group.items.filter(
          (service) =>
            service.name.toLowerCase().includes(query) ||
            (service.description ?? '').toLowerCase().includes(query),
        ),
      }
    })
    .filter((group) => group.items.length > 0)
})

function toggleExpanded(id: number) {
  const next = new Set(expandedIds.value)
  if (next.has(id)) next.delete(id)
  else next.add(id)
  expandedIds.value = next
}

function isExpanded(id: number) {
  return expandedIds.value.has(id)
}

function isCategoryOpen(name: string) {
  return openCategories.value.has(name)
}

function toggleCategory(name: string) {
  const next = new Set(openCategories.value)
  if (next.has(name)) next.delete(name)
  else next.add(name)
  openCategories.value = next
}

function shouldTruncate(description: string | null) {
  return (description?.length ?? 0) > DESCRIPTION_LIMIT
}

function previewText(description: string | null) {
  if (!description) return ''
  if (description.length <= DESCRIPTION_LIMIT) return description
  return description.slice(0, DESCRIPTION_LIMIT).trim()
}

function bookingLink(serviceId: number) {
  return { name: 'booking-flow', query: { serviceId: String(serviceId) } }
}
</script>

<template>
  <section class="booking-services">
    <p class="booking-services__heading">Todos os serviços</p>

    <div v-if="loading" class="booking-services__loading">A carregar serviços...</div>

    <div v-else-if="!filteredCategories.length" class="booking-services__loading">
      Nenhum serviço cadastrado no painel administrativo.
    </div>

    <div v-else class="booking-services__groups">
      <section
        v-for="group in filteredCategories"
        :key="group.name"
        class="booking-services__group"
        :class="{ 'is-open': isCategoryOpen(group.name) }"
      >
        <button
          type="button"
          class="booking-services__accordion"
          :aria-expanded="isCategoryOpen(group.name)"
          @click="toggleCategory(group.name)"
        >
          <span>{{ group.name }}</span>
          <span class="booking-services__chevron" aria-hidden="true">
            {{ isCategoryOpen(group.name) ? '▲' : '▼' }}
          </span>
        </button>

        <div v-show="isCategoryOpen(group.name)" class="booking-services__panel">
          <div class="booking-services__search">
            <input
              v-model="searchByCategory[group.name]"
              type="search"
              placeholder="Pesquisar serviço..."
              aria-label="Pesquisar serviço"
            />
            <span class="booking-services__search-icon" aria-hidden="true">⌕</span>
          </div>

          <article v-for="service in group.items" :key="service.id" class="booking-service-card">
            <div class="booking-service-card__main">
              <div class="booking-service-card__header">
                <h4 class="booking-service-card__name">{{ service.name }}</h4>
                <span class="booking-service-card__duration">{{
                  formatDuration(service.duration_minutes)
                }}</span>
              </div>

              <p v-if="service.description" class="booking-service-card__description">
                <template v-if="isExpanded(service.id) || !shouldTruncate(service.description)">
                  {{ service.description }}
                </template>
                <template v-else>{{ previewText(service.description) }}</template>
                <button
                  v-if="shouldTruncate(service.description)"
                  type="button"
                  class="booking-service-card__toggle"
                  @click="toggleExpanded(service.id)"
                >
                  {{ isExpanded(service.id) ? '− Ler menos' : '+ Ler mais' }}
                </button>
              </p>
            </div>

            <div class="booking-service-card__aside">
              <span class="booking-service-card__price">{{ formatPrice(service.price) }}</span>
              <RouterLink :to="bookingLink(service.id)" class="booking-service-card__btn">
                Agendar
              </RouterLink>
            </div>
          </article>
        </div>
      </section>
    </div>
  </section>
</template>

<style scoped>
.booking-services__heading {
  font-size: 0.95rem;
  color: #666;
  margin-bottom: 1rem;
}

.booking-services__loading {
  color: #888;
  padding: 2rem 0;
}

.booking-services__groups {
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
}

.booking-services__group {
  border-radius: 6px;
  overflow: hidden;
}

.booking-services__accordion {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding: 0.9rem 1rem;
  border: 1px solid #e0e0e0;
  border-radius: 6px;
  background: #fff;
  font-size: 0.95rem;
  font-weight: 500;
  color: #333;
  cursor: pointer;
  text-align: left;
}

.booking-services__group.is-open .booking-services__accordion {
  border-radius: 6px 6px 0 0;
  border-bottom: none;
}

.booking-services__chevron {
  font-size: 0.7rem;
  color: #888;
}

.booking-services__panel {
  border: 1px solid #e0e0e0;
  border-top: none;
  border-radius: 0 0 6px 6px;
  padding: 1rem;
  background: #fff;
}

.booking-services__search {
  position: relative;
  margin-bottom: 1rem;
}

.booking-services__search input {
  width: 100%;
  padding: 0.65rem 2.2rem 0.65rem 0.9rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 0.9rem;
}

.booking-services__search-icon {
  position: absolute;
  right: 0.85rem;
  top: 50%;
  transform: translateY(-50%);
  color: #999;
  pointer-events: none;
}

.booking-service-card {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  gap: 1rem;
  padding: 1.25rem 0;
  border-bottom: 1px solid #efefef;
}

.booking-service-card:last-child {
  border-bottom: none;
}

.booking-service-card__header {
  display: flex;
  flex-wrap: wrap;
  align-items: baseline;
  gap: 0.75rem;
  margin-bottom: 0.35rem;
}

.booking-service-card__name {
  font-size: 1rem;
  font-weight: 600;
  color: #333;
}

.booking-service-card__duration {
  font-size: 0.85rem;
  color: #888;
}

.booking-service-card__description {
  color: #666;
  font-size: 0.9rem;
  line-height: 1.55;
  max-width: 680px;
}

.booking-service-card__toggle {
  border: none;
  background: none;
  color: #ff9459;
  font-size: 0.85rem;
  font-weight: 500;
  cursor: pointer;
  padding: 0;
  margin-left: 0.25rem;
}

.booking-service-card__aside {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.65rem;
  min-width: 120px;
}

.booking-service-card__price {
  font-size: 1rem;
  font-weight: 700;
  color: #333;
}

.booking-service-card__btn {
  display: inline-block;
  padding: 0.5rem 1.25rem;
  border-radius: 4px;
  background: #ff9459;
  color: #fff;
  text-decoration: none;
  font-size: 0.9rem;
  font-weight: 500;
  transition: background 0.2s ease;
}

.booking-service-card__btn:hover {
  background: #e8834a;
  color: #fff;
}
</style>

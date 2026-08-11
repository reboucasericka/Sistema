<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { formatDuration, formatPrice } from '@/lib/booking'
import type { Service } from '@/types/api'

const props = defineProps<{
  services: Service[]
  loading?: boolean
}>()

const emit = defineEmits<{
  pagination: [state: { activePage: number; pageCount: number }]
}>()

const VISIBLE = 4
const offset = ref(0)
const trackRef = ref<HTMLElement | null>(null)

const visibleServices = computed(() => props.services.slice(0, 12))
const maxOffset = computed(() => Math.max(0, visibleServices.value.length - VISIBLE))
const pageCount = computed(() => Math.ceil(visibleServices.value.length / VISIBLE) || 1)
const activePage = computed(() => Math.floor(offset.value / VISIBLE))

function prev() {
  offset.value = Math.max(0, offset.value - 1)
  scrollToOffset()
}

function next() {
  offset.value = Math.min(maxOffset.value, offset.value + 1)
  scrollToOffset()
}

function scrollToOffset() {
  const el = trackRef.value
  if (!el) return
  const card = el.querySelector<HTMLElement>('.featured-card')
  const cardWidth = card?.offsetWidth ?? 0
  const gap = 16
  el.scrollTo({ left: offset.value * (cardWidth + gap), behavior: 'smooth' })
}

function bookingLink(serviceId: number) {
  return { name: 'booking-flow', query: { serviceId: String(serviceId) } }
}

function goToPage(page: number) {
  offset.value = Math.min(page * VISIBLE, maxOffset.value)
  scrollToOffset()
}

onMounted(() => {
  trackRef.value?.addEventListener('scroll', onScroll, { passive: true })
})

onBeforeUnmount(() => {
  trackRef.value?.removeEventListener('scroll', onScroll)
})

function onScroll() {
  const el = trackRef.value
  if (!el) return
  const card = el.querySelector<HTMLElement>('.featured-card')
  const cardWidth = card?.offsetWidth ?? 1
  const gap = 16
  offset.value = Math.round(el.scrollLeft / (cardWidth + gap))
}

watch([activePage, pageCount, visibleServices], () => {
  if (!visibleServices.value.length) return
  emit('pagination', { activePage: activePage.value, pageCount: pageCount.value })
}, { immediate: true })

defineExpose({ goToPage })
</script>

<template>
  <section class="featured-section">
    <h2 class="featured-section__title">Mais agendados</h2>

    <div v-if="loading" class="featured-section__empty">A carregar...</div>
    <div v-else-if="!visibleServices.length" class="featured-section__empty">
      Sem serviços disponíveis.
    </div>

    <template v-else>
      <div class="featured-section__carousel">
        <button
          type="button"
          class="featured-section__nav"
          aria-label="Anterior"
          :disabled="offset === 0"
          @click="prev"
        >
          ‹
        </button>

        <div ref="trackRef" class="featured-section__track">
          <article v-for="service in visibleServices" :key="service.id" class="featured-card">
            <h3 class="featured-card__name" :title="service.name">{{ service.name }}</h3>
            <span class="featured-card__duration">{{ formatDuration(service.duration_minutes) }}</span>
            <p class="featured-card__price">{{ formatPrice(service.price) }}</p>
            <RouterLink :to="bookingLink(service.id)" class="featured-card__btn">Agendar</RouterLink>
          </article>
        </div>

        <button
          type="button"
          class="featured-section__nav"
          aria-label="Próximo"
          :disabled="offset >= maxOffset"
          @click="next"
        >
          ›
        </button>
      </div>
    </template>
  </section>
</template>

<style scoped>
.featured-section {
  margin-bottom: 2.5rem;
}

.featured-section__title {
  font-size: 1.1rem;
  font-weight: 600;
  color: #333;
  margin-bottom: 1rem;
}

.featured-section__empty {
  color: #888;
  font-size: 0.95rem;
}

.featured-section__carousel {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.featured-section__nav {
  flex-shrink: 0;
  width: 36px;
  height: 36px;
  border: none;
  border-radius: 50%;
  background: #ff9459;
  color: #fff;
  font-size: 1.5rem;
  line-height: 1;
  cursor: pointer;
  transition: opacity 0.2s ease;
}

.featured-section__nav:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.featured-section__track {
  flex: 1;
  display: flex;
  gap: 1rem;
  overflow-x: auto;
  scroll-snap-type: x mandatory;
  scroll-behavior: smooth;
  scrollbar-width: none;
  padding: 0.25rem 0;
}

.featured-section__track::-webkit-scrollbar {
  display: none;
}

.featured-card {
  flex: 0 0 calc(25% - 0.75rem);
  min-width: 160px;
  scroll-snap-align: start;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 1.5rem 1rem;
  background: #fff;
  border-radius: 4px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  gap: 0.5rem;
}

@media (max-width: 992px) {
  .featured-card {
    flex: 0 0 calc(50% - 0.5rem);
  }
}

@media (max-width: 576px) {
  .featured-card {
    flex: 0 0 calc(85% - 0.5rem);
  }
}

.featured-card__name {
  font-size: 0.95rem;
  font-weight: 500;
  color: #333;
  line-height: 1.3;
  width: 100%;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.featured-card__duration {
  font-size: 0.85rem;
  color: #888;
}

.featured-card__price {
  font-size: 1rem;
  font-weight: 700;
  color: #333;
  margin: 0.25rem 0 0.5rem;
}

.featured-card__btn {
  display: block;
  width: 100%;
  padding: 0.55rem 0.75rem;
  border-radius: 4px;
  background: #ff9459;
  color: #fff;
  text-decoration: none;
  font-size: 0.9rem;
  font-weight: 500;
  transition: background 0.2s ease;
}

.featured-card__btn:hover {
  background: #e8834a;
  color: #fff;
}
</style>

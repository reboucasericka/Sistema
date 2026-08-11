<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { fetchPublicProfessionals } from '@/services/publicApi'
import type { PublicProfessional } from '@/types/api'
import { IMAGES, mediaUrl } from '@/lib/images'

const professionals = ref<PublicProfessional[]>([])
const loading = ref(true)
const error = ref('')

const trackRef = ref<HTMLElement | null>(null)
const offset = ref(0)
const VISIBLE = 4

const maxOffset = computed(() => Math.max(0, professionals.value.length - VISIBLE))
const pageCount = computed(() => Math.ceil(professionals.value.length / VISIBLE) || 1)
const activePage = computed(() => Math.floor(offset.value / VISIBLE))

function imageFor(pro: PublicProfessional, index: number) {
  const url = mediaUrl(pro.image_url || pro.photo)
  if (url) return url
  const pool = [IMAGES.pd1, IMAGES.pd2, IMAGES.pd3, IMAGES.avatar]
  return pool[index % pool.length]
}

function scrollToOffset() {
  const el = trackRef.value
  if (!el) return
  const card = el.querySelector<HTMLElement>('.team-card')
  const cardWidth = card?.offsetWidth ?? 0
  const gap = 16
  el.scrollTo({ left: offset.value * (cardWidth + gap), behavior: 'smooth' })
}

function prev() {
  offset.value = Math.max(0, offset.value - 1)
  scrollToOffset()
}

function next() {
  offset.value = Math.min(maxOffset.value, offset.value + 1)
  scrollToOffset()
}

function goToPage(page: number) {
  offset.value = Math.min(page * VISIBLE, maxOffset.value)
  scrollToOffset()
}

function onScroll() {
  const el = trackRef.value
  if (!el) return
  const card = el.querySelector<HTMLElement>('.team-card')
  const cardWidth = card?.offsetWidth ?? 1
  const gap = 16
  offset.value = Math.round(el.scrollLeft / (cardWidth + gap))
}

function bindTrack() {
  trackRef.value?.addEventListener('scroll', onScroll, { passive: true })
}

function unbindTrack() {
  trackRef.value?.removeEventListener('scroll', onScroll)
}

watch(loading, async (isLoading) => {
  if (isLoading) return
  await nextTick()
  bindTrack()
})

onMounted(async () => {
  try {
    const response = await fetchPublicProfessionals({ per_page: 100 })
    professionals.value = response.data ?? []
  } catch {
    error.value = 'Não foi possível carregar a equipa.'
  } finally {
    loading.value = false
  }
})

onBeforeUnmount(() => {
  unbindTrack()
})
</script>

<template>
  <div class="team-page">
    <h1 class="team-page__title">Nossa equipe</h1>

    <div v-if="loading" class="team-page__status">A carregar...</div>
    <div v-else-if="error" class="team-page__status team-page__status--error">{{ error }}</div>
    <div v-else-if="!professionals.length" class="team-page__status">
      A nossa equipa estará disponível em breve.
    </div>

    <template v-else>
      <div class="team-page__carousel">
        <button
          type="button"
          class="team-page__nav"
          aria-label="Anterior"
          :disabled="offset === 0"
          @click="prev"
        >
          ‹
        </button>

        <div ref="trackRef" class="team-page__track">
          <RouterLink
            v-for="(pro, index) in professionals"
            :key="pro.id"
            :to="`/professionals/${pro.id}`"
            class="team-card"
          >
            <img :src="imageFor(pro, index)" :alt="pro.name" class="team-card__photo" />
            <h2 class="team-card__name">{{ pro.name }}</h2>
            <p class="team-card__role" :title="pro.specialty">{{ pro.specialty }}</p>
          </RouterLink>
        </div>

        <button
          type="button"
          class="team-page__nav"
          aria-label="Próximo"
          :disabled="offset >= maxOffset"
          @click="next"
        >
          ›
        </button>
      </div>

      <div
        v-if="pageCount > 1"
        class="team-page__dots"
        role="tablist"
        aria-label="Páginas da equipa"
      >
        <button
          v-for="page in pageCount"
          :key="page"
          type="button"
          class="team-page__dot"
          :class="{ 'team-page__dot--active': activePage === page - 1 }"
          :aria-label="`Página ${page}`"
          :aria-selected="activePage === page - 1"
          @click="goToPage(page - 1)"
        />
      </div>
    </template>
  </div>
</template>

<style scoped>
.team-page {
  max-width: 960px;
  margin: 0 auto;
  padding: 7.5rem 1.25rem 4rem;
}

.team-page__title {
  font-size: 1.35rem;
  font-weight: 600;
  color: #333;
  margin: 0 0 1.25rem;
}

.team-page__status {
  color: #888;
  font-size: 0.95rem;
  padding: 2rem 0;
}

.team-page__status--error {
  color: #c0392b;
}

.team-page__carousel {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.team-page__nav {
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

.team-page__nav:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.team-page__track {
  flex: 1;
  display: flex;
  gap: 1rem;
  overflow-x: auto;
  scroll-snap-type: x mandatory;
  scroll-behavior: smooth;
  scrollbar-width: none;
  padding: 0.35rem 0 0.75rem;
}

.team-page__track::-webkit-scrollbar {
  display: none;
}

.team-card {
  flex: 0 0 calc(25% - 0.75rem);
  min-width: 140px;
  max-width: 180px;
  scroll-snap-align: start;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  text-decoration: none;
  padding: 0.75rem 0.65rem 1rem;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
  transition:
    box-shadow 0.2s ease,
    transform 0.2s ease;
}

.team-card:hover {
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.12);
  transform: translateY(-2px);
}

.team-card__photo {
  width: 100%;
  aspect-ratio: 1;
  object-fit: cover;
  border-radius: 6px;
  margin-bottom: 0.65rem;
  background: #f0f0f0;
}

.team-card__name {
  font-size: 0.95rem;
  font-weight: 600;
  color: #222;
  margin: 0;
  line-height: 1.25;
  width: 100%;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.team-card__role {
  font-size: 0.8rem;
  color: #888;
  margin: 0.2rem 0 0;
  width: 100%;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.team-page__dots {
  display: flex;
  justify-content: center;
  gap: 0.45rem;
  margin-top: 0.75rem;
}

.team-page__dot {
  width: 8px;
  height: 8px;
  padding: 0;
  border: none;
  border-radius: 50%;
  background: #d0d0d0;
  cursor: pointer;
  transition:
    background 0.2s ease,
    transform 0.2s ease;
}

.team-page__dot--active {
  background: #ff9459;
  transform: scale(1.15);
}

@media (max-width: 992px) {
  .team-card {
    flex: 0 0 calc(33.333% - 0.7rem);
  }
}

@media (max-width: 768px) {
  .team-page {
    padding-top: 6.5rem;
  }

  .team-card {
    flex: 0 0 calc(50% - 0.5rem);
    max-width: none;
  }
}

@media (max-width: 480px) {
  .team-page__nav {
    width: 32px;
    height: 32px;
    font-size: 1.25rem;
  }

  .team-card {
    flex: 0 0 calc(70% - 0.5rem);
  }
}
</style>

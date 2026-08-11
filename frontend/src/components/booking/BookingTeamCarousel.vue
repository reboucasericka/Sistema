<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { IMAGES, mediaUrl } from '@/lib/images'
import type { PublicProfessional } from '@/types/api'

const props = defineProps<{
  professionals: PublicProfessional[]
  loading?: boolean
}>()

const scrollRef = ref<HTMLElement | null>(null)
const canScrollLeft = ref(false)
const canScrollRight = ref(false)

const visibleProfessionals = computed(() => props.professionals.slice(0, 10))

function professionalPhoto(pro: PublicProfessional, index: number) {
  const url = mediaUrl(pro.image_url || pro.photo)
  if (url) return url
  const pool = [IMAGES.pd1, IMAGES.pd2, IMAGES.pd3, IMAGES.avatar]
  return pool[index % pool.length]
}

function updateScrollState() {
  const el = scrollRef.value
  if (!el) return
  canScrollLeft.value = el.scrollLeft > 0
  canScrollRight.value = el.scrollLeft + el.clientWidth < el.scrollWidth - 4
}

function scrollBy(direction: -1 | 1) {
  scrollRef.value?.scrollBy({ left: direction * 280, behavior: 'smooth' })
}

onMounted(() => {
  updateScrollState()
  scrollRef.value?.addEventListener('scroll', updateScrollState, { passive: true })
  window.addEventListener('resize', updateScrollState)
})

onBeforeUnmount(() => {
  scrollRef.value?.removeEventListener('scroll', updateScrollState)
  window.removeEventListener('resize', updateScrollState)
})
</script>

<template>
  <section class="booking-team">
    <div class="booking-team__head">
      <h2 class="booking-team__title">Nossa equipe</h2>
      <div class="booking-team__controls">
        <button
          type="button"
          class="booking-team__nav"
          :disabled="!canScrollLeft"
          aria-label="Anterior"
          @click="scrollBy(-1)"
        >
          ‹
        </button>
        <button
          type="button"
          class="booking-team__nav"
          :disabled="!canScrollRight"
          aria-label="Próximo"
          @click="scrollBy(1)"
        >
          ›
        </button>
      </div>
    </div>

    <div v-if="loading" class="booking-team__empty">A carregar equipa...</div>
    <div v-else-if="!visibleProfessionals.length" class="booking-team__empty">
      Nenhum profissional disponível.
    </div>

    <div v-else ref="scrollRef" class="booking-team__track">
      <article
        v-for="(pro, index) in visibleProfessionals"
        :key="pro.id"
        class="booking-team__card"
      >
        <img :src="professionalPhoto(pro, index)" :alt="pro.name" class="booking-team__photo" />
        <h3 class="booking-team__name">{{ pro.name }}</h3>
        <p class="booking-team__role">{{ pro.specialty }}</p>
      </article>
    </div>
  </section>
</template>

<style scoped>
.booking-team {
  margin-top: 4rem;
  padding-top: 3rem;
  border-top: 1px solid #ececec;
}

.booking-team__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.booking-team__title {
  font-family: 'Playfair Display', serif;
  font-size: 1.8rem;
  color: var(--neutral-dark);
}

.booking-team__controls {
  display: flex;
  gap: 0.5rem;
}

.booking-team__nav {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  border: 1px solid #e5e5e5;
  background: #fff;
  color: var(--accent-terracotta);
  font-size: 1.3rem;
  cursor: pointer;
}

.booking-team__nav:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.booking-team__empty {
  color: #888;
}

.booking-team__track {
  display: flex;
  gap: 1rem;
  overflow-x: auto;
  scroll-snap-type: x mandatory;
  padding-bottom: 0.5rem;
  scrollbar-width: thin;
}

.booking-team__card {
  flex: 0 0 200px;
  scroll-snap-align: start;
  text-align: center;
}

.booking-team__photo {
  width: 160px;
  height: 160px;
  border-radius: 50%;
  object-fit: cover;
  margin: 0 auto 0.85rem;
  border: 3px solid var(--primary-beige);
}

.booking-team__name {
  font-size: 1rem;
  font-weight: 600;
  color: var(--neutral-dark);
}

.booking-team__role {
  font-size: 0.88rem;
  color: #888;
  margin-top: 0.25rem;
}
</style>

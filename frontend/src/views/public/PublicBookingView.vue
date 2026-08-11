<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import BookingFeaturedCarousel from '@/components/booking/BookingFeaturedCarousel.vue'
import BookingReviewsSection, {
  type BookingReview,
} from '@/components/booking/BookingReviewsSection.vue'
import BookingSalonInfo from '@/components/booking/BookingSalonInfo.vue'
import BookingServiceList from '@/components/booking/BookingServiceList.vue'
import BookingTeamCarousel from '@/components/booking/BookingTeamCarousel.vue'
import { fetchPublicProfessionals, fetchPublicServices } from '@/services/publicApi'
import type { PublicProfessional, Service } from '@/types/api'

const route = useRoute()
const services = ref<Service[]>([])
const popularServices = ref<Service[]>([])
const professionals = ref<PublicProfessional[]>([])
const loading = ref(true)
const carouselRef = ref<InstanceType<typeof BookingFeaturedCarousel> | null>(null)
const carouselPagination = ref({ activePage: 0, pageCount: 1 })

function onCarouselPagination(state: { activePage: number; pageCount: number }) {
  carouselPagination.value = state
}

function goToCarouselPage(page: number) {
  carouselRef.value?.goToPage(page)
}

const showSuccess = computed(() => route.query.success === '1')

const reviews: BookingReview[] = [
  {
    id: 1,
    rating: 5,
    text: 'A Sarah é muito atenciosa e simpática. Sempre faço todos os serviços com ela, pois ficam muito bem feitos. Parabéns Sarah pelo seu trabalho!',
    author: 'Ana',
    date: '23/01/2026',
  },
  {
    id: 2,
    rating: 5,
    text: 'Fiz design e coloração de sobrancelhas e a Sarah foi muito atenciosa e competente.',
    author: 'Carolina',
    date: '12/12/2025',
  },
  {
    id: 3,
    rating: 5,
    text: 'Foram todas muito legais, explicaram todos os procedimentos e aconselharam qual era o mais indicado para mim. Muito delicada Patricia na sua atenção.',
    author: 'Maria',
    date: '15/11/2025',
  },
  {
    id: 4,
    rating: 5,
    text: 'Excelente profissional!!!',
    author: 'Marcelle',
    date: '11/01/2025',
  },
  {
    id: 5,
    rating: 5,
    text: 'Fui muito bem atendida pela Sarah e ficou além das minhas expectativas.',
    author: 'Ana',
    date: '13/07/2024',
  },
]

onMounted(async () => {
  try {
    const [allServices, popular, pros] = await Promise.all([
      fetchPublicServices({ per_page: 500 }),
      fetchPublicServices({ per_page: 10, sort: 'popular' }),
      fetchPublicProfessionals({ per_page: 10 }),
    ])
    services.value = allServices.data ?? []
    popularServices.value = popular.data?.length ? popular.data : services.value.slice(0, 10)
    professionals.value = pros.data ?? []
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="booking-page">
    <div class="booking-page__wrapper">
      <div v-if="showSuccess" class="booking-page__alert">
        Agendamento criado com sucesso!
        <RouterLink to="/register">Criar conta</RouterLink>
        para gerir os seus agendamentos.
      </div>

      <section class="booking-page__hero row align-items-center g-4 mb-5">
        <div class="col-md-6">
          <h2 class="booking-page__hero-title display-5 fw-bold">
            Sua beleza merece destaque
            <span class="booking-page__hero-highlight">Agende com quem entende.</span>
          </h2>
          <p class="booking-page__hero-lead lead">
            Escolha o serviço ideal, selecione o profissional dos seus sonhos e garanta seu horário
            com apenas alguns cliques.
          </p>
        </div>
        <div class="col-md-6">
          <img
            src="/images/Academy/ew1.jpg"
            class="booking-page__hero-img img-fluid rounded"
            alt="Agendamento Online"
          />
        </div>
      </section>

      <BookingFeaturedCarousel
        ref="carouselRef"
        :services="popularServices"
        :loading="loading"
        @pagination="onCarouselPagination"
      />

      <div
        v-if="!loading && carouselPagination.pageCount > 0 && popularServices.length"
        class="booking-page__pagination"
        role="tablist"
        aria-label="Paginação do carrossel Mais agendados"
      >
        <button
          v-for="page in carouselPagination.pageCount"
          :key="page"
          type="button"
          class="booking-page__pagination-dot"
          :class="{ 'is-active': carouselPagination.activePage === page - 1 }"
          :aria-label="`Página ${page}`"
          :aria-selected="carouselPagination.activePage === page - 1"
          @click="goToCarouselPage(page - 1)"
        />
      </div>

      <BookingServiceList :services="services" :loading="loading" />

      <BookingTeamCarousel :professionals="professionals" :loading="loading" />

      <BookingReviewsSection :reviews="reviews" :average-rating="5" :total-reviews="20" />

      <BookingSalonInfo
        salon-name="Ewellin Jordão Beauty"
        welcome-message="Bem-vinda(o) ao nosso espaço"
        description="A nossa missão é dar a todas as mulheres a possibilidade de terem o olhar mais marcante, adaptado às suas feições. Realizamos serviços de extensão de cílios, design e coloração de sobrancelhas, brow lamination, lifting de cílios e micropigmentação, realizados por profissionais especialistas no embelezamento do olhar."
        payment-methods="Visa, Mastercard, American Express, Elo, Maestro, PIX, Dinheiro, Vale-Presente"
        languages="Português, Espanhol, Inglês"
        facilities="Estacionamento, Atendemos adultos e crianças, Acesso para deficientes, Aceita cartão de crédito"
      />
    </div>
  </div>
</template>

<style scoped>
.booking-page {
  margin-top: 100px;
  padding: 2rem 0 4rem;
  background: #fff;
}

.booking-page__wrapper {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1.25rem;
}

.booking-page__alert {
  padding: 0.85rem 1rem;
  margin-bottom: 1.5rem;
  border-radius: 10px;
  background: #d4edda;
  color: #155724;
}

.booking-page__alert a {
  color: #0f5132;
  font-weight: 600;
}

.booking-page__hero {
  margin-bottom: 2.5rem;
}

.booking-page__hero-title {
  color: #333;
  line-height: 1.2;
}

.booking-page__hero-highlight {
  color: #ffc107;
}

.booking-page__hero-lead {
  color: #666;
  margin-top: 1rem;
  margin-bottom: 0;
}

.booking-page__hero-img {
  width: 100%;
  max-height: 360px;
  object-fit: cover;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
}

.booking-page__pagination {
  display: flex;
  justify-content: center;
  gap: 0.5rem;
  margin: 0.5rem 0 1.25rem;
}

.booking-page__pagination-dot {
  width: 9px;
  height: 9px;
  border-radius: 50%;
  border: none;
  padding: 0;
  background: #ddd;
  cursor: pointer;
  transition: background 0.2s ease;
}

.booking-page__pagination-dot.is-active {
  background: #ff9459;
}
</style>

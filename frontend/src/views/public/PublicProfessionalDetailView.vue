<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { fetchPublicProfessional } from '@/services/publicApi'
import type { PublicProfessional } from '@/types/api'
import { IMAGES, mediaUrl } from '@/lib/images'

const route = useRoute()
const professional = ref<PublicProfessional | null>(null)
const loading = ref(true)
const notFound = ref(false)

function imageFor(pro: PublicProfessional) {
  return mediaUrl(pro.image_url || pro.photo) || IMAGES.avatar
}

onMounted(async () => {
  try {
    professional.value = await fetchPublicProfessional(Number(route.params.id))
  } catch {
    notFound.value = true
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="container py-5" style="margin-top: 100px !important">
    <div v-if="loading" class="text-muted">A carregar...</div>

    <div v-else-if="notFound || !professional" class="text-center py-5">
      <h2>Profissional não encontrado</h2>
      <RouterLink to="/professionals" class="btn btn-primary mt-3">Ver equipa</RouterLink>
    </div>

    <div v-else class="row align-items-center g-4">
      <div class="col-md-4 text-center">
        <img
          :src="imageFor(professional)"
          :alt="professional.name"
          class="rounded-circle img-fluid"
          style="max-width: 220px"
        />
      </div>
      <div class="col-md-8">
        <h1 class="display-6 fw-bold">{{ professional.name }}</h1>
        <p class="lead text-muted">{{ professional.specialty }}</p>
        <p v-if="professional.years_experience != null" class="text-muted">
          {{ professional.years_experience }} anos de experiência
        </p>
        <p v-if="professional.biography" class="mt-3">{{ professional.biography }}</p>
        <ul class="list-unstyled mt-3">
          <li v-if="professional.phone"><i class="fas fa-phone me-2" />{{ professional.phone }}</li>
          <li v-if="professional.email">
            <i class="fas fa-envelope me-2" />{{ professional.email }}
          </li>
          <li v-if="professional.instagram">
            <i class="fab fa-instagram me-2" />{{ professional.instagram }}
          </li>
          <li v-if="professional.facebook">
            <i class="fab fa-facebook me-2" />{{ professional.facebook }}
          </li>
        </ul>
        <RouterLink
          :to="{ name: 'booking-flow', query: { professionalId: professional.id } }"
          class="btn btn-primary btn-lg"
        >
          Agendar consulta
        </RouterLink>
      </div>
    </div>

    <p class="mt-4">
      <RouterLink to="/professionals">← Voltar à equipa</RouterLink>
    </p>
  </div>
</template>

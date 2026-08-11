<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { serviceImage } from '@/lib/images'
import { fetchPublicServices } from '@/services/publicApi'
import type { Service } from '@/types/api'

const route = useRoute()
const services = ref<Service[]>([])
const loading = ref(true)

const service = computed(() => services.value.find((s) => s.id === Number(route.params.id)))

function formatPrice(value: string) {
  return new Intl.NumberFormat('pt-PT', { style: 'currency', currency: 'EUR' }).format(Number(value))
}

function heroSrc(item: Service) {
  return serviceImage(item.image_url || item.image, 0)
}

onMounted(async () => {
  try {
    const response = await fetchPublicServices({ per_page: 100 })
    services.value = response.data ?? []
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div v-if="loading" class="service-details" style="margin-top: 80px; padding: 4rem; text-align: center">
    A carregar...
  </div>

  <div v-else-if="!service" class="service-details" style="margin-top: 80px; padding: 4rem; text-align: center">
    <p>Serviço não encontrado.</p>
    <RouterLink to="/services" class="btn btn-primary">Voltar aos Serviços</RouterLink>
  </div>

  <div v-else class="service-details">
    <section class="hero">
      <div class="container">
        <div class="row align-items-center">
          <div class="col-lg-6">
            <div class="hero-content">
              <h1 class="hero-title">{{ service.name }}</h1>
              <p class="hero-subtitle">Serviço</p>
              <div class="hero-description">{{ service.description || 'Sem descrição disponível.' }}</div>
              <div class="hero-info">
                <div class="info-item">
                  <i class="fas fa-euro-sign" />
                  <span class="price">{{ formatPrice(service.price) }}</span>
                </div>
                <div class="info-item">
                  <i class="fas fa-clock" />
                  <span>{{ service.duration_minutes }} minutos</span>
                </div>
              </div>
              <div class="hero-actions">
                <RouterLink
                  :to="{ name: 'booking-flow', query: { serviceId: String(service.id) } }"
                  class="btn btn-primary btn-lg"
                >
                  <i class="fas fa-calendar-plus" /> Agendar Serviço
                </RouterLink>
                <RouterLink to="/services" class="btn btn-outline-secondary btn-lg">
                  <i class="fas fa-arrow-left" /> Voltar aos Serviços
                </RouterLink>
              </div>
            </div>
          </div>
          <div class="col-lg-6">
            <div class="hero-image">
              <img :src="heroSrc(service)" :alt="service.name" class="img-fluid rounded" />
            </div>
          </div>
        </div>
      </div>
    </section>

    <section class="service-info">
      <div class="container">
        <div class="row">
          <div class="col-lg-8">
            <div class="info-card">
              <h3>Detalhes do Serviço</h3>
              <div class="info-grid">
                <div class="info-item">
                  <i class="fas fa-euro-sign" />
                  <div>
                    <strong>Preço:</strong>
                    <span class="price">{{ formatPrice(service.price) }}</span>
                  </div>
                </div>
                <div class="info-item">
                  <i class="fas fa-clock" />
                  <div>
                    <strong>Duração:</strong>
                    <span>{{ service.duration_minutes }} minutos</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-lg-4">
            <div class="booking-card">
              <h4>Agendar Este Serviço</h4>
              <p>Reserve seu horário para este serviço</p>
              <RouterLink
                :to="{ name: 'booking-flow', query: { serviceId: String(service.id) } }"
                class="btn btn-primary btn-block"
              >
                <i class="fas fa-calendar-plus" /> Agendar Agora
              </RouterLink>
              <div class="contact-info">
                <p><i class="fas fa-phone" /> (+351) 910 375 956</p>
                <p><i class="fas fa-envelope" /> ewellinjordao@gmail.com</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.service-details {
  font-family: 'Arial', sans-serif;
  margin-top: 80px;
}

.hero {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 100px 0;
  min-height: 70vh;
  display: flex;
  align-items: center;
}

.hero-content {
  padding-right: 30px;
}

.hero-title {
  font-size: 3.5rem;
  font-weight: 700;
  margin-bottom: 15px;
  line-height: 1.2;
}

.hero-subtitle {
  font-size: 1.3rem;
  color: rgba(255, 255, 255, 0.8);
  margin-bottom: 25px;
}

.hero-description {
  font-size: 1.1rem;
  line-height: 1.6;
  margin-bottom: 30px;
  color: rgba(255, 255, 255, 0.9);
}

.hero-info {
  display: flex;
  gap: 30px;
  margin-bottom: 40px;
}

.info-item {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 1.1rem;
}

.info-item i {
  font-size: 1.3rem;
  color: rgba(255, 255, 255, 0.8);
}

.price {
  font-size: 1.5rem;
  font-weight: 700;
  color: #ffd700;
}

.hero-actions {
  display: flex;
  gap: 20px;
  flex-wrap: wrap;
}

.hero-actions a {
  text-decoration: none;
}

.hero-image {
  text-align: center;
}

.hero-image img {
  max-width: 100%;
  height: auto;
  border-radius: 15px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.3);
}

.service-info {
  padding: 80px 0;
  background: #f8f9fa;
}

.service-info .info-item {
  color: #333;
  padding: 15px;
  background: #f8f9fa;
  border-radius: 10px;
}

.service-info .info-item i {
  color: #667eea;
}

.info-card,
.booking-card {
  background: white;
  border-radius: 15px;
  padding: 30px;
  box-shadow: 0 5px 20px rgba(0, 0, 0, 0.1);
  margin-bottom: 30px;
}

.info-grid {
  display: grid;
  gap: 20px;
}

.booking-card {
  text-align: center;
}

.btn-block {
  display: block;
  width: 100%;
  margin-bottom: 20px;
}

.contact-info {
  border-top: 1px solid #eee;
  padding-top: 20px;
  margin-top: 20px;
  text-align: left;
}

.contact-info p {
  margin-bottom: 10px;
  color: #666;
}

.contact-info i {
  color: #667eea;
  margin-right: 10px;
}

@media (max-width: 768px) {
  .hero {
    padding: 60px 0;
    text-align: center;
  }

  .hero-content {
    padding-right: 0;
    margin-bottom: 40px;
  }

  .hero-title {
    font-size: 2.5rem;
  }

  .hero-info {
    justify-content: center;
    flex-wrap: wrap;
    gap: 20px;
  }

  .hero-actions {
    justify-content: center;
  }

  .service-info {
    padding: 40px 0;
  }
}
</style>

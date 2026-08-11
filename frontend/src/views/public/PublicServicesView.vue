<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { fetchPublicServiceCategories, fetchPublicServices } from '@/services/publicApi'
import type { Service, ServiceCategory } from '@/types/api'
import { serviceImage } from '@/lib/images'

const services = ref<Service[]>([])
const categories = ref<ServiceCategory[]>([])
const loading = ref(true)
const categoryFilter = ref('')

function formatPrice(value: string) {
  return new Intl.NumberFormat('pt-PT', { style: 'currency', currency: 'EUR' }).format(
    Number(value),
  )
}

function categoryName(service: Service) {
  return service.service_category?.name?.trim() || service.category?.trim() || 'Sem categoria'
}

function imageFor(service: Service, index: number) {
  return serviceImage(service.image_url || service.image, index)
}

const filteredServices = computed(() => {
  if (!categoryFilter.value) return services.value
  if (categoryFilter.value === '__none__') {
    return services.value.filter((s) => !s.service_category_id)
  }
  const id = Number(categoryFilter.value)
  return services.value.filter((s) => s.service_category_id === id)
})

onMounted(async () => {
  try {
    const [servicesResponse, categoriesResponse] = await Promise.all([
      fetchPublicServices({ per_page: 100 }),
      fetchPublicServiceCategories(),
    ])
    services.value = servicesResponse.data ?? []
    categories.value = categoriesResponse ?? []
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="services">
    <section class="content">
      <div class="wrapper">
        <div class="box">
          <h2 class="title_bg">Ewellin Jordão Beauty</h2>
          <div class="item">
            <span class="subtitle">Catálogo</span>
            <h2 class="title">Serviços</h2>
          </div>
        </div>

        <div
          v-if="categories.length"
          class="category-filters mb-4 flex flex-wrap justify-center gap-2"
        >
          <button
            type="button"
            class="filter-chip"
            :class="{ active: !categoryFilter }"
            @click="categoryFilter = ''"
          >
            Todas
          </button>
          <button
            v-for="cat in categories"
            :key="cat.id"
            type="button"
            class="filter-chip"
            :class="{ active: categoryFilter === String(cat.id) }"
            @click="categoryFilter = String(cat.id)"
          >
            {{ cat.name }}
          </button>
        </div>

        <p v-if="loading" class="text-center text-muted">A carregar...</p>
        <p v-else-if="!filteredServices.length" class="text-center text-muted">
          Nenhum serviço disponível no momento.
        </p>

        <div v-else class="tabs">
          <div class="box_tabs">
            <RouterLink
              v-for="(service, index) in filteredServices"
              :key="service.id"
              :to="`/services/${service.id}`"
              class="item_tab"
            >
              <div class="bg">
                <div class="content text-center">
                  <img
                    :src="imageFor(service, index)"
                    :alt="service.name"
                    class="img-fluid mb-2"
                    style="max-height: 120px; object-fit: cover"
                  />
                  <h2 class="title">{{ service.name }}</h2>
                  <span class="subtitle">{{ categoryName(service) }}</span>
                  <div class="price-info mt-2">
                    <span class="price">{{ formatPrice(service.price) }}</span>
                    <span class="duration">{{ service.duration_minutes }} min</span>
                  </div>
                </div>
              </div>
            </RouterLink>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.services {
  font-family: 'Arial', sans-serif;
  margin-top: 80px;
}

.banner {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 60px 0;
  text-align: center;
  color: white;
}

.banner .item img {
  max-width: 100%;
  height: auto;
  border-radius: 10px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
}

.content {
  padding: 80px 0;
  background: #f8f9fa;
}

.wrapper {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.box {
  text-align: center;
  margin-bottom: 40px;
}

.title_bg {
  font-size: 5.8rem;
  color: #0a0000;
}

.filter-chip {
  border: 1px solid #d4d4d8;
  background: #fff;
  border-radius: 999px;
  padding: 0.4rem 0.9rem;
  font-size: 0.85rem;
  color: #52525b;
  cursor: pointer;
}

.filter-chip.active {
  background: #7c3aed;
  border-color: #7c3aed;
  color: #fff;
}

.tabs .box_tabs {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 1.5rem;
}

.item_tab {
  text-decoration: none;
  color: inherit;
}

.item_tab .bg {
  background: #fff;
  border-radius: 12px;
  padding: 1.25rem;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.06);
  transition: transform 0.15s ease;
}

.item_tab:hover .bg {
  transform: translateY(-2px);
}

.title {
  font-size: 1.1rem;
  margin: 0.5rem 0 0.25rem;
}

.subtitle {
  color: #888;
  font-size: 0.85rem;
}

.price-info {
  display: flex;
  justify-content: center;
  gap: 0.75rem;
  align-items: center;
}

.price {
  font-weight: 700;
  color: #333;
}

.duration {
  font-size: 0.85rem;
  color: #777;
}
</style>

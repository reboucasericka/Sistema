<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { fetchPublicProducts } from '@/services/publicApi'
import type { Product } from '@/types/api'

const products = ref<Product[]>([])
const loading = ref(true)

const grouped = computed(() => {
  const map = new Map<string, Product[]>()
  for (const product of products.value) {
    const key =
      product.product_category?.name?.trim() || product.category?.trim() || 'Sem categoria'
    if (!map.has(key)) map.set(key, [])
    map.get(key)!.push(product)
  }
  return [...map.entries()].sort(([a], [b]) => a.localeCompare(b, 'pt'))
})

function formatPrice(value: string) {
  return new Intl.NumberFormat('pt-PT', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(Number(value))
}

onMounted(async () => {
  try {
    const response = await fetchPublicProducts({ per_page: 100 })
    products.value = response.data ?? []
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <section class="produtos-section container mt-5" style="margin-top: 100px !important">
    <div class="container tabela-precos">
      <h2 class="titulo">Produtos</h2>
      <p class="subtitulo">Conheça os produtos disponíveis no nosso espaço</p>
    </div>

    <p v-if="loading" class="text-center text-muted">A carregar...</p>
    <p v-else-if="!products.length" class="text-center text-muted">Nenhum produto disponível no momento.</p>

    <template v-else>
      <div v-for="[category, items] in grouped" :key="category" class="categoria mb-3">
        <div class="categoria">{{ category }}</div>
        <div class="linha-categoria">
          <div
            v-for="item in items"
            :key="item.id"
            class="linha-preco d-flex justify-content-between align-items-center border-bottom py-2"
          >
            <span class="servico fw-semibold">{{ item.name }}</span>
            <span class="preco text-muted">€{{ formatPrice(item.price) }}</span>
          </div>
        </div>
      </div>
    </template>
  </section>
</template>

<style scoped>
.titulo {
  font-family: 'Playfair Display', serif;
  font-size: 2.2rem;
  margin-bottom: 0.5rem;
}

.subtitulo {
  color: #888;
  font-size: 1rem;
  margin-bottom: 2rem;
}

.linha-preco {
  font-family: 'Montserrat', sans-serif;
  font-size: 1rem;
}

.preco {
  color: #444;
}
</style>

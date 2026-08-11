<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { fetchPublicServices } from '@/services/publicApi'
import type { Service } from '@/types/api'

const services = ref<Service[]>([])
const loading = ref(true)

function formatPrice(value: string) {
  return new Intl.NumberFormat('pt-PT', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(Number(value))
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
  <div class="container tabela-precos" style="margin-top: 100px">
    <h2 class="titulo">Tabela de Preços</h2>

    <p v-if="loading" class="text-center text-muted">A carregar...</p>
    <p v-else-if="!services.length" class="text-center text-muted">Nenhum preço disponível no momento.</p>

    <template v-else>
      <div class="categoria">Serviços</div>
      <div v-for="item in services" :key="item.id" class="linha-preco">
        <span class="servico">{{ item.name }}</span>
        <span class="preco">€{{ formatPrice(item.price) }}</span>
      </div>
    </template>
  </div>
</template>

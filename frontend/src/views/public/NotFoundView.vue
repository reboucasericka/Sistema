<template>
  <div class="home" style="margin-top: 80px; padding: 4rem 1rem; text-align: center">
    <p style="font-size: 6rem; font-weight: 900; color: var(--accent-terracotta); line-height: 1">404</p>
    <h1 class="about-title">Página não encontrada</h1>
    <p class="about-text">A página que procura não existe.</p>
    <RouterLink to="/" class="about-link">VOLTAR AO INÍCIO</RouterLink>
  </div>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>

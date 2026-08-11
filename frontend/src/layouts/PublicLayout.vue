<template>
  <PublicHeader />
  <main role="main">
    <RouterView />
  </main>
  <PublicFooter />
</template>

<script setup lang="ts">
import { RouterView } from 'vue-router'
import PublicFooter from '@/components/layout/PublicFooter.vue'
import PublicHeader from '@/components/layout/PublicHeader.vue'
</script>

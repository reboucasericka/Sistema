<script setup lang="ts">
import { RouterView } from 'vue-router'
import AdminShell from '@/components/layout/AdminShell.vue'
import '@/styles/admin.css'
</script>

<template>
  <AdminShell>
    <RouterView />
  </AdminShell>
</template>

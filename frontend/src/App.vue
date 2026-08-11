<script setup lang="ts">
import { RouterView } from 'vue-router'
import ConfirmDialog from '@/components/ui/ConfirmDialog.vue'
import ToastContainer from '@/components/ui/ToastContainer.vue'
</script>

<template>
  <RouterView />
  <ToastContainer />
  <ConfirmDialog />
</template>

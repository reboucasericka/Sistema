<script setup lang="ts">
defineProps<{
  imageTitle: string
  imageText: string
  imageIcon: string
  title: string
  subtitle: string
  headerIcon: string
  cardMaxWidth?: string
  centered?: boolean
}>()
</script>

<template>
  <div class="auth-split">
    <div class="auth-split__image auth-image d-none d-md-flex">
      <div class="auth-image-content">
        <h3>{{ imageTitle }}</h3>
        <p>{{ imageText }}</p>
        <div class="mt-4">
          <i :class="[imageIcon, 'fa-3x', 'mb-3']" style="opacity: 0.8" aria-hidden="true" />
        </div>
      </div>
    </div>

    <div class="auth-split__form" :class="{ 'auth-split__form--centered': centered }">
      <div
        class="auth-card"
        :style="{ width: '100%', maxWidth: cardMaxWidth ?? '420px' }"
      >
        <div class="auth-header">
          <h2>
            <i :class="[headerIcon, 'me-2']" aria-hidden="true" />
            {{ title }}
          </h2>
          <p>{{ subtitle }}</p>
        </div>

        <div class="p-4">
          <slot />
        </div>
      </div>
    </div>
  </div>
</template>

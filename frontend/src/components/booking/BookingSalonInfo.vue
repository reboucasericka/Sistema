<script setup lang="ts">
defineProps<{
  salonName?: string
  welcomeMessage?: string
  description?: string
  paymentMethods?: string
  languages?: string
  facilities?: string
}>()
</script>

<template>
  <section class="booking-salon-info">
    <h2 class="booking-salon-info__title">{{ salonName }}</h2>
    <p v-if="welcomeMessage" class="booking-salon-info__welcome">{{ welcomeMessage }}</p>
    <p v-if="description" class="booking-salon-info__description">{{ description }}</p>

    <dl class="booking-salon-info__details">
      <div v-if="paymentMethods">
        <dt>Formas de pagamento</dt>
        <dd>{{ paymentMethods }}</dd>
      </div>
      <div v-if="languages">
        <dt>Idiomas</dt>
        <dd>{{ languages }}</dd>
      </div>
      <div v-if="facilities">
        <dt>Facilidades</dt>
        <dd>{{ facilities }}</dd>
      </div>
    </dl>
  </section>
</template>

<style scoped>
.booking-salon-info {
  margin-top: 4rem;
  padding: 2rem;
  border-radius: 16px;
  background: linear-gradient(135deg, #faf7f5 0%, #f3ece8 100%);
  border: 1px solid #ece3dd;
}

.booking-salon-info__title {
  font-family: 'Playfair Display', serif;
  font-size: 1.6rem;
  color: var(--neutral-dark);
  margin-bottom: 0.75rem;
}

.booking-salon-info__welcome {
  font-weight: 600;
  color: var(--accent-terracotta);
  margin-bottom: 0.75rem;
}

.booking-salon-info__description {
  color: #555;
  line-height: 1.65;
  margin-bottom: 1.5rem;
}

.booking-salon-info__details {
  display: grid;
  gap: 1rem;
}

.booking-salon-info__details dt {
  font-weight: 600;
  color: var(--neutral-dark);
  margin-bottom: 0.25rem;
}

.booking-salon-info__details dd {
  color: #666;
  font-size: 0.92rem;
  line-height: 1.5;
}
</style>

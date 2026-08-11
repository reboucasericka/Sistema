<script setup lang="ts">
export interface BookingReview {
  id: number
  rating: number
  text: string
  author: string
  date: string
}

defineProps<{
  reviews: BookingReview[]
  averageRating: number
  totalReviews: number
}>()

function stars(rating: number) {
  return Array.from({ length: 5 }, (_, index) => index < Math.round(rating))
}
</script>

<template>
  <section class="booking-reviews">
    <div class="booking-reviews__summary">
      <h2 class="booking-reviews__title">Avaliações dos nossos clientes</h2>
      <div class="booking-reviews__score">
        <span class="booking-reviews__average">{{ averageRating.toFixed(1) }}</span>
        <div class="booking-reviews__stars" aria-hidden="true">
          <span v-for="(filled, index) in stars(averageRating)" :key="index" :class="{ filled }"
            >★</span
          >
        </div>
        <p class="booking-reviews__count">{{ totalReviews }} avaliações</p>
      </div>
    </div>

    <div class="booking-reviews__list">
      <article v-for="review in reviews" :key="review.id" class="booking-review-card">
        <div class="booking-review-card__rating" aria-hidden="true">
          <span v-for="(filled, index) in stars(review.rating)" :key="index" :class="{ filled }"
            >★</span
          >
        </div>
        <p class="booking-review-card__text">{{ review.text }}</p>
        <p class="booking-review-card__meta">{{ review.author }}, em {{ review.date }}</p>
      </article>
    </div>
  </section>
</template>

<style scoped>
.booking-reviews {
  margin-top: 4rem;
  padding-top: 3rem;
  border-top: 1px solid #ececec;
}

.booking-reviews__summary {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.booking-reviews__title {
  font-family: 'Playfair Display', serif;
  font-size: 1.8rem;
  color: var(--neutral-dark);
}

.booking-reviews__score {
  text-align: right;
}

.booking-reviews__average {
  font-size: 2rem;
  font-weight: 700;
  color: var(--neutral-dark);
  margin-right: 0.5rem;
}

.booking-reviews__stars,
.booking-review-card__rating {
  display: inline-flex;
  gap: 0.1rem;
  color: #ddd;
}

.booking-reviews__stars span.filled,
.booking-review-card__rating span.filled {
  color: #f5b301;
}

.booking-reviews__count {
  font-size: 0.88rem;
  color: #888;
  margin-top: 0.25rem;
}

.booking-reviews__list {
  display: grid;
  gap: 1rem;
}

.booking-review-card {
  padding: 1rem 1.15rem;
  border: 1px solid #efefef;
  border-radius: 12px;
  background: #fafafa;
}

.booking-review-card__text {
  margin: 0.5rem 0;
  color: #555;
  line-height: 1.5;
}

.booking-review-card__meta {
  font-size: 0.85rem;
  color: #888;
}
</style>

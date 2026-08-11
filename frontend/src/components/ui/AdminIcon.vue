<script setup lang="ts">
defineProps<{
  name:
    | 'home'
    | 'calendar'
    | 'users'
    | 'service'
    | 'box'
    | 'stock'
    | 'sale'
    | 'cash'
    | 'mail'
    | 'external'
}>()
</script>

<template>
  <svg
    class="h-5 w-5 shrink-0"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    stroke-width="1.75"
    stroke-linecap="round"
    stroke-linejoin="round"
    aria-hidden="true"
  >
    <template v-if="name === 'home'">
      <path d="M3 10.5 12 3l9 7.5" />
      <path d="M5 9.5V20a1 1 0 0 0 1 1h4v-6h4v6h4a1 1 0 0 0 1-1V9.5" />
    </template>
    <template v-else-if="name === 'calendar'">
      <rect x="3" y="5" width="18" height="16" rx="2" />
      <path d="M16 3v4M8 3v4M3 11h18" />
    </template>
    <template v-else-if="name === 'users'">
      <path d="M16 19a4 4 0 0 0-8 0" />
      <circle cx="12" cy="9" r="3" />
      <path d="M20 19a3 3 0 0 0-2.8-4M4 19a3 3 0 0 1 2.8-4" />
    </template>
    <template v-else-if="name === 'service'">
      <path d="M12 3l2.4 4.9 5.4.8-3.9 3.8.9 5.3L12 15.8 7.2 17.8l.9-5.3L4.2 8.7l5.4-.8L12 3z" />
    </template>
    <template v-else-if="name === 'box'">
      <path d="M12 3 21 8v8l-9 5-9-5V8l9-5z" />
      <path d="M12 12 21 8M12 12V22M12 12 3 8" />
    </template>
    <template v-else-if="name === 'stock'">
      <path d="M4 19V5" />
      <path d="M4 19h16" />
      <path d="M8 15v-4M12 15V9M16 15V7" />
    </template>
    <template v-else-if="name === 'sale'">
      <circle cx="9" cy="20" r="1.5" />
      <circle cx="18" cy="20" r="1.5" />
      <path d="M2 4h2l2.5 12H18l2.5-9H6" />
    </template>
    <template v-else-if="name === 'cash'">
      <rect x="2" y="6" width="20" height="12" rx="2" />
      <circle cx="12" cy="12" r="2.5" />
      <path d="M6 10h.01M18 14h.01" />
    </template>
    <template v-else-if="name === 'mail'">
      <rect x="3" y="5" width="18" height="14" rx="2" />
      <path d="m3 7 9 6 9-6" />
    </template>
    <template v-else-if="name === 'external'">
      <path d="M14 3h7v7" />
      <path d="M10 14 21 3" />
      <path d="M21 14v6a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1h6" />
    </template>
  </svg>
</template>

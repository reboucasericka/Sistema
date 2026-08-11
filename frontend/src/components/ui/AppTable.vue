<script setup lang="ts">
defineProps<{
  compact?: boolean
}>()
</script>

<template>
  <div class="w-full overflow-hidden">
    <table class="admin-table w-full" :class="compact ? 'text-xs' : ''">
      <slot />
    </table>
  </div>
</template>

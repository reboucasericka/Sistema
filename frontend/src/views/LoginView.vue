<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import AuthPageShell from '@/components/auth/AuthPageShell.vue'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'

const auth = useAuthStore()
const ui = useUiStore()
const router = useRouter()
const route = useRoute()

const email = ref('admin@salon.test')
const password = ref('password')

async function submit() {
  try {
    const user = await auth.login(email.value, password.value)
    ui.success('Login efetuado com sucesso.')
    const redirect = (route.query.redirect as string) || auth.homeRouteForRole(user.role)
    await router.push(redirect)
  } catch {
    ui.error(auth.error ?? 'Não foi possível entrar.')
  }
}
</script>

<template>
  <AuthPageShell
    image-title="Bem-vinda de volta!"
    image-text="Entre na sua conta e continue a sua jornada de beleza connosco."
    image-icon="fas fa-spa"
    title="Entrar"
    subtitle="Aceda à sua conta para continuar"
    header-icon="fas fa-sign-in-alt"
    centered
  >
    <form @submit.prevent="submit">
      <div class="mb-3">
        <label for="login-email" class="form-label">
          <i class="fas fa-envelope me-2" style="color: var(--accent-terracotta)" aria-hidden="true" />
          Email
        </label>
        <input
          id="login-email"
          v-model="email"
          type="email"
          class="form-control"
          placeholder="O seu email"
          required
          autocomplete="email"
        />
      </div>

      <div class="mb-3">
        <label for="login-password" class="form-label">
          <i class="fas fa-lock me-2" style="color: var(--accent-terracotta)" aria-hidden="true" />
          Password
        </label>
        <input
          id="login-password"
          v-model="password"
          type="password"
          class="form-control"
          placeholder="A sua password"
          required
          autocomplete="current-password"
        />
      </div>

      <div class="d-grid gap-2 mb-3">
        <button type="submit" class="btn btn-primary btn-lg" :disabled="auth.loading">
          <span
            v-if="auth.loading"
            class="spinner-border spinner-border-sm me-2"
            role="status"
            aria-hidden="true"
          />
          Entrar
        </button>
      </div>

      <div class="text-center mt-3">
        <p class="mb-2">Não tem conta?</p>
        <RouterLink
          :to="{ name: 'register', query: route.query.redirect ? { redirect: route.query.redirect } : {} }"
          class="btn btn-outline-primary"
        >
          <i class="fas fa-user-plus me-1" aria-hidden="true" />
          Criar conta
        </RouterLink>
      </div>

      <div class="alert alert-light border mt-4 mb-0 small text-muted">
        <strong class="text-body">Contas de teste:</strong>
        admin@salon.test · maria.silva@salon.test · password
      </div>
    </form>
  </AuthPageShell>
</template>

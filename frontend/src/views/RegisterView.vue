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

const firstName = ref('')
const lastName = ref('')
const email = ref('')
const phone = ref('')
const password = ref('')
const passwordConfirmation = ref('')

async function submit() {
  try {
    const user = await auth.register({
      first_name: firstName.value,
      last_name: lastName.value,
      email: email.value,
      phone: phone.value || undefined,
      password: password.value,
      password_confirmation: passwordConfirmation.value,
    })
    ui.success('Conta criada com sucesso.')
    const redirect = (route.query.redirect as string) || auth.homeRouteForRole(user.role)
    await router.push(redirect)
  } catch {
    ui.error(auth.error ?? 'Não foi possível criar a conta.')
  }
}
</script>

<template>
  <AuthPageShell
    image-title="Venha fazer parte!"
    image-text="Registe o seu perfil para agendar os seus tratamentos de beleza connosco."
    image-icon="fas fa-user-plus"
    title="Criar conta"
    subtitle="Crie a sua conta e comece a sua jornada"
    header-icon="fas fa-user-plus"
    card-max-width="500px"
  >
    <form @submit.prevent="submit">
      <div class="auth-field-grid">
        <div class="mb-3">
          <label for="register-first-name" class="form-label">
            <i class="fas fa-user me-2" style="color: var(--accent-terracotta)" aria-hidden="true" />
            Nome
          </label>
          <input
            id="register-first-name"
            v-model="firstName"
            type="text"
            class="form-control"
            placeholder="O seu nome"
            required
            autocomplete="given-name"
          />
        </div>
        <div class="mb-3">
          <label for="register-last-name" class="form-label">
            <i class="fas fa-user me-2" style="color: var(--accent-terracotta)" aria-hidden="true" />
            Apelido
          </label>
          <input
            id="register-last-name"
            v-model="lastName"
            type="text"
            class="form-control"
            placeholder="O seu apelido"
            required
            autocomplete="family-name"
          />
        </div>
      </div>

      <div class="mb-3">
        <label for="register-email" class="form-label">
          <i class="fas fa-envelope me-2" style="color: var(--accent-terracotta)" aria-hidden="true" />
          Email
        </label>
        <input
          id="register-email"
          v-model="email"
          type="email"
          class="form-control"
          placeholder="O seu email"
          required
          autocomplete="email"
        />
        <div class="form-text">Usaremos este email para confirmar os seus agendamentos.</div>
      </div>

      <div class="mb-3">
        <label for="register-phone" class="form-label">
          <i class="fas fa-mobile-alt me-2" style="color: var(--accent-terracotta)" aria-hidden="true" />
          Telefone
        </label>
        <input
          id="register-phone"
          v-model="phone"
          type="tel"
          class="form-control"
          placeholder="Telemóvel"
          autocomplete="tel"
        />
      </div>

      <div class="mb-3">
        <label for="register-password" class="form-label">
          <i class="fas fa-lock me-2" style="color: var(--accent-terracotta)" aria-hidden="true" />
          Password
        </label>
        <input
          id="register-password"
          v-model="password"
          type="password"
          class="form-control"
          placeholder="Mínimo 8 caracteres"
          required
          autocomplete="new-password"
        />
      </div>

      <div class="mb-3">
        <label for="register-password-confirm" class="form-label">
          <i class="fas fa-lock me-2" style="color: var(--accent-terracotta)" aria-hidden="true" />
          Confirmar password
        </label>
        <input
          id="register-password-confirm"
          v-model="passwordConfirmation"
          type="password"
          class="form-control"
          placeholder="Repita a password"
          required
          autocomplete="new-password"
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
          <i v-else class="fas fa-user-plus me-2" aria-hidden="true" />
          Criar conta
        </button>
      </div>

      <div class="text-center mt-3">
        <p class="mb-2">Já tem conta?</p>
        <RouterLink
          :to="{ name: 'login', query: route.query.redirect ? { redirect: route.query.redirect } : {} }"
          class="btn btn-outline-primary"
        >
          <i class="fas fa-sign-in-alt me-1" aria-hidden="true" />
          Entrar
        </RouterLink>
      </div>
    </form>
  </AuthPageShell>
</template>

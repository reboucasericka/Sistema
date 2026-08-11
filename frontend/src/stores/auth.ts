import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { api, getErrorMessage } from '@/lib/api'
import type { LoginResponse, RegisterResponse, User, UserRole } from '@/types/api'

const USER_KEY = 'auth_user'
const TOKEN_KEY = 'auth_token'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(loadUser())
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
  const loading = ref(false)
  const error = ref<string | null>(null)

  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const role = computed(() => user.value?.role ?? null)
  const isAdmin = computed(() => role.value === 'admin')
  const isProfessional = computed(() => role.value === 'professional')
  const isClient = computed(() => role.value === 'client')
  const fullName = computed(() => user.value?.full_name ?? '')

  function loadUser(): User | null {
    const raw = localStorage.getItem(USER_KEY)
    if (!raw) return null
    try {
      return JSON.parse(raw) as User
    } catch {
      return null
    }
  }

  function persist(session: { token: string; user: User }) {
    token.value = session.token
    user.value = session.user
    localStorage.setItem(TOKEN_KEY, session.token)
    localStorage.setItem(USER_KEY, JSON.stringify(session.user))
  }

  function clear() {
    token.value = null
    user.value = null
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  }

  async function login(email: string, password: string) {
    loading.value = true
    error.value = null
    try {
      const { data } = await api.post<LoginResponse>('/api/v1/auth/login', {
        email,
        password,
        device_name: 'salon-web',
      })
      persist({ token: data.token, user: data.user })
      return data.user
    } catch (err) {
      error.value = getErrorMessage(err, 'Credenciais inválidas.')
      throw err
    } finally {
      loading.value = false
    }
  }

  async function register(payload: {
    first_name: string
    last_name: string
    email: string
    phone?: string
    password: string
    password_confirmation: string
  }) {
    loading.value = true
    error.value = null
    try {
      const { data } = await api.post<RegisterResponse>('/api/v1/auth/register', {
        ...payload,
        device_name: 'salon-web',
      })
      persist({ token: data.token, user: data.user })
      return data.user
    } catch (err) {
      error.value = getErrorMessage(err, 'Não foi possível criar a conta.')
      throw err
    } finally {
      loading.value = false
    }
  }

  async function fetchMe() {
    const { data } = await api.get<{ user: User }>('/api/v1/auth/me')
    user.value = data.user
    localStorage.setItem(USER_KEY, JSON.stringify(data.user))
    return data.user
  }

  async function logout() {
    try {
      await api.post('/api/v1/auth/logout')
    } finally {
      clear()
    }
  }

  function homeRouteForRole(userRole: UserRole | null): string {
    if (userRole === 'admin' || userRole === 'professional') return '/panel/admin/dashboard'
    if (userRole === 'client') return '/panel/client/dashboard'
    return '/login'
  }

  return {
    user,
    token,
    loading,
    error,
    isAuthenticated,
    role,
    isAdmin,
    isProfessional,
    isClient,
    fullName,
    login,
    register,
    fetchMe,
    logout,
    clear,
    homeRouteForRole,
  }
})

import axios from 'axios'

const baseURL = import.meta.env.VITE_API_URL ?? ''

export const api = axios.create({
  baseURL,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('auth_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('auth_token')
      localStorage.removeItem('auth_user')
      if (window.location.pathname.startsWith('/panel')) {
        const redirect = encodeURIComponent(window.location.pathname + window.location.search)
        window.location.href = `/login?redirect=${redirect}`
      }
    }
    return Promise.reject(error)
  },
)

export function getErrorMessage(error: unknown, fallback = 'Ocorreu um erro.'): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as { message?: string; errors?: Record<string, string[]> }
    if (data?.errors) {
      const fieldMessages = Object.values(data.errors).flat().filter(Boolean)
      if (fieldMessages.length) return fieldMessages.join(' ')
    }
    if (data?.message) return data.message
  }
  return fallback
}

export function getValidationErrors(error: unknown): Record<string, string> {
  if (!axios.isAxiosError(error)) return {}
  const data = error.response?.data as { errors?: Record<string, string[]> } | undefined
  if (!data?.errors) return {}

  return Object.fromEntries(
    Object.entries(data.errors).map(([field, messages]) => [field, messages[0] ?? '']),
  )
}

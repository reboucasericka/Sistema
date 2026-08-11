import { api } from '@/lib/api'
import type {
  ApiMessageResponse,
  Appointment,
  AvailabilityResponse,
  ContactMessage,
  Paginated,
  Product,
  ProductCategory,
  PublicProfessional,
  Service,
  ServiceCategory,
} from '@/types/api'

export type PublicListParams = Record<string, string | number | boolean | undefined>

export async function fetchPublicServices(params?: PublicListParams) {
  const { data } = await api.get<Paginated<Service>>('/api/v1/public/services', { params })
  return data
}

export async function fetchPublicProducts(params?: PublicListParams) {
  const { data } = await api.get<Paginated<Product>>('/api/v1/public/products', { params })
  return data
}

export async function fetchPublicServiceCategories() {
  const { data } = await api.get<{ data: ServiceCategory[] }>('/api/v1/public/service-categories')
  return data.data
}

export async function fetchPublicProductCategories() {
  const { data } = await api.get<{ data: ProductCategory[] }>('/api/v1/public/product-categories')
  return data.data
}

export async function fetchPublicProfessionals(params?: PublicListParams) {
  const { data } = await api.get<Paginated<PublicProfessional>>('/api/v1/public/professionals', { params })
  return data
}

export async function fetchPublicProfessional(id: number) {
  const { data } = await api.get<{ data: PublicProfessional }>(`/api/v1/public/professionals/${id}`)
  return data.data
}

export async function fetchPublicServiceProfessionals(serviceId: number) {
  const { data } = await api.get<{ data: PublicProfessional[] }>(
    `/api/v1/public/services/${serviceId}/professionals`,
  )
  return data.data
}

export async function fetchAvailability(params: {
  professional_id: number
  service_id: number
  date: string
}) {
  const { data } = await api.get<{ data: AvailabilityResponse }>('/api/v1/public/availability', { params })
  return data.data
}

export type CreatePublicAppointmentPayload = {
  service_id: number
  professional_id: number
  date: string
  time: string
  notes?: string
  client_name?: string
  client_email?: string
  client_phone?: string
}

export async function createPublicAppointment(payload: CreatePublicAppointmentPayload) {
  const { data } = await api.post<ApiMessageResponse<Appointment>>('/api/v1/public/appointments', payload)
  return data
}

export type ContactMessagePayload = {
  name: string
  email: string
  phone?: string
  subject?: string
  message: string
}

export async function submitContactMessage(payload: ContactMessagePayload) {
  const { data } = await api.post<ApiMessageResponse<ContactMessage>>('/api/v1/public/contact', payload)
  return data
}

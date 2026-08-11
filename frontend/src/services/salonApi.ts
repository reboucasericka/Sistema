import { api } from '@/lib/api'
import type {
  ApiMessageResponse,
  Appointment,
  CashRegisterSummary,
  CashReport,
  Client,
  ContactMessage,
  DashboardData,
  Notification,
  Paginated,
  Product,
  ProductCategory,
  Professional,
  ProfessionalPayload,
  ProfessionalSchedule,
  ProfessionalSchedulePayload,
  Sale,
  Service,
  ServiceCategory,
  StockMovement,
} from '@/types/api'

export type ListParams = Record<string, string | number | boolean | undefined>

export async function fetchDashboard() {
  const { data } = await api.get<{ data: DashboardData }>('/api/v1/dashboard')
  return data.data
}

export async function fetchClients(params?: ListParams) {
  const { data } = await api.get<Paginated<Client>>('/api/v1/clients', { params })
  return data
}

export async function fetchClient(id: number) {
  const { data } = await api.get<{ data: Client }>(`/api/v1/clients/${id}`)
  return data.data
}

export async function createClient(payload: Partial<Client>) {
  const { data } = await api.post<ApiMessageResponse<Client>>('/api/v1/clients', payload)
  return data.data
}

export async function updateClient(id: number, payload: Partial<Client>) {
  const { data } = await api.put<ApiMessageResponse<Client>>(`/api/v1/clients/${id}`, payload)
  return data.data
}

export async function deleteClient(id: number) {
  await api.delete(`/api/v1/clients/${id}`)
}

export async function fetchServices(params?: ListParams) {
  const { data } = await api.get<Paginated<Service>>('/api/v1/services', { params })
  return data
}

export async function createService(payload: Partial<Service> | FormData) {
  const { data } = await api.post<ApiMessageResponse<Service>>(
    '/api/v1/services',
    payload,
    payload instanceof FormData ? { headers: { 'Content-Type': 'multipart/form-data' } } : undefined,
  )
  return data.data
}

export async function updateService(id: number, payload: Partial<Service> | FormData) {
  if (payload instanceof FormData) {
    const { data } = await api.post<ApiMessageResponse<Service>>(
      `/api/v1/services/${id}`,
      payload,
      { headers: { 'Content-Type': 'multipart/form-data' } },
    )
    return data.data
  }
  const { data } = await api.put<ApiMessageResponse<Service>>(`/api/v1/services/${id}`, payload)
  return data.data
}

export async function deleteService(id: number) {
  await api.delete(`/api/v1/services/${id}`)
}

export async function fetchServiceCategories(params?: ListParams) {
  const { data } = await api.get<Paginated<ServiceCategory>>('/api/v1/service-categories', { params })
  return data
}

export async function createServiceCategory(payload: Partial<ServiceCategory>) {
  const { data } = await api.post<ApiMessageResponse<ServiceCategory>>('/api/v1/service-categories', payload)
  return data.data
}

export async function updateServiceCategory(id: number, payload: Partial<ServiceCategory>) {
  const { data } = await api.put<ApiMessageResponse<ServiceCategory>>(`/api/v1/service-categories/${id}`, payload)
  return data.data
}

export async function deleteServiceCategory(id: number) {
  await api.delete(`/api/v1/service-categories/${id}`)
}

export async function fetchProductCategories(params?: ListParams) {
  const { data } = await api.get<Paginated<ProductCategory>>('/api/v1/product-categories', { params })
  return data
}

export async function createProductCategory(payload: Partial<ProductCategory>) {
  const { data } = await api.post<ApiMessageResponse<ProductCategory>>('/api/v1/product-categories', payload)
  return data.data
}

export async function updateProductCategory(id: number, payload: Partial<ProductCategory>) {
  const { data } = await api.put<ApiMessageResponse<ProductCategory>>(`/api/v1/product-categories/${id}`, payload)
  return data.data
}

export async function deleteProductCategory(id: number) {
  await api.delete(`/api/v1/product-categories/${id}`)
}

export async function fetchProfessionals(params?: ListParams) {
  const { data } = await api.get<Paginated<Professional>>('/api/v1/professionals', {
    params: { per_page: 100, ...params },
  })
  return data
}

export async function fetchProfessional(id: number) {
  const { data } = await api.get<{ data: Professional }>(`/api/v1/professionals/${id}`)
  return data.data
}

export async function createProfessional(payload: ProfessionalPayload | FormData) {
  const { data } = await api.post<ApiMessageResponse<Professional>>(
    '/api/v1/professionals',
    payload,
    payload instanceof FormData ? { headers: { 'Content-Type': 'multipart/form-data' } } : undefined,
  )
  return data.data
}

export async function updateProfessional(id: number, payload: Partial<ProfessionalPayload> | FormData) {
  if (payload instanceof FormData) {
    const { data } = await api.post<ApiMessageResponse<Professional>>(
      `/api/v1/professionals/${id}`,
      payload,
      { headers: { 'Content-Type': 'multipart/form-data' } },
    )
    return data.data
  }
  const { data } = await api.put<ApiMessageResponse<Professional>>(
    `/api/v1/professionals/${id}`,
    payload,
  )
  return data.data
}

export async function deleteProfessional(id: number) {
  await api.delete(`/api/v1/professionals/${id}`)
}

export async function toggleProfessionalStatus(id: number, isActive: boolean) {
  return updateProfessional(id, { is_active: isActive })
}

export async function fetchProfessionalServices(professionalId: number) {
  const { data } = await api.get<{ data: Service[] }>(`/api/v1/professionals/${professionalId}/services`)
  return data.data
}

export async function syncProfessionalServices(professionalId: number, serviceIds: number[]) {
  const { data } = await api.put<{
    message: string
    data: { professional_id: number; services: Service[]; services_count: number }
  }>(`/api/v1/professionals/${professionalId}/services`, { service_ids: serviceIds })
  return data.data
}

export async function fetchProfessionalSchedules(params?: ListParams) {
  const { data } = await api.get<Paginated<ProfessionalSchedule>>('/api/v1/professional-schedules', {
    params,
  })
  return data
}

export async function fetchProfessionalSchedule(id: number) {
  const { data } = await api.get<{ data: ProfessionalSchedule }>(`/api/v1/professional-schedules/${id}`)
  return data.data
}

export async function createProfessionalSchedule(payload: ProfessionalSchedulePayload) {
  const { data } = await api.post<ApiMessageResponse<ProfessionalSchedule>>(
    '/api/v1/professional-schedules',
    payload,
  )
  return data.data
}

export async function updateProfessionalSchedule(id: number, payload: ProfessionalSchedulePayload) {
  const { data } = await api.put<ApiMessageResponse<ProfessionalSchedule>>(
    `/api/v1/professional-schedules/${id}`,
    payload,
  )
  return data.data
}

export async function deleteProfessionalSchedule(id: number) {
  await api.delete(`/api/v1/professional-schedules/${id}`)
}

export async function fetchAppointments(params?: ListParams) {
  const { data } = await api.get<Paginated<Appointment>>('/api/v1/appointments', { params })
  return data
}

export async function createAppointment(payload: {
  client_id: number
  service_id: number
  professional_id: number
  start_time: string
  notes?: string
  status?: string
}) {
  const { data } = await api.post<ApiMessageResponse<Appointment>>('/api/v1/appointments', payload)
  return data.data
}

export async function updateAppointment(id: number, payload: Record<string, unknown>) {
  const { data } = await api.put<ApiMessageResponse<Appointment>>(`/api/v1/appointments/${id}`, payload)
  return data.data
}

export async function cancelAppointment(id: number) {
  await api.delete(`/api/v1/appointments/${id}`)
}

export async function fetchNotifications(unreadOnly = false) {
  const { data } = await api.get<Paginated<Notification>>('/api/v1/notifications', {
    params: { unread_only: unreadOnly },
  })
  return data
}

export async function markNotificationRead(id: number) {
  await api.patch(`/api/v1/notifications/${id}/read`)
}

export async function fetchProducts(params?: ListParams) {
  const { data } = await api.get<Paginated<Product>>('/api/v1/products', { params })
  return data
}

export async function createProduct(payload: Partial<Product> | FormData) {
  const { data } = await api.post<ApiMessageResponse<Product>>(
    '/api/v1/products',
    payload,
    payload instanceof FormData ? { headers: { 'Content-Type': 'multipart/form-data' } } : undefined,
  )
  return data.data
}

export async function updateProduct(id: number, payload: Partial<Product> | FormData) {
  if (payload instanceof FormData) {
    const { data } = await api.post<ApiMessageResponse<Product>>(`/api/v1/products/${id}`, payload, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return data.data
  }
  const { data } = await api.put<ApiMessageResponse<Product>>(`/api/v1/products/${id}`, payload)
  return data.data
}

export async function deleteProduct(id: number) {
  await api.delete(`/api/v1/products/${id}`)
}

export async function fetchStockHistory(params?: ListParams) {
  const { data } = await api.get<Paginated<StockMovement>>('/api/v1/stock/history', { params })
  return data
}

export async function fetchLowStockProducts() {
  const { data } = await api.get<Paginated<Product>>('/api/v1/stock/low')
  return data
}

export async function stockEntry(payload: { product_id: number; quantity: number; notes?: string; reason?: string }) {
  const { data } = await api.post<ApiMessageResponse<StockMovement>>('/api/v1/stock/entry', payload)
  return data.data
}

export async function stockExit(payload: { product_id: number; quantity: number; notes?: string; reason?: string }) {
  const { data } = await api.post<ApiMessageResponse<StockMovement>>('/api/v1/stock/exit', payload)
  return data.data
}

export async function stockAdjustment(payload: {
  product_id: number
  new_quantity: number
  notes?: string
  reason?: string
}) {
  const { data } = await api.post<ApiMessageResponse<StockMovement>>('/api/v1/stock/adjustment', payload)
  return data.data
}

export async function fetchStockMovements(params?: ListParams) {
  const { data } = await api.get<Paginated<StockMovement>>('/api/v1/stock-movements', { params })
  return data
}

export async function stockIn(productId: number, payload: { quantity: number; reason?: string; notes?: string }) {
  const { data } = await api.post<ApiMessageResponse<StockMovement>>(
    `/api/v1/products/${productId}/stock/in`,
    payload,
  )
  return data.data
}

export async function stockOut(productId: number, payload: { quantity: number; reason?: string; notes?: string }) {
  const { data } = await api.post<ApiMessageResponse<StockMovement>>(
    `/api/v1/products/${productId}/stock/out`,
    payload,
  )
  return data.data
}

export async function stockAdjust(
  productId: number,
  payload: { new_quantity: number; reason?: string; notes?: string },
) {
  const { data } = await api.post<ApiMessageResponse<StockMovement>>(
    `/api/v1/products/${productId}/stock/adjust`,
    payload,
  )
  return data.data
}

export async function fetchSales(params?: ListParams) {
  const { data } = await api.get<Paginated<Sale>>('/api/v1/sales', { params })
  return data
}

export async function fetchSale(id: number) {
  const { data } = await api.get<{ data: Sale }>(`/api/v1/sales/${id}`)
  return data.data
}

export async function createSale(payload: {
  client_id?: number | null
  payment_method: string
  status?: string
  notes?: string
  items: Array<{
    product_id?: number
    service_id?: number
    description?: string
    quantity?: number
    unit_price?: number | string
  }>
}) {
  const { data } = await api.post<ApiMessageResponse<Sale>>('/api/v1/sales', payload)
  return data.data
}

export async function cancelSale(id: number) {
  const { data } = await api.post<ApiMessageResponse<Sale>>(`/api/v1/sales/${id}/cancel`)
  return data.data
}

export async function fetchContactMessages(params?: ListParams) {
  const { data } = await api.get<Paginated<ContactMessage>>('/api/v1/contact-messages', { params })
  return data
}

export async function markContactMessageRead(id: number) {
  const { data } = await api.patch<ApiMessageResponse<ContactMessage>>(`/api/v1/contact-messages/${id}/read`)
  return data.data
}

export async function openCashRegister(opening_amount: number) {
  const { data } = await api.post<ApiMessageResponse<unknown>>('/api/v1/cash/open', { opening_amount })
  return data
}

export async function closeCashRegister(closing_amount: number) {
  const { data } = await api.post<ApiMessageResponse<unknown>>('/api/v1/cash/close', { closing_amount })
  return data
}

export async function recordCashIncome(amount: number, description?: string) {
  const { data } = await api.post<ApiMessageResponse<unknown>>('/api/v1/cash/income', { amount, description })
  return data
}

export async function recordCashExpense(amount: number, description?: string) {
  const { data } = await api.post<ApiMessageResponse<unknown>>('/api/v1/cash/expense', { amount, description })
  return data
}

export async function fetchCurrentCashRegister() {
  const { data } = await api.get<{ data: CashRegisterSummary | null }>('/api/v1/cash/current')
  return data.data
}

export async function fetchCashReport(date?: string) {
  const { data } = await api.get<{ data: CashReport }>('/api/v1/cash/report', {
    params: date ? { date } : undefined,
  })
  return data.data
}

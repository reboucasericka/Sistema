<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import BookingDateStrip from '@/components/booking/BookingDateStrip.vue'
import { getErrorMessage } from '@/lib/api'
import {
  formatPrice,
  groupSlotsByPeriod,
  TIME_PERIOD_LABELS,
  type TimePeriod,
} from '@/lib/booking'
import {
  createPublicAppointment,
  fetchAvailability,
  fetchPublicServiceProfessionals,
  fetchPublicServices,
} from '@/services/publicApi'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'
import type { PublicProfessional, Service } from '@/types/api'

const ALL_PERIODS: TimePeriod[] = ['morning', 'afternoon', 'evening']

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const ui = useUiStore()

const step = ref<'schedule' | 'details' | 'confirm'>('schedule')
const loading = ref(false)
const submitting = ref(false)
const services = ref<Service[]>([])
const professionals = ref<PublicProfessional[]>([])
const professionalsLoading = ref(false)
const slots = ref<string[]>([])
const slotsLoading = ref(false)

const form = reactive({
  serviceId: 0,
  professionalId: 0,
  date: '',
  time: '',
  notes: '',
  clientName: '',
  clientEmail: '',
  clientPhone: '',
})

const selectedService = computed(() => services.value.find((s) => s.id === form.serviceId))
const selectedProfessional = computed(() =>
  professionals.value.find((p) => p.id === form.professionalId),
)

const minDate = computed(() => new Date().toISOString().split('T')[0] ?? '')

const groupedSlots = computed(() => groupSlotsByPeriod(slots.value))

async function loadProfessionalsForService(serviceId: number, preferredProfessionalId?: number) {
  if (!serviceId) {
    professionals.value = []
    form.professionalId = 0
    return
  }

  professionalsLoading.value = true
  try {
    professionals.value = await fetchPublicServiceProfessionals(serviceId)

    if (
      preferredProfessionalId &&
      professionals.value.some((pro) => pro.id === preferredProfessionalId)
    ) {
      form.professionalId = preferredProfessionalId
    } else if (professionals.value.length >= 1) {
      form.professionalId = professionals.value[0]!.id
    } else {
      form.professionalId = 0
    }
  } catch (err) {
    professionals.value = []
    form.professionalId = 0
    ui.error(getErrorMessage(err, 'Não foi possível carregar profissionais para este serviço.'))
  } finally {
    professionalsLoading.value = false
  }
}

onMounted(async () => {
  loading.value = true
  try {
    const servicesRes = await fetchPublicServices({ per_page: 500 })
    services.value = servicesRes.data ?? []

    const serviceId = Number(route.query.serviceId)
    if (serviceId) {
      form.serviceId = serviceId
    } else if (services.value.length === 1) {
      form.serviceId = services.value[0]!.id
    } else {
      await router.replace({ name: 'public-booking' })
      return
    }

    const preferredProfessionalId = Number(route.query.professionalId) || undefined
    await loadProfessionalsForService(form.serviceId, preferredProfessionalId)

    if (auth.isAuthenticated && auth.user) {
      form.clientName = auth.user.full_name
      form.clientEmail = auth.user.email
      form.clientPhone = auth.user.phone ?? ''
    }
  } catch (err) {
    ui.error(getErrorMessage(err))
  } finally {
    loading.value = false
  }
})

watch(
  () => form.serviceId,
  async (serviceId, previous) => {
    if (!serviceId || serviceId === previous || loading.value) return
    form.professionalId = 0
    form.time = ''
    slots.value = []
    await loadProfessionalsForService(serviceId)
  },
)

watch(
  () => [form.professionalId, form.serviceId, form.date] as const,
  async ([professionalId, serviceId, date]) => {
    if (!professionalId || !serviceId || !date) {
      slots.value = []
      form.time = ''
      return
    }

    slotsLoading.value = true
    try {
      const availability = await fetchAvailability({
        professional_id: professionalId,
        service_id: serviceId,
        date,
      })
      slots.value = availability.slots
      if (!slots.value.includes(form.time)) form.time = ''
    } catch (err) {
      slots.value = []
      ui.error(getErrorMessage(err, 'Não foi possível carregar horários.'))
    } finally {
      slotsLoading.value = false
    }
  },
)

function canProceedSchedule() {
  return !!form.serviceId && !!form.professionalId && !!form.date && !!form.time
}

function canProceedDetails() {
  if (auth.isAuthenticated) return true
  return !!form.clientName && !!form.clientEmail
}

function goToDetails() {
  if (!canProceedSchedule()) return
  step.value = 'details'
}

function goToConfirm() {
  if (!canProceedDetails()) return
  step.value = 'confirm'
}

async function submitBooking() {
  submitting.value = true
  try {
    await createPublicAppointment({
      service_id: form.serviceId,
      professional_id: form.professionalId,
      date: form.date,
      time: form.time,
      notes: form.notes || undefined,
      client_name: auth.isAuthenticated ? undefined : form.clientName,
      client_email: auth.isAuthenticated ? undefined : form.clientEmail,
      client_phone: auth.isAuthenticated ? undefined : form.clientPhone || undefined,
    })

    ui.success('Agendamento criado com sucesso!')

    if (auth.isAuthenticated && auth.isClient) {
      await router.push('/panel/client/appointments')
    } else {
      await router.push({ name: 'public-booking', query: { success: '1' } })
    }
  } catch (err) {
    ui.error(getErrorMessage(err, 'Não foi possível criar o agendamento.'))
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="booking-flow">
    <div class="booking-flow__wrapper">
      <RouterLink to="/booking" class="booking-flow__back">← Voltar aos serviços</RouterLink>

      <div v-if="loading" class="booking-flow__loading">A carregar...</div>

      <template v-else>
        <article v-if="selectedService" class="booking-flow__service-card">
          <div>
            <span class="booking-flow__label">Serviço</span>
            <h1 class="booking-flow__title">{{ selectedService.name }}</h1>
          </div>
          <span class="booking-flow__price">{{ formatPrice(selectedService.price) }}</span>
        </article>

        <section v-if="step === 'schedule'" class="booking-flow__section">
          <div class="booking-flow__professionals">
            <label class="booking-flow__prof-label">Profissional</label>
            <div v-if="professionalsLoading" class="booking-flow__empty-pros">A carregar profissionais...</div>
            <div
              v-else-if="!professionals.length"
              class="booking-flow__empty-pros"
            >
              Nenhum profissional está disponível para este serviço.
            </div>
            <select
              v-else
              v-model="form.professionalId"
              class="booking-flow__prof-select"
            >
              <option :value="0" disabled>Selecione...</option>
              <option v-for="pro in professionals" :key="pro.id" :value="pro.id">
                {{ pro.name }} — {{ pro.specialty }}
              </option>
            </select>
          </div>

          <h2 class="booking-flow__section-title">Escolha o seu horário</h2>

          <BookingDateStrip v-model="form.date" :min-date="minDate" />

          <div class="booking-flow__slots">
            <p v-if="!form.professionalId" class="booking-flow__hint">
              Selecione um profissional para ver os horários.
            </p>
            <p v-else-if="slotsLoading" class="booking-flow__hint">A carregar horários...</p>
            <template v-else-if="form.date">
              <div v-for="period in ALL_PERIODS" :key="period" class="booking-flow__period">
                <h3>{{ TIME_PERIOD_LABELS[period] }}</h3>
                <p v-if="!groupedSlots[period].length" class="booking-flow__empty">
                  Não há horários disponíveis nesse período.
                </p>
                <div v-else class="booking-flow__times">
                  <button
                    v-for="slot in groupedSlots[period]"
                    :key="slot"
                    type="button"
                    class="booking-flow__time"
                    :class="{ 'is-selected': form.time === slot }"
                    @click="form.time = slot"
                  >
                    {{ slot }}
                  </button>
                </div>
              </div>
            </template>
          </div>

          <div class="booking-flow__actions">
            <button
              type="button"
              class="booking-flow__btn booking-flow__btn--primary"
              :disabled="!canProceedSchedule()"
              @click="goToDetails"
            >
              Continuar
            </button>
          </div>
        </section>

        <section v-else-if="step === 'details'" class="booking-flow__section">
          <h2 class="booking-flow__section-title">Os seus dados</h2>

          <div v-if="auth.isAuthenticated" class="booking-flow__auth-info">
            Autenticado como <strong>{{ auth.fullName }}</strong>.
          </div>

          <div v-else class="booking-flow__form">
            <label>
              Nome completo
              <input v-model="form.clientName" type="text" required />
            </label>
            <label>
              Email
              <input v-model="form.clientEmail" type="email" required />
            </label>
            <label>
              Telefone
              <input v-model="form.clientPhone" type="tel" />
            </label>
          </div>

          <label class="booking-flow__notes">
            Observações (opcional)
            <textarea v-model="form.notes" rows="3" maxlength="500" />
          </label>

          <div class="booking-flow__actions">
            <button type="button" class="booking-flow__btn" @click="step = 'schedule'">Voltar</button>
            <button
              type="button"
              class="booking-flow__btn booking-flow__btn--primary"
              :disabled="!canProceedDetails()"
              @click="goToConfirm"
            >
              Rever agendamento
            </button>
          </div>
        </section>

        <section v-else class="booking-flow__section">
          <h2 class="booking-flow__section-title">Confirmar agendamento</h2>
          <ul class="booking-flow__summary">
            <li><strong>Serviço:</strong> {{ selectedService?.name }}</li>
            <li><strong>Profissional:</strong> {{ selectedProfessional?.name }}</li>
            <li><strong>Data:</strong> {{ form.date }} às {{ form.time }}</li>
            <li>
              <strong>Cliente:</strong>
              {{ auth.isAuthenticated ? auth.fullName : form.clientName }}
            </li>
            <li v-if="form.notes"><strong>Observações:</strong> {{ form.notes }}</li>
          </ul>

          <div class="booking-flow__actions">
            <button type="button" class="booking-flow__btn" @click="step = 'details'">Voltar</button>
            <button
              type="button"
              class="booking-flow__btn booking-flow__btn--success"
              :disabled="submitting"
              @click="submitBooking"
            >
              {{ submitting ? 'A confirmar...' : 'Confirmar agendamento' }}
            </button>
          </div>
        </section>
      </template>
    </div>
  </div>
</template>

<style scoped>
.booking-flow {
  margin-top: 100px;
  padding: 2rem 0 4rem;
  background: #fff;
}

.booking-flow__wrapper {
  max-width: 720px;
  margin: 0 auto;
  padding: 0 1.25rem;
}

.booking-flow__back {
  display: inline-block;
  margin-bottom: 1.5rem;
  color: #ff9459;
  text-decoration: none;
  font-weight: 500;
  font-size: 0.9rem;
}

.booking-flow__loading {
  color: #888;
}

.booking-flow__service-card {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  padding: 1rem 1.15rem;
  margin-bottom: 1.5rem;
  border: 1px solid #e5e5e5;
  border-radius: 4px;
  background: #fff;
}

.booking-flow__label {
  display: block;
  font-size: 0.8rem;
  color: #888;
  margin-bottom: 0.2rem;
}

.booking-flow__title {
  font-size: 1.05rem;
  font-weight: 500;
  color: #333;
  margin: 0;
}

.booking-flow__price {
  font-size: 1rem;
  font-weight: 700;
  color: #333;
  white-space: nowrap;
}

.booking-flow__section-title {
  font-size: 1rem;
  font-weight: 600;
  color: #333;
  margin-bottom: 1rem;
}

.booking-flow__professionals {
  margin-bottom: 1.5rem;
}

.booking-flow__empty-pros {
  margin-top: 0.35rem;
  padding: 0.85rem 1rem;
  border: 1px dashed #e0e0e0;
  border-radius: 4px;
  color: #888;
  font-size: 0.92rem;
  background: #fafafa;
}

.booking-flow__prof-label {
  display: block;
  font-size: 0.85rem;
  color: #666;
  margin-bottom: 0.35rem;
}

.booking-flow__prof-select {
  width: 100%;
  padding: 0.6rem 0.85rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font: inherit;
}

.booking-flow__slots {
  margin-top: 1.5rem;
}

.booking-flow__hint {
  color: #888;
  font-size: 0.9rem;
}

.booking-flow__period {
  margin-bottom: 1.25rem;
}

.booking-flow__period h3 {
  font-size: 0.95rem;
  font-weight: 600;
  color: #333;
  margin-bottom: 0.5rem;
}

.booking-flow__empty {
  font-size: 0.88rem;
  color: #888;
}

.booking-flow__times {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.booking-flow__time {
  padding: 0.4rem 0.75rem;
  border-radius: 4px;
  border: 1px solid #ff9459;
  background: #fff;
  color: #ff9459;
  cursor: pointer;
  font-size: 0.88rem;
  min-width: 58px;
}

.booking-flow__time.is-selected {
  background: #ff9459;
  color: #fff;
}

.booking-flow__form {
  display: grid;
  gap: 1rem;
  margin-bottom: 1rem;
}

.booking-flow__form label,
.booking-flow__notes {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  font-size: 0.9rem;
  font-weight: 500;
}

.booking-flow__form input,
.booking-flow__notes textarea {
  padding: 0.65rem 0.85rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font: inherit;
}

.booking-flow__auth-info {
  padding: 0.85rem 1rem;
  background: #eef6ff;
  border-radius: 4px;
  margin-bottom: 1rem;
}

.booking-flow__summary {
  list-style: none;
  padding: 1rem 1.15rem;
  border: 1px solid #ececec;
  border-radius: 4px;
  background: #fafafa;
  margin-bottom: 1.5rem;
}

.booking-flow__summary li + li {
  margin-top: 0.5rem;
}

.booking-flow__actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  margin-top: 2rem;
}

.booking-flow__btn {
  padding: 0.65rem 1.25rem;
  border-radius: 4px;
  border: 1px solid #ddd;
  background: #fff;
  cursor: pointer;
  font-weight: 500;
}

.booking-flow__btn--primary {
  background: #ff9459;
  border-color: #ff9459;
  color: #fff;
}

.booking-flow__btn--success {
  background: #198754;
  border-color: #198754;
  color: #fff;
}

.booking-flow__btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { getErrorMessage } from '@/lib/api'
import { submitContactMessage } from '@/services/publicApi'
import { useUiStore } from '@/stores/ui'

const ui = useUiStore()
const submitting = ref(false)
const sent = ref(false)

const form = reactive({
  name: '',
  email: '',
  phone: '',
  subject: '',
  message: '',
})

async function submit() {
  submitting.value = true
  try {
    await submitContactMessage({
      name: form.name,
      email: form.email,
      phone: form.phone || undefined,
      subject: form.subject || undefined,
      message: form.message,
    })
    sent.value = true
    ui.success('Mensagem enviada com sucesso.')
    form.name = ''
    form.email = ''
    form.phone = ''
    form.subject = ''
    form.message = ''
  } catch (err) {
    ui.error(getErrorMessage(err, 'Não foi possível enviar a mensagem.'))
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="container py-5" style="margin-top: 100px !important">
    <div class="row justify-content-center">
      <div class="col-lg-8">
        <h1 class="display-6 fw-bold mb-2">Contacte-nos</h1>
        <p class="text-muted mb-4">
          Envie-nos a sua mensagem. Responderemos o mais breve possível.
        </p>

        <div v-if="sent" class="alert alert-success">
          Obrigado pelo seu contacto. Entraremos em contacto em breve.
        </div>

        <form class="row g-3" @submit.prevent="submit">
          <div class="col-md-6">
            <label class="form-label">Nome</label>
            <input v-model="form.name" type="text" class="form-control" required />
          </div>
          <div class="col-md-6">
            <label class="form-label">Email</label>
            <input v-model="form.email" type="email" class="form-control" required />
          </div>
          <div class="col-md-6">
            <label class="form-label">Telefone</label>
            <input v-model="form.phone" type="tel" class="form-control" />
          </div>
          <div class="col-md-6">
            <label class="form-label">Assunto</label>
            <input v-model="form.subject" type="text" class="form-control" />
          </div>
          <div class="col-12">
            <label class="form-label">Mensagem</label>
            <textarea v-model="form.message" class="form-control" rows="5" required maxlength="2000" />
          </div>
          <div class="col-12">
            <button type="submit" class="btn btn-primary" :disabled="submitting">
              {{ submitting ? 'A enviar...' : 'Enviar mensagem' }}
            </button>
          </div>
        </form>

        <div class="mt-5 p-4 bg-light rounded">
          <h5>Outros contactos</h5>
          <p class="mb-1">R. Vale das Flores 23, Alto de São João, 3030-486 Coimbra</p>
          <p class="mb-1">ewellinjordao@gmail.com</p>
          <p class="mb-0">(+351) 910 375 956</p>
        </div>
      </div>
    </div>
  </div>
</template>

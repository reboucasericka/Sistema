<template>
  <section class="academia py-5" style="margin-top: 80px">
    <div class="container">
      <div id="academyCarousel" class="carousel slide mb-5" data-bs-ride="carousel">
        <div class="carousel-inner">
          <div class="carousel-item active">
            <img src="/images/Academy/ew1.jpg" class="d-block w-100" alt="Ewellin ensinando 1" />
          </div>
          <div class="carousel-item">
            <img src="/images/Academy/ew2.jpg" class="d-block w-100" alt="Ewellin ensinando 2" />
          </div>
          <div class="carousel-item">
            <img src="/images/Academy/ew3.jpg" class="d-block w-100" alt="Ewellin ensinando 3" />
          </div>
        </div>
        <button class="carousel-control-prev" type="button" data-bs-target="#academyCarousel" data-bs-slide="prev">
          <span class="carousel-control-prev-icon" />
        </button>
        <button class="carousel-control-next" type="button" data-bs-target="#academyCarousel" data-bs-slide="next">
          <span class="carousel-control-next-icon" />
        </button>
      </div>

      <h2 class="academia-title text-center mb-4">Academia Ewellin Jordão</h2>
      <p class="text-center mb-5">
        Invista no seu futuro! Aprenda as técnicas mais avançadas de extensão de pestanas
        e torne-se uma profissional de sucesso com os nossos cursos exclusivos.
      </p>

      <div class="row">
        <div v-for="course in courses" :key="course.title" class="col-md-4 mb-4">
          <div class="card shadow-sm">
            <img :src="course.image" class="card-img-top" :alt="course.title" />
            <div class="card-body text-center">
              <h4>{{ course.title }}</h4>
              <p v-for="(line, i) in course.lines" :key="i">{{ line }}</p>
              <h5>{{ course.price }}</h5>
            </div>
          </div>
        </div>
      </div>

      <div class="formulario-academia mt-5">
        <h2 class="formulario-title text-center mb-4">Gostaria de saber mais informações?</h2>
        <p class="text-center mb-5">Preencha o formulário abaixo e entraremos em contato.</p>
        <form @submit.prevent>
          <input type="text" class="form-control" placeholder="Nome completo" required />
          <input type="email" class="form-control" placeholder="Email" required />
          <input type="text" class="form-control" placeholder="Telefone" />
          <input type="text" class="form-control" placeholder="Cidade" />
          <textarea class="form-control" rows="4" placeholder="Mensagem (opcional)" />
          <div class="text-center mt-4">
            <button type="submit" class="btn-enviar">Enviar Mensagem</button>
          </div>
        </form>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
const courses = [
  {
    title: 'Extensão Clássica',
    image: '/images/Academy/classic.jpg',
    price: '€250',
    lines: ['Curso completo para iniciantes.', 'O pilar. Foco em saúde das pestanas e isolamento perfeito', 'Kit Inicial Incluído.'],
  },
  {
    title: 'Volume Russo',
    image: '/images/Academy/russian.jpg',
    price: '€350',
    lines: ['Técnicas avançadas de volume.', 'Aumento da densidade e técnica de fanning.', 'Módulo de Marketing incluído.'],
  },
  {
    title: 'Mega Volume',
    image: '/images/Academy/mega.jpeg',
    price: '€450',
    lines: ['Formação intensiva para experts.', 'Foco em densidade, retenção e técnicas avançadas de styling.'],
  },
  {
    title: 'Lash Lifting & Tinting',
    image: '/images/Academy/ew4.jpg',
    price: '€250',
    lines: ['Curso completo para iniciantes.', 'Técnicas Naturais: curvar e pintar as pestanas naturais.'],
  },
  {
    title: 'Dermo Labial',
    image: '/images/Academy/ew5.jpg',
    price: '€650',
    lines: ['Especialização Facial.', 'Curso avançado de micropigmentação labial.'],
  },
  {
    title: 'Master Pack - Volume',
    image: '/images/Academy/ew6.jpg',
    price: '€750',
    lines: ['Volume Russo + Mega Volume.', 'Formação completa em todas as técnicas de volume.'],
  },
]
</script>

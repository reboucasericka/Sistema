<template>
  <section class="sobre-nos py-5 bg-light" style="margin-top: 80px">
    <div class="container">
      <div class="row align-items-center">
        <div class="col-md-6">
          <img src="/images/About/2.jpg" alt="Ewellin Jordão" class="img-fluid rounded shadow" />
        </div>
        <div class="col-md-6">
          <h2 class="mb-4">Sobre Nós</h2>
          <p>
            A <strong>Ewellin Jordão</strong> é referência em realce do olhar, trazendo
            técnicas inovadoras de extensão de pestanas e estética facial.
            Nosso compromisso é valorizar a beleza de cada cliente com
            profissionalismo, carinho e resultados únicos.
          </p>
          <p>Desde 2024, formamos profissionais e transformamos vidas através da estética.</p>
          <RouterLink to="/booking" class="btn btn-primary mt-3">Agende o seu horário</RouterLink>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>

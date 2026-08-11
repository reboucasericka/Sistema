<script setup lang="ts">
import { nextTick, onBeforeUnmount, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import $ from 'jquery'
import 'slick-carousel'
import { IMAGES } from '@/lib/images'

function initSlider() {
  const el = document.querySelector('.slider_home')
  if (!el) return
  const $slider = $(el)
  if ($slider.hasClass('slick-initialized')) return
  ;($slider as unknown as { slick: (opts: object) => void }).slick({
    dots: true,
    arrows: false,
    infinite: true,
    autoplay: true,
    autoplaySpeed: 4000,
    fade: true,
    cssEase: 'linear',
  })
}

function destroySlider() {
  const el = document.querySelector('.slider_home')
  if (!el) return
  const $slider = $(el)
  if ($slider.hasClass('slick-initialized')) {
    ;($slider as unknown as { slick: (method: string) => void }).slick('unslick')
  }
}

onMounted(async () => {
  await nextTick()
  initSlider()
})

onBeforeUnmount(() => {
  destroySlider()
})
</script>

<template>
  <div class="home">
    <section class="slider_home">
      <div class="item">
        <img :src="IMAGES.slider1" alt="Ewellin Jordão" />
        <div class="title_one animated fadeInDown">O olhar que marca</div>
        <div class="title_two animated fadeInRight">• Ewellin •</div>
        <div class="title_three animated fadeInUp">Portugal • DESDE 2021</div>
      </div>
      <div class="item">
        <img :src="IMAGES.slider2" alt="Ewellin Jordão" />
        <div class="title_one animated fadeInDown">Revolucionar</div>
        <div class="title_two animated fadeInRight">• O OLHAR •</div>
        <div class="title_three animated fadeInUp">visite-nos • numa das nossas lojas</div>
      </div>
    </section>

    <section class="about py-5">
      <div class="container text-center">
        <span class="about-subtitle">PORTUGAL • DESDE 2021</span>
        <h2 class="about-title">SOBRE NÓS</h2>
        <p class="about-text">
          A nossa missão é dar a todas as mulheres a possibilidade de terem o olhar mais marcante,
          adaptado às suas feições. <strong>Ewellin</strong>, o olhar que marca.
        </p>
        <RouterLink to="/about" class="about-link">SAIBA MAIS</RouterLink>
      </div>
    </section>

    <section class="showcase">
      <div class="box">
        <div class="item">
          <div class="border">
            <h3 class="title">Serviços</h3>
            <span class="subtitle">Extensões &amp; Lifting</span>
            <p class="text">Transforme o seu olhar com Volume Russo ou Clássico..</p>
            <div class="more"><RouterLink to="/services">saiba mais</RouterLink></div>
          </div>
        </div>
        <div class="item black">
          <div class="border">
            <h3 class="title">Estética Facial</h3>
            <span class="subtitle">Sobrancelhas &amp; Lábios</span>
            <p class="text">Design, Lamination e Micropigmentação para o seu rosto.</p>
            <div class="more"><RouterLink to="/services">saiba mais</RouterLink></div>
          </div>
        </div>
        <div class="item bg_full academy-bg">
          <div class="border">
            <h3 class="title">Ewellin Academy</h3>
            <span class="subtitle">Formação Profissional</span>
            <p class="text">Cursos Clássico, Volume e Master. Torne-se um expert!</p>
            <div class="more"><RouterLink to="/academy">VER CURSOS</RouterLink></div>
          </div>
        </div>
        <div class="item products-bg">
          <img :src="IMAGES.lapisLiftDraw" alt="Produtos Exclusivos" />
          <div class="border">
            <h3 class="title">Loja Online</h3>
            <span class="subtitle">Produtos Exclusivos</span>
            <p class="text">A sua linha de produtos de pestanas e sobrancelhas.</p>
            <div class="more"><RouterLink to="/products">COMPRAR AGORA</RouterLink></div>
          </div>
        </div>
        <div class="item products-bg">
          <img :src="IMAGES.lapisLiftDraw" alt="Produtos Exclusivos" />
          <div class="border">
            <h3 class="title">Loja Online</h3>
            <span class="subtitle">Produtos Exclusivos</span>
            <p class="text">A sua linha de produtos de pestanas e sobrancelhas.</p>
            <div class="more"><RouterLink to="/products">COMPRAR AGORA</RouterLink></div>
          </div>
        </div>
      </div>
    </section>

    <section class="best_sellers">
      <h2 class="section_title">Best Sellers</h2>
      <span class="section_subtitle">Ewellin</span>
      <div class="wrapper">
        <div class="box">
          <div class="item">
            <img :src="IMAGES.pd1" alt="Sérum de Cílios" />
            <p class="text">Sérum de Cílios</p>
          </div>
          <div class="item">
            <img :src="IMAGES.pd2" alt="Lift &amp; Draw" />
            <p class="text">Lift &amp; Draw</p>
          </div>
          <div class="item">
            <img :src="IMAGES.pd3" alt="Demaquilante" />
            <p class="text">Demaquilante</p>
          </div>
        </div>
      </div>
    </section>

    <section class="how_to_use">
      <h2 class="section_title">Como Usar</h2>
      <span class="section_subtitle">VÍDEOS • EWELLIN</span>
      <div class="wrapper">
        <div class="box">
          <div class="item">
            <a href="https://www.youtube.com/watch?v=g7ShGcZ-t80&t=36s" target="_blank" rel="noopener">
              <img :src="IMAGES.video1" alt="Demaquilante" />
              <p class="text">Demaquilante</p>
            </a>
          </div>
          <div class="item">
            <a href="https://www.youtube.com/shorts/zJxEz_Tmy28" target="_blank" rel="noopener">
              <img :src="IMAGES.video3" alt="Lift & Draw" />
              <p class="text">Lift &amp; Draw</p>
            </a>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.more a {
  color: inherit;
  text-decoration: none;
}

.about-link {
  text-decoration: none;
}
</style>

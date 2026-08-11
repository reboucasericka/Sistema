<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { IMAGES } from '@/lib/images'

const usefulLinks = [
  { to: '/', label: 'Home' },
  { to: '/services', label: 'Serviços' },
  { to: '/products', label: 'Produtos' },
  { to: '/academy', label: 'Academia' },
  { to: '/about', label: 'Sobre Nós' },
  { to: '/booking', label: 'Marcação Online' },
] as const

const specialties = [
  'Maquilhagem profissional',
  'Extensão de pestanas',
  'Design de sobrancelhas',
  'Lash lifting & facial',
] as const

const socialLinks = [
  {
    href: 'https://www.facebook.com/ewellinup',
    label: 'Facebook',
    icon: 'fab fa-facebook-f',
  },
  {
    href: 'https://www.instagram.com/ewellinup/',
    label: 'Instagram',
    icon: 'fab fa-instagram',
  },
] as const
</script>

<template>
  <footer class="site-footer">
    <div class="footer-main">
      <div class="wrapper">
        <div class="box">
          <div class="item footer-brand">
            <div class="logo">
              <RouterLink to="/">
                <img :src="IMAGES.aboutFooter" alt="Ewellin Jordão" />
              </RouterLink>
              <p class="footer-tagline">O olhar que marca.</p>
            </div>
            <p class="footer-address">
              R. Vale das Flores 23, Alto de São João<br />
              3030-486 Coimbra — Portugal
            </p>
            <p>
              <a href="mailto:ewellinjordao@gmail.com">ewellinjordao@gmail.com</a>
            </p>
            <p>
              <a href="tel:+351910375956">(+351) 910 375 956</a>
            </p>
            <div class="social">
              <a
                v-for="link in socialLinks"
                :key="link.label"
                :href="link.href"
                :aria-label="link.label"
                target="_blank"
                rel="noopener noreferrer"
              >
                <i :class="link.icon" />
              </a>
            </div>
          </div>

          <div class="item">
            <h2>Links Úteis</h2>
            <ul>
              <li v-for="link in usefulLinks" :key="link.to">
                <RouterLink :to="link.to">{{ link.label }}</RouterLink>
              </li>
            </ul>
          </div>

          <div class="item footer-experience">
            <h2>Experiências</h2>
            <ul class="footer-specialties">
              <li v-for="item in specialties" :key="item">{{ item }}</li>
            </ul>
            <p class="footer-hours">
              <strong>Horário</strong><br />
              Seg — Sáb · 10h — 19h
            </p>
            <RouterLink to="/booking" class="footer-cta">Marcar agora</RouterLink>
          </div>
        </div>
      </div>
    </div>
    <div class="footer-bottom">
      <div class="wrapper">
        <p>© {{ new Date().getFullYear() }} Ewellin Jordão Beauty · Coimbra, Portugal</p>
      </div>
    </div>
  </footer>
</template>

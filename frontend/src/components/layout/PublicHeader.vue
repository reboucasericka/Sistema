<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { IMAGES } from '@/lib/images'

const route = useRoute()
const drawerOpen = ref(false)

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

function closeDrawer() {
  drawerOpen.value = false
  document.body.style.overflow = 'auto'
}

function toggleDrawer() {
  drawerOpen.value = !drawerOpen.value
  document.body.style.overflow = drawerOpen.value ? 'hidden' : 'auto'
}

function isActive(path: string) {
  if (path === '/') {
    return route.path === '/' ? 'active' : ''
  }
  return route.path === path || route.path.startsWith(`${path}/`) ? 'active' : ''
}

function isAboutSectionActive() {
  return isActive('/about') || isActive('/academy') ? 'active' : ''
}

onMounted(() => {
  document.body.classList.add('legacy-site')
})

onUnmounted(() => {
  document.body.classList.remove('legacy-site')
  document.body.style.overflow = 'auto'
})
</script>

<template>
  <header>
    <div class="wrapper">
      <div class="logo">
        <RouterLink to="/" @click="closeDrawer">
          <img :src="IMAGES.logo" alt="Ewellin Jordão" />
        </RouterLink>
      </div>

      <nav class="nav-desktop" aria-label="Menu principal">
        <ul>
          <li><RouterLink to="/" :class="isActive('/')">Início</RouterLink></li>
          <li class="dropdown">
            <button type="button" class="dropbtn" :class="isAboutSectionActive()">Sobre</button>
            <div class="dropdown-content">
              <RouterLink to="/about" :class="isActive('/about')">Sobre Nós</RouterLink>
              <RouterLink to="/academy" :class="isActive('/academy')">Academia</RouterLink>
            </div>
          </li>
          <li>
            <RouterLink to="/booking" :class="isActive('/booking')">Agendamento Online</RouterLink>
          </li>
          <li>
            <RouterLink to="/prices" :class="isActive('/prices')">Tabela de preços</RouterLink>
          </li>
          <li><RouterLink to="/services" :class="isActive('/services')">Serviços</RouterLink></li>
          <li>
            <RouterLink to="/professionals" :class="isActive('/professionals')"
              >Profissionais</RouterLink
            >
          </li>
          <li><RouterLink to="/products" :class="isActive('/products')">Produtos</RouterLink></li>
          <li><RouterLink to="/contact" :class="isActive('/contact')">Contacto</RouterLink></li>
          <li>
            <RouterLink to="/recruitment" :class="isActive('/recruitment')"
              >Recrutamento</RouterLink
            >
          </li>
          <li>
            <RouterLink to="/login" :class="isActive('/login')">Login</RouterLink>
          </li>
        </ul>

        <div class="nav-compact">
          <RouterLink to="/products" :class="isActive('/products')">Produtos</RouterLink>
          <RouterLink to="/contact" :class="isActive('/contact')">Contacto</RouterLink>
        </div>

        <button
          type="button"
          class="menu-hamburguer"
          :class="{ active: drawerOpen }"
          aria-label="Abrir informações de contacto"
          :aria-expanded="drawerOpen"
          @click="toggleDrawer"
        >
          <div class="bar1" />
          <div class="bar2" />
          <div class="bar3" />
        </button>
      </nav>
    </div>
  </header>

  <Teleport to="body">
    <div
      class="nav-drawer-backdrop"
      :class="{ active: drawerOpen }"
      aria-hidden="true"
      @click="closeDrawer"
    />

    <aside
      class="nav-drawer"
      :class="{ active: drawerOpen }"
      aria-label="Informações de contacto"
      :aria-hidden="!drawerOpen"
    >
      <button type="button" class="nav-drawer-close" aria-label="Fechar" @click="closeDrawer">
        <i class="fas fa-times" />
      </button>

      <div class="nav-drawer-logo">
        <img :src="IMAGES.logoBar" alt="Ewellin Jordão" />
      </div>

      <hr class="nav-drawer-divider" />

      <address class="nav-drawer-contact">
        <p>R. Vale das Flores 23, Alto de São João</p>
        <p>3030-486 Coimbra — Portugal</p>
        <p>
          <a href="mailto:ewellinjordao@gmail.com">ewellinjordao@gmail.com</a>
        </p>
        <p>
          <a href="tel:+351910375956">(+351) 910 375 956</a>
        </p>
      </address>

      <hr class="nav-drawer-divider" />

      <div class="nav-drawer-social">
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

      <nav class="nav-drawer-links" aria-label="Navegação rápida">
        <RouterLink to="/" @click="closeDrawer">Início</RouterLink>
        <RouterLink to="/about" @click="closeDrawer">Sobre Nós</RouterLink>
        <RouterLink to="/academy" @click="closeDrawer">Academia</RouterLink>
        <RouterLink to="/booking" @click="closeDrawer">Agendamento Online</RouterLink>
        <RouterLink to="/services" @click="closeDrawer">Serviços</RouterLink>
        <RouterLink to="/products" @click="closeDrawer">Produtos</RouterLink>
        <RouterLink to="/login" @click="closeDrawer">Login</RouterLink>
      </nav>
    </aside>
  </Teleport>
</template>

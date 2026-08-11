import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      component: () => import('@/layouts/PublicLayout.vue'),
      meta: { public: true },
      children: [
        {
          path: '',
          name: 'home',
          component: () => import('@/views/public/HomeView.vue'),
        },
        {
          path: 'services',
          name: 'public-services',
          component: () => import('@/views/public/PublicServicesView.vue'),
        },
        {
          path: 'products',
          name: 'public-products',
          component: () => import('@/views/public/PublicProductsView.vue'),
        },
        {
          path: 'booking',
          name: 'public-booking',
          component: () => import('@/views/public/PublicBookingView.vue'),
        },
        {
          path: 'booking/flow',
          name: 'booking-flow',
          component: () => import('@/views/public/BookingFlowView.vue'),
        },
        {
          path: 'professionals',
          name: 'public-professionals',
          component: () => import('@/views/public/PublicProfessionalsView.vue'),
        },
        {
          path: 'professionals/:id',
          name: 'public-professional-detail',
          component: () => import('@/views/public/PublicProfessionalDetailView.vue'),
        },
        {
          path: 'contact',
          name: 'public-contact',
          component: () => import('@/views/public/PublicContactView.vue'),
        },
        {
          path: 'prices',
          name: 'public-prices',
          component: () => import('@/views/public/PublicPricesView.vue'),
        },
        {
          path: 'about',
          name: 'public-about',
          component: () => import('@/views/public/PublicAboutView.vue'),
        },
        {
          path: 'academy',
          name: 'public-academy',
          component: () => import('@/views/public/PublicAcademyView.vue'),
        },
        {
          path: 'recruitment',
          name: 'public-recruitment',
          component: () => import('@/views/public/PublicRecruitmentView.vue'),
        },
        {
          path: 'services/:id',
          name: 'public-service-detail',
          component: () => import('@/views/public/ServiceDetailView.vue'),
        },
      ],
    },
    {
      path: '/login',
      component: () => import('@/layouts/AuthLayout.vue'),
      meta: { guest: true, public: true },
      children: [
        {
          path: '',
          name: 'login',
          component: () => import('@/views/LoginView.vue'),
        },
      ],
    },
    {
      path: '/register',
      component: () => import('@/layouts/AuthLayout.vue'),
      meta: { guest: true, public: true },
      children: [
        {
          path: '',
          name: 'register',
          component: () => import('@/views/RegisterView.vue'),
        },
      ],
    },
    {
      path: '/panel',
      component: () => import('@/layouts/PanelLayout.vue'),
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          name: 'panel-home',
          component: () => import('@/views/PanelView.vue'),
          meta: { roles: ['admin', 'professional'] },
        },
        {
          path: 'admin',
          component: () => import('@/layouts/AdminLayout.vue'),
          meta: { roles: ['admin', 'professional'] },
          children: [
            {
              path: '',
              redirect: { name: 'admin-dashboard' },
            },
            {
              path: 'dashboard',
              name: 'admin-dashboard',
              component: () => import('@/views/admin/DashboardView.vue'),
            },
            {
              path: 'professionals',
              name: 'admin-professionals',
              component: () => import('@/views/admin/ProfessionalsView.vue'),
              meta: { roles: ['admin'] },
            },
            {
              path: 'clients',
              name: 'admin-clients',
              component: () => import('@/views/admin/ClientsView.vue'),
              meta: { roles: ['admin', 'professional'] },
            },
            {
              path: 'categories',
              name: 'admin-categories',
              component: () => import('@/views/admin/CategoriesView.vue'),
              meta: { requiresAuth: true, roles: ['admin'] },
            },
            {
              path: 'services',
              name: 'admin-services',
              component: () => import('@/views/admin/ServicesView.vue'),
              meta: { roles: ['admin'] },
            },
            {
              path: 'appointments',
              name: 'admin-appointments',
              component: () => import('@/views/admin/AppointmentsView.vue'),
            },
            {
              path: 'schedules',
              name: 'admin-professional-schedules',
              component: () => import('@/views/admin/ProfessionalSchedulesView.vue'),
              meta: { roles: ['admin'] },
            },
            {
              path: 'products',
              name: 'admin-products',
              component: () => import('@/views/admin/ProductsView.vue'),
              meta: { roles: ['admin', 'professional'] },
            },
            {
              path: 'stock',
              name: 'admin-stock',
              component: () => import('@/views/admin/StockView.vue'),
              meta: { roles: ['admin', 'professional'] },
            },
            {
              path: 'sales',
              name: 'admin-sales',
              component: () => import('@/views/admin/SalesView.vue'),
              meta: { roles: ['admin', 'professional'] },
            },
            {
              path: 'cash',
              name: 'admin-cash',
              component: () => import('@/views/admin/CashView.vue'),
              meta: { roles: ['admin', 'professional'] },
            },
            {
              path: 'messages',
              name: 'admin-messages',
              component: () => import('@/views/admin/ContactMessagesView.vue'),
              meta: { roles: ['admin'] },
            },
          ],
        },
        {
          path: 'client',
          component: () => import('@/layouts/ClientLayout.vue'),
          meta: { roles: ['client'] },
          children: [
            {
              path: '',
              redirect: { name: 'client-dashboard' },
            },
            {
              path: 'dashboard',
              name: 'client-dashboard',
              component: () => import('@/views/client/DashboardView.vue'),
            },
            {
              path: 'appointments',
              name: 'client-appointments',
              component: () => import('@/views/client/AppointmentsView.vue'),
            },
          ],
        },
      ],
    },
    {
      path: '/admin',
      redirect: '/panel/admin/dashboard',
    },
    {
      path: '/admin/:pathMatch(.*)*',
      redirect: (to) => `/panel/admin/${to.params.pathMatch}`,
    },
    {
      path: '/client',
      redirect: '/panel/client/dashboard',
    },
    {
      path: '/client/:pathMatch(.*)*',
      redirect: (to) => `/panel/client/${to.params.pathMatch}`,
    },
    {
      path: '/:pathMatch(.*)*',
      component: () => import('@/layouts/PublicLayout.vue'),
      meta: { public: true },
      children: [
        {
          path: '',
          name: 'not-found',
          component: () => import('@/views/public/NotFoundView.vue'),
        },
      ],
    },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.guest && auth.isAuthenticated) {
    return auth.homeRouteForRole(auth.role)
  }

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  const allowedRoles =
    [...to.matched]
      .reverse()
      .map((record) => record.meta.roles as string[] | undefined)
      .find((roles) => Array.isArray(roles) && roles.length > 0) ?? []

  if (allowedRoles.length && auth.role && !allowedRoles.includes(auth.role)) {
    return auth.homeRouteForRole(auth.role)
  }

  return true
})

export default router

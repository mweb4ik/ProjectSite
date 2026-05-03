
import { createRouter, createWebHistory } from 'vue-router'
import LoginPage from '@/auth/LoginPage.vue'
import HomePage from '@/views/HomePage.vue'
import ErrorPage from '@/views/ErrorPage.vue'
import ResetPasswordPage from '@/auth/ResetPasswordPage.vue'
import ForgotPasswordPage from '@/auth/ForgotPasswordPage.vue'
import ProfilePage from '@/views/ProfilePage.vue'
import AdminPage from '@/views/AdminPage.vue'
import ComponentsPage from '@/components/ComponentsPage.vue'
import BiosPage from '@/components/BiosPage.vue'
import ComponentsDetailsPage from '@/components/ComponentsDetailsPage.vue'
import BuilderPage from '@/components/BuilderPage.vue'


const routes = [
  { path: '/', component: LoginPage },
  { path: '/home', component: HomePage },
  { path: '/component/:type', component: ComponentsPage }, 
  { path: '/components-details/:id', name: 'components-details', component: ComponentsDetailsPage },
  { path: '/builder', name: 'builder', component: BuilderPage }, 
  { path: '/bios', name: 'bios', component: BiosPage },
  { path: '/profile', name: 'profile', component: ProfilePage },
  { path: '/admin', name: 'admin', component: AdminPage },
  { path: '/forgot-password', name: 'forgot-password', component: ForgotPasswordPage },
  { path: '/reset-password', name: 'reset-password', component: ResetPasswordPage },
  { path: '/:pathMatch(.*)*', name: 'NotFound', component: ErrorPage }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to) => {
  const token = localStorage.getItem('token')
  const user = JSON.parse(localStorage.getItem('user') || 'null')

  if (!token && to.path !== '/') {
    return '/'
  }
  if (user?.Role === 'guest') {
    if (to.path === '/profile' || to.path === '/admin') {
      return '/home'
    }
  }
  if (to.path === '/admin' && user?.Role !== 'admin') {
    return '/home'
  }
  return true
})

export default router
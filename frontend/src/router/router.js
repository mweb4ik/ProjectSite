import { createRouter, createWebHistory } from 'vue-router'
import LoginPage from '@/auth/LoginPage.vue'
import HomePage from '@/views/HomePage.vue'
import ErrorPage from '@/views/ErrorPage.vue'
import ResetPasswordPage from '@/auth/ResetPasswordPage.vue'
import ForgotPasswordPage from '@/auth/ForgotPasswordPage.vue'
import ProfilePage from '@/views/ProfilePage.vue'
import AdminPage from '@/views/AdminPage.vue'
import ComponentsPage  from '@/components/ComponentsPage.vue'
import BiosPage  from '@/components/BiosPage.vue'
import ComponentsDetailsPage from '@/components/ComponentsDetailsPage'  
const routes = [
  { path: '/', component: LoginPage },
  { path: '/home', component: HomePage },
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: ErrorPage
  },
  {  name: 'forgot-password',
    path: '/forgot-password',
    component: ForgotPasswordPage
  },
  { name: 'reset-password',
    path: '/reset-password',
    component: ResetPasswordPage
  },
  { name: 'profile',
    path: '/profile',
    component: ProfilePage
  },
  {
    path: '/admin',
    name: 'admin',
    component: AdminPage
  },
  {  name: 'bios',
    path: '/bios',
    component: BiosPage
  },
  {  name: 'components-details',
    path: '/components-details',
    component: ComponentsDetailsPage
  },
  { path: '/component/:type', component: ComponentsPage }
]
const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to) => {
  const token = localStorage.getItem('token')
  const user = JSON.parse(localStorage.getItem('user'))

  if (!token && to.path !== '/') {
    return '/'
  }

  // роли
  if (to.path === '/profile' && user?.Role === 'guest') {
    return '/home'
  }

  if (to.path === '/admin' && user?.Role !== 'admin') {
    return '/home'
  }

  return true
})
function isAdmin(user) {
  return user?.Role === 'admin'
}

function isUser(user) {
  return user?.Role === 'user'
}
export default router
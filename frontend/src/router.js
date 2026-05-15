import { createRouter, createWebHistory } from 'vue-router'
import LandingPage from '@/views/LandingPage.vue'
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
import LabPage from '@/components/LabPage.vue'
import QuizPage from '@/views/QuizPage.vue'
import QuizResultPage from '@/views/QuizResultPage.vue'


const routes = [

  { path: '/', name: 'landing', component: () => import('@/views/LandingPage.vue') },
  
  { path: '/auth', name: 'auth', component: () => import('@/auth/LoginPage.vue') },
  { path: '/auth/login', redirect: '/auth' },
  { path: '/auth/register', redirect: '/auth' },
  
 
  { path: '/home', component: HomePage },
  { path: '/component/:type', component: ComponentsPage },
  { path: '/components-details/:id', name: 'components-details', component: ComponentsDetailsPage },
  { path: '/builder', name: 'builder', component: BuilderPage },
  { path: '/bios', name: 'bios', component: BiosPage },
  { path: '/profile', name: 'profile', component: ProfilePage },
  { path: '/admin', name: 'admin', component: AdminPage },
  { path: '/forgot-password', name: 'forgot-password', component: ForgotPasswordPage },
  { path: '/reset-password', name: 'reset-password', component: ResetPasswordPage },
  { path: '/quiz', name: 'quiz', component: QuizPage },
  { path: '/quiz-result', name: 'quiz-result', component: QuizResultPage },
  { path: '/lab', name: 'lab', component: LabPage },
  
  // 404 — должен быть ПОСЛЕДНИМ
  { path: '/:pathMatch(.*)*', name: 'NotFound', component: ErrorPage }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL), 
  routes
})

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token');
  const userStr = localStorage.getItem('user');
  let user = null;
  
  try {
    user = userStr ? JSON.parse(userStr) : null;
  } catch {
    user = null;
  }

  const publicPages = ['/', '/forgot-password', '/reset-password','/auth'];

  if (to.name === 'NotFound') {
    return next();
  }

  if (publicPages.includes(to.path)) {
    return next();
  }

  if (!token && user?.Role !== 'guest') {
    return next('/');
  }

  if (user?.Role === 'guest' && (to.path === '/profile' || to.path === '/admin')) {
    return next('/home');
  }

  if (to.path === '/admin' && user?.Role !== 'admin') {
    return next('/home');
  }

  next();
});

export default router;
import { createRouter, createWebHistory } from 'vue-router'
import LoginPage from './views/LoginPage.vue'
import HomePage from './views/HomePage.vue'

const routes = [
  { path: '/', component: LoginPage },
  { path: '/home', component: HomePage }
]

export default createRouter({
  history: createWebHistory(),
  routes
})

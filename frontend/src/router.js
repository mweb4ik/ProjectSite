import { createRouter, createWebHistory } from 'vue-router'
import LoginPage from './views/LoginPage.vue'
import HomePage from './views/HomePage.vue'
import VideocardPage from './views/VideocardPage.vue'
import ProcessorPage from './views/ProcessorPage.vue'
import MotherboardPage from './views/MotherboardPage.Vue'
import CoolingPage from './views/CoolingPage.vue'
import RamPage from './views/RamPage.vue'
import StoragePage from './views/StoragePage.vue'
import ErrorPage from './views/ErrorPage.vue'
const routes = [
  { path: '/', component: LoginPage },
  { path: '/home', component: HomePage },

  { path: '/videocard',
    name: 'videocard',
    component: VideocardPage },
  { path: '/processor',
    name: 'proccessor' ,
    component: ProcessorPage },
  { path: '/motherboard',
    name: 'motherboard',
    component: MotherboardPage },
  { path: '/cooling',
    name: 'cooling',
    component: CoolingPage },
  { path: '/ram',
    name: 'ram',
    component: RamPage },
  { path: '/storage',
    name: 'storage',
    component: StoragePage },
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: ErrorPage
  }
]
export default createRouter({
  history: createWebHistory(),
  routes
})

import { createRouter, createWebHistory } from 'vue-router'
 import App from '../App.vue'
 import UserLogin from '../auth/UserLogin.vue'

 const routes = [
{ path: '/', component: App },
{ path: '/register', component: UserLogin },
{ path: '/login', component: UserLogin } 
]
const router = createRouter({
history: createWebHistory(),
routes
})
export default router
import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import  api  from '@/api'
import './assets/styles/components/buttons.css'
import './assets/styles/base/main.css'
import './assets/styles/home.css'
import './assets/styles/pages/ErrorPage.css'
import './assets/styles/pages/ProfilePage.css'
import './assets/styles/pages/ResetPasswordPage.css'
import './assets/styles/pages/ForgotPasswordPage.css'
import './assets/styles/pages/ComponentsPage.css'
import './assets/styles/pages/ComponentsDetailsPage.css'
import './assets/styles/pages/BuilderPage.css'
createApp(App).use(router).mount('#app')
setInterval(() => {
  fetch("https://projectsite-backend.onrender.com/health")
    .catch(() => {});
}, 5 * 60 * 1000);
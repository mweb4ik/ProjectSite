import { createApp } from 'vue'
import App from './App.vue'
import router from './router/router'
import  api  from '@/api'

import '@/assets/styles/base/main.css'

import '@/assets/styles/components/buttons.css'
import '@/assets/styles/components/forms.css'
import '@/assets/styles/components/header.css'
import '@/assets/styles/components/skeleton.css'

import '@/assets/styles/pages/home.css'
import '@/assets/styles/pages/profile.css'
import '@/assets/styles/pages/error.css'
import '@/assets/styles/pages/auth.css'
import '@/assets/styles/pages/components.css'
import '@/assets/styles/pages/ForgotPasswordPage.css'
import '@/assets/styles/pages/ResetPasswordPage.css'
createApp(App).use(router).mount('#app')
setInterval(() => {
  fetch("https://projectsite-backend.onrender.com/health")
    .catch(() => {});
}, 5 * 60 * 1000);
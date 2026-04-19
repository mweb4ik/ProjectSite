import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import './assets/styles/buttons.css'
import './assets/styles/main.css'
import './assets/styles/home.css'
import './assets/styles/ErrorPage.css'
import './assets/styles/ProfilePage.css'
import './assets/styles/ResetPasswordPage.css'
import './assets/styles/ForgotPasswordPage.css'
import './assets/styles/ComponentsPage.css'
createApp(App).use(router).mount('#app')
setInterval(() => {
  fetch("https://projectsite-backend.onrender.com/health")
    .catch(() => {});
}, 5 * 60 * 1000);
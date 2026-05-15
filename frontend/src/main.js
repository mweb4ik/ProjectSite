import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import  api  from '@/api'
import './assets/styles/components/buttons.css'
import './assets/styles/base/main.css'
import './assets/styles/components/skeleton.css'
import './assets/styles/components/header.css'
import './assets/styles/pages/home.css'
import './assets/styles/responsive.css'

// Основная точка входа фронтенда: подключаем роутер и монтируем приложение в корневой контейнер.
createApp(App).use(router).mount('#app')

<template>
  <div class="error-container">
    <div class="error-box">
      <h1 class="error-code">404</h1>
      <p class="error-title">Страница не найдена</p>
      <p class="error-subtitle">
        Похоже, вы забрели не туда...
      </p>

      <div class="emoji">💻⚠️</div>

      <button class="btn-home" @click="goHome">
        Вернуться на главную
      </button>
    </div>
  </div>
</template>

<script>
import { useRouter } from 'vue-router'
import { onMounted, onUnmounted } from 'vue'
import '@/assets/styles/pages/ErrorPage.css'

export default {
  name: 'ErrorPage',
  setup() {
    const router = useRouter()
    let redirectTimer = null 

    const goHome = () => {

      if (redirectTimer) {
        clearTimeout(redirectTimer)
        redirectTimer = null
      }
      router.push('/')
    }

    onMounted(() => {
      redirectTimer = setTimeout(() => {
        router.replace('/') 
      }, 5000)
    })

    onUnmounted(() => {
      if (redirectTimer) {
        clearTimeout(redirectTimer)
        redirectTimer = null
      }
    })

    return { goHome }
  }
}
</script>
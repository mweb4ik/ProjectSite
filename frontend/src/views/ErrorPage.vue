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

  export default {
    name: 'ErrorPage',
    setup() {
      const router = useRouter()

      const goHome = () => {
        router.push('/')
      }
      setTimeout(() => {
    router.push('/')
  }, 5000)
      return { goHome }
    }
  }
  </script>

  
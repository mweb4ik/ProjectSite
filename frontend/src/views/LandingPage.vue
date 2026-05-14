<template>
  <div class="landing-page">
    <div class="landing-content">
      <!-- Логотип / Заголовок -->
      <div class="brand">
        <h1 class="brand-title">🖥️ PC Components</h1>
        <p class="brand-subtitle">Познай внутреннее устройство компьютера</p>
      </div>

      <!-- Герой-секция -->
      <div class="hero">
        <div class="hero-visual">
          <div class="pc-illustration">
            <span class="icon cpu">🧠</span>
            <span class="icon gpu">🎮</span>
            <span class="icon ram">💾</span>
            <span class="icon mb">🔌</span>
          </div>
        </div>

        <div class="hero-text">
          <h2>Собирай, изучай, тестируй</h2>
          <p>
            Интерактивный конфигуратор ПК, квизы по железу и симулятор BIOS — 
            всё в одном месте для начинающих и энтузиастов.
          </p>
        </div>
      </div>

      <!-- Кнопка начала -->
      <div class="cta-section">
        <button class="btn-start" @click="goToAuth">
          <span class="btn-text">Начать</span>
          <span class="btn-arrow">→</span>
        </button>
        <p class="hint">Бесплатно • Без регистрации для гостей</p>
      </div>

      <!-- Фичи -->
      <div class="features">
        <div class="feature">
          <span class="feature-icon">🔧</span>
          <h3>Конфигуратор</h3>
          <p>Подбирай совместимые компоненты</p>
        </div>
        <div class="feature">
          <span class="feature-icon">🧠</span>
          <h3>Квизы</h3>
          <p>Проверяй знания о железе</p>
        </div>
        <div class="feature">
          <span class="feature-icon">⚙️</span>
          <h3>BIOS Simulator</h3>
          <p>Изучай настройки в безопасной среде</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { useRouter } from 'vue-router'
import '@/assets/styles/pages/LandingPage.css'
export default {
  name: 'LandingPage',
  setup() {
    const router = useRouter()

   const goToAuth = () => {
  console.log('🖱️ Клик по "Начать"')
  router.push('/auth')
}

    return { goToAuth }
  }
}
</script>
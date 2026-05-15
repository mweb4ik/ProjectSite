<template>
  <div class="result-page">
    <AppHeader />

    <main class="result-container">
      <div class="result-card">

        <div class="icon">
          {{ resultPercent >= 70 ? '🏆' : resultPercent >= 40 ? '🔥' : '💻' }}
        </div>

        <h1>Результат квиза</h1>

        <div class="score-block">
          <div class="score-circle">
            {{ resultPercent }}%
          </div>

          <div class="score-info">
            <h2>{{ result.Score }} / {{ result.TotalQuestions }}</h2>
            <p>{{ resultText }}</p>
          </div>
        </div>

        <div class="stats">
          <div class="stat-item">
            <span>📅 Дата</span>
            <strong>{{ formatDate(result.CompletedAt) }}</strong>
          </div>

          <div class="stat-item">
            <span>🧠 Правильных ответов</span>
            <strong>{{ result.Score }}</strong>
          </div>

          <div class="stat-item">
            <span>📊 Всего вопросов</span>
            <strong>{{ result.TotalQuestions }}</strong>
          </div>
        </div>

        <div class="actions">
          <button class="btn-primary" @click="$router.push('/quiz')">
            Пройти снова
          </button>

          <button class="btn-secondary" @click="$router.push('/profile')">
            В профиль
          </button>
        </div>

      </div>
    </main>
  </div>
</template>

<script>
import AppHeader from '@/components/AppHeader.vue'
import '@/assets/styles/pages/QuizResultPage.css'
export default {
  name: 'QuizResultPage',

  components: {
    AppHeader
  },

  data() {
    return {
      result: {
        Score: 0,
        TotalQuestions: 0,
        CompletedAt: null
      }
    }
  },

  computed: {
    resultPercent() {
      if (!this.result.TotalQuestions) return 0

      return Math.round(
        (this.result.Score / this.result.TotalQuestions) * 100
      )
    },

    resultText() {
      if (this.resultPercent >= 80)
        return 'Отличный результат!'

      if (this.resultPercent >= 50)
        return 'Хорошие знания железа'

      return 'Есть куда расти 🚀'
    }
  },

  mounted() {
    this.result =
      history.state ||
      JSON.parse(localStorage.getItem('lastQuizResult')) ||
      this.result
  },

  methods: {
    formatDate(date) {
      if (!date) return 'Сейчас'

      return new Date(date).toLocaleString('ru-RU')
    }
  }
}
</script>


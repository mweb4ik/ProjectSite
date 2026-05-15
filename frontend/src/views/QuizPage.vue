<template>
  <div class="quiz-page">
    <AppHeader />

    <main class="quiz-container">
      <div class="quiz-header">
        <h1>🧠 PC Quiz Challenge</h1>

        <div class="progress-wrapper" v-if="questions.length">
          <div class="progress-info">
            <span>Вопрос {{ index + 1 }} / {{ questions.length }}</span>
            <span>{{ progressPercent }}%</span>
          </div>

          <div class="progress-bar">
            <div
              class="progress-fill"
              :style="{ width: progressPercent + '%' }"
            ></div>
          </div>
        </div>
      </div>

      <!-- loading -->
      <div v-if="loading" class="loading-card">
        <div class="loader"></div>
        <p>Загрузка вопросов...</p>
      </div>

      <!-- quiz -->
      <div v-else-if="currentQuestion" class="quiz-card">
        <div class="question-number">{{ index + 1 }}</div>

        <div class="difficulty" :class="currentQuestion.Difficulty">
          {{ difficultyText(currentQuestion.Difficulty) }}
        </div>

        <h2 class="question">
          {{ currentQuestion.Question }}
        </h2>

        <div class="options">
          <button
            v-for="(opt, i) in currentQuestion.Options"
            :key="i"
            class="option-btn"
            :class="{ active: answers[currentQuestion.Id] === i }"
            @click="selectAnswer(i)"
          >
            <span class="option-index">
              {{ String.fromCharCode(65 + i) }}
            </span>
            <span>{{ opt }}</span>
          </button>
        </div>

        <div class="actions">
          <button
            class="btn-next"
            :disabled="answers[currentQuestion.Id] === undefined"
            @click="next"
          >
            {{ isLast ? 'Завершить квиз 🚀' : 'Следующий вопрос →' }}
          </button>
        </div>
      </div>

      <!-- finish -->
      <div v-else class="finish-card">
        <h2>🎉 Квиз завершён</h2>
        <p>Все ответы сохранены. Отправить результат?</p>
        <button class="btn-submit" @click="submit">
          Показать результат
        </button>
      </div>
    </main>
  </div>
</template>

<script>
import AppHeader from '@/components/AppHeader.vue'
import api from '@/api'
import '@/assets/styles/pages/QuizPage.css'

export default {
  name: 'QuizPage',
  components: { AppHeader },
  data() {
    return {
      questions: [],
      index: 0,
      answers: {},
      loading: true
    }
  },
  computed: {
    currentQuestion() {
      return this.questions[this.index]
    },
    isLast() {
      return this.index === this.questions.length - 1
    },
    progressPercent() {
      if (!this.questions.length) return 0
      return Math.round((this.index / this.questions.length) * 100)
    }
  },
  async mounted() {
    try {
      const res = await api.get('/quiz')
      this.questions = this.shuffleArray(res.data)
    } catch (e) {
      console.error('Ошибка загрузки квиза', e)
    } finally {
      this.loading = false
    }
  },
  methods: {
    shuffleArray(array) {
      const shuffled = [...array]
      for (let i = shuffled.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1))
        ;[shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]]
      }
      return shuffled
    },
    difficultyText(diff) {
      const map = {
        easy: '🟢 Легко',
        medium: '🟡 Средне',
        hard: '🔴 Сложно'
      }
      return map[diff] || '⚪ Вопрос'
    },
    selectAnswer(i) {
      this.answers[this.currentQuestion.Id] = i
    },
    next() {
      if (this.index < this.questions.length - 1) {
        this.index++
      }
    },
    async submit() {
      try {
        const res = await api.post('/quiz/submit', this.answers)
        localStorage.setItem('lastQuizResult', JSON.stringify(res.data))
        this.$router.push({
          path: '/quiz-result',
          state: res.data
        })
      } catch (e) {
        console.error(e)
        alert('Ошибка отправки результатов')
      }
    }
  }
}
</script>
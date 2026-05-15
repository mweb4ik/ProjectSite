<template>
  <div class="profile-page">
    <AppHeader :user="user" @logout="logout" />

    <main class="profile-content">
      <!-- HEADER -->
      <section class="hero-card">
        <div class="avatar">
          {{ user.Username?.charAt(0)?.toUpperCase() || 'U' }}
        </div>

        <div class="hero-info">
          <h1>{{ user.Username }}</h1>
          <p>{{ user.Email }}</p>

          <div class="role-badge" :class="user.Role">
            {{ user.Role }}
          </div>
        </div>
      </section>

      <!-- LOADING -->
      <div v-if="loading" class="loading-box">
        Загрузка профиля...
      </div>

      <template v-else>
        <!-- STATS -->
        <section class="stats-grid">
          <div class="stat-card">
            <span class="emoji">🧠</span>
            <h3>Квизов</h3>
            <p>{{ quizResults.length }}</p>
          </div>

          <div class="stat-card">
            <span class="emoji">🔥</span>
            <h3>Лучший результат</h3>
            <p>{{ bestScore }}/{{ bestTotal }}</p>
          </div>

        </section>

        <!-- QUIZ HISTORY -->
        <section class="profile-card">
          <div class="section-header">
            <h2>🧠 История квизов</h2>
          </div>

          <div v-if="quizResults.length === 0" class="empty">
            Пока нет результатов
          </div>

          <div v-else class="quiz-history">
            <div
              class="quiz-item"
              v-for="quiz in quizResults"
              :key="quiz.Id"
            >
              <div>
                <h3>{{ quiz.Score }}/{{ quiz.TotalQuestions }}</h3>
                <p>{{ formatDate(quiz.CompletedAt) }}</p>
              </div>

              <div
                class="quiz-badge"
                :class="getQuizClass(quiz.Score, quiz.TotalQuestions)"
              >
                {{ getQuizLabel(quiz.Score, quiz.TotalQuestions) }}
              </div>
            </div>
          </div>
        </section>
        <!-- ACTIONS -->
        <section class="actions">
          <button class="action-btn blue" @click="$router.push('/quiz')">
            🧠 Пройти квиз
          </button>
        </section>
      </template>
    </main>
  </div>
</template>

<script>
import AppHeader from '@/components/AppHeader.vue'
import api from '@/api'
import '@/assets/styles/pages/ProfilePage.css'

export default {
  name: 'ProfilePage',
  components: { AppHeader },
  data() {
    return {
      loading: true,
      user: { Username: '', Email: '', Role: '' },
      quizResults: [],
      viewedComponents: [],
      totalComponents: 0
    }
  },
  computed: {
    bestScore() {
      if (!this.quizResults.length) return 0
      return Math.max(...this.quizResults.map(q => q.Score))
    },
    bestTotal() {
      if (!this.quizResults.length) return 0
      return this.quizResults[0]?.TotalQuestions || 0
    },
    progressPercent() {
      if (!this.totalComponents) return 0
      return Math.round((this.viewedComponents.length / this.totalComponents) * 100)
    }
  },
  async mounted() {
    await this.loadProfile()
  },
  methods: {
    async loadProfile() {
      this.loading = true

      try {
        const savedUser = localStorage.getItem('user')
        if (savedUser) {
          try {
            this.user = JSON.parse(savedUser)
          } catch (e) {
            console.warn('Failed to parse user from localStorage', e)
            localStorage.removeItem('user')
          }
        }

        try {
          const token = localStorage.getItem('token')
          const headers = token ? { Authorization: `Bearer ${token}` } : {}
          
          const quizRes = await api.get('/quiz/results', { headers })
          this.quizResults = quizRes.data || []
        } catch (e) {
          console.warn('Quiz results error', e)
          if (e.response?.status === 401) {
            localStorage.removeItem('token')
            localStorage.removeItem('user')
            this.$router.push('/')
          }
        }
      } catch (error) {
        console.error('Profile load error:', error)
      } finally {
        this.loading = false
      }
    },

    formatDate(date) {
      if (!date) return '-'
      return new Date(date).toLocaleDateString('ru-RU', {
        year: 'numeric', month: 'long', day: 'numeric'
      })
    },

    getQuizClass(score, total) {
      if (!total) return 'bad'
      const percent = (score / total) * 100
      if (percent >= 80) return 'excellent'
      if (percent >= 50) return 'good'
      return 'bad'
    },

    getQuizLabel(score, total) {
      if (!total) return 'Слабо'
      const percent = (score / total) * 100
      if (percent >= 80) return 'Отлично'
      if (percent >= 50) return 'Хорошо'
      return 'Слабо'
    },

  }
}
</script>
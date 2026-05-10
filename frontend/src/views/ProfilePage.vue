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

          <div class="stat-card">
            <span class="emoji">🔧</span>
            <h3>Изучено компонентов</h3>
            <p>{{ viewedComponents.length }}</p>
          </div>

          <div class="stat-card">
            <span class="emoji">🏆</span>
            <h3>Прогресс</h3>
            <p>{{ progressPercent }}%</p>
          </div>
        </section>

        <!-- PROGRESS -->
        <section class="profile-card">
          <div class="section-header">
            <h2>📈 Прогресс изучения комплектующих</h2>
            <span>{{ viewedComponents.length }}/{{ totalComponents }}</span>
          </div>

          <div class="progress-bar">
            <div
              class="progress-fill"
              :style="{ width: progressPercent + '%' }"
            ></div>
          </div>

          <p class="progress-text">
            Вы просмотрели {{ viewedComponents.length }}
            из {{ totalComponents }} комплектующих
          </p>
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

        <!-- VIEWED COMPONENTS -->
        <section class="profile-card">
          <div class="section-header">
            <h2>👀 Просмотренные компоненты</h2>
          </div>

          <div v-if="viewedComponents.length === 0" class="empty">
            Вы ещё не изучали комплектующие
          </div>

          <div v-else class="components-grid">
            <div
              v-for="item in viewedComponents"
              :key="item.id"
              class="component-item"
            >
              <div class="component-icon">💻</div>

              <div>
                <h3>{{ item.name }}</h3>
                <p>{{ item.category }}</p>
              </div>
            </div>
          </div>
        </section>

        <!-- ACTIONS -->
        <section class="actions">
          <button class="action-btn blue" @click="$router.push('/quiz')">
            🧠 Пройти квиз
          </button>

          <button class="action-btn green" @click="$router.push('/components')">
            🔧 Компоненты
          </button>

          <button class="action-btn red" @click="logout">
            🚪 Выйти
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

  components: {
    AppHeader
  },

  data() {
    return {
      loading: true,

      user: {
        Username: '',
        Email: '',
        Role: ''
      },

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

      return this.quizResults[0].TotalQuestions
    },

    progressPercent() {
      if (!this.totalComponents) return 0

      return Math.round(
        (this.viewedComponents.length / this.totalComponents) * 100
      )
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
          this.user = JSON.parse(savedUser)
        }

        // quiz results
        try {
          const quizRes = await api.get('/quiz/results')

          this.quizResults = quizRes.data || []
        } catch (e) {
          console.warn('Quiz results error', e)
        }

        // viewed components
        const viewed = localStorage.getItem('viewed_components')

        if (viewed) {
          this.viewedComponents = JSON.parse(viewed)
        }

        // total components
        try {
          const compRes = await api.get('/components')

          this.totalComponents = compRes.data.length || 0
        } catch (e) {
          console.warn('Components error', e)
        }

      } catch (err) {
        console.error(err)
      } finally {
        this.loading = false
      }
    },

    formatDate(date) {
      return new Date(date).toLocaleDateString('ru-RU', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      })
    },

    getQuizClass(score, total) {
      const percent = (score / total) * 100

      if (percent >= 80) return 'excellent'
      if (percent >= 50) return 'good'

      return 'bad'
    },

    getQuizLabel(score, total) {
      const percent = (score / total) * 100

      if (percent >= 80) return 'Отлично'
      if (percent >= 50) return 'Хорошо'

      return 'Слабо'
    },

    logout() {
      localStorage.removeItem('user')
      localStorage.removeItem('token')

      this.$router.push('/')
    }
  }
}
</script>

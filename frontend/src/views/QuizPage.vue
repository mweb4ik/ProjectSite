<template>
  <div class="page">
    <AppHeader />

    <div class="content">
      <h1>🧠 Квиз по ПК</h1>

      <div v-if="loading" class="skeleton-wrapper">
        <div class="skeleton-text"></div>
        <div class="skeleton-buttons">
          <div class="skeleton-btn" v-for="i in 4" :key="i"></div>
        </div>
      </div>

      <div v-else>
        <div v-if="currentQuestion" class="quiz-card">
          <h2>{{ currentQuestion.question }}</h2>

          <div class="options">
            <button
              v-for="(opt, i) in currentQuestion.options"
              :key="i"
              class="option-btn"
              :class="{ selected: answers[currentQuestion.id] === i }"
              @click="selectAnswer(i)"
            >
              {{ opt }}
            </button>
          </div>

          <button class="btn-section" @click="next">
            {{ isLast ? 'Завершить' : 'Далее →' }}
          </button>
        </div>

        <div v-else>
          <button class="btn-section" @click="submit">
            Отправить результаты
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import AppHeader from '@/components/AppHeader.vue'
import api from '@/api'

export default {
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
    }
  },

  async mounted() {
    const res = await api.get('/quiz')
    this.questions = res.data
    this.loading = false
  },

  methods: {
    selectAnswer(i) {
      this.answers[this.currentQuestion.id] = i
    },

    next() {
      if (!this.isLast) this.index++
      else this.index++
    },

    async submit() {
      const res = await api.post('/quiz/submit', this.answers)
      this.$router.push({ path: '/quiz-result', state: res.data })
    }
  }
}
</script>

<style scoped>
.quiz-card {
  background: #1e1e1e;
  padding: 25px;
  border-radius: 16px;
  box-shadow: 0 0 25px rgba(0, 163, 255, 0.2);
}

.options {
  display: grid;
  gap: 10px;
  margin: 20px 0;
}

.option-btn {
  padding: 12px;
  border-radius: 10px;
  border: 2px solid #00a3ff33;
  background: #2a2a2a;
  color: white;
  cursor: pointer;
  transition: 0.2s;
}

.option-btn:hover {
  transform: translateY(-2px);
}

.selected {
  background: linear-gradient(135deg, #00ff9d, #00a3ff);
}
</style>
<template>
  <div class="page">
    <AppHeader />

    <div class="content">
      <h1>📊 Результат</h1>

      <div class="profile-card">
        <p>Баллы: {{ result.Score }}</p>
        <p>Всего: {{ result.TotalQuestions }}</p>
      </div>

      <button class="btn-section" @click="$router.push('/quiz')">
        Пройти снова
      </button>
    </div>
  </div>
</template>

<script>
import AppHeader from '@/components/AppHeader.vue'

export default {
  components: { AppHeader },

  data() {
    return {
      result: {}
    }
  },

  mounted() {
    this.result = history.state
  }
}
</script>
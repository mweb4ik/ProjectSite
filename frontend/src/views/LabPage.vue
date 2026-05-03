<template>
  <div class="page">
    <AppHeader />

    <div class="content">
      <h1>⚡ Лаборатория разгона</h1>

      <div class="card">
        <label>Частота (GHz)</label>
        <input v-model.number="freq" type="number" step="0.1" />

        <label>Напряжение (V)</label>
        <input v-model.number="voltage" type="number" step="0.01" />

        <button class="btn-section" @click="simulate">
          🔬 Симулировать
        </button>
      </div>

      <div v-if="result" class="card result">
        <p>🌡 Температура: {{ result.temperature.toFixed(1) }} °C</p>
        <p>📊 Стабильность: {{ result.stability.toFixed(1) }}%</p>

        <p :class="result.success ? 'good' : 'bad'">
          {{ result.success ? '✅ Успешно' : '❌ Нестабильно' }}
        </p>

        <button
          v-if="isAuth"
          class="btn-component green"
          @click="saveProfile"
        >
          💾 Сохранить профиль
        </button>
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
      freq: 4.0,
      voltage: 1.2,
      result: null
    }
  },

  computed: {
    isAuth() {
      return !!localStorage.getItem('token')
    }
  },

  methods: {
    async simulate() {
      try {
        const res = await api.post('/overclock/simulate', {
          frequency: this.freq,
          voltage: this.voltage
        })
        this.result = res.data
      } catch (e) {
        console.error(e)
        alert('Ошибка симуляции')
      }
    },

    async saveProfile() {
      try {
        await api.post('/overclock/save', {
          cpuName: "Custom CPU",
          frequency: this.freq,
          voltage: this.voltage,
          temperature: this.result.temperature,
          stability: this.result.stability
        })

        alert('Профиль сохранён ✅')
      } catch (e) {
        console.error(e)
        alert('Ошибка сохранения')
      }
    }
  }
}
</script>

<style scoped>
.page {
  max-width: 900px;
  margin: auto;
}

.card {
  margin-top: 20px;
  padding: 20px;
  border-radius: 12px;
  background: #1e1e1e;
  box-shadow: 0 0 20px rgba(0, 163, 255, 0.2);
  display: flex;
  flex-direction: column;
  gap: 10px;
}

input {
  padding: 10px;
  border-radius: 8px;
  border: none;
  background: #2a2a2a;
  color: white;
}

.result {
  text-align: center;
}

.good {
  color: #00ff9d;
  font-weight: bold;
}

.bad {
  color: #ff4d4d;
  font-weight: bold;
}
</style>
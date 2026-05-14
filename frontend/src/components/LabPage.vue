<template>
  <div class="lab-wrapper">
    <AppHeader :user="user" @logout="handleLogout" />
    
    <div class="lab-page">
      <!-- HERO -->
      <section class="hero">
        <div class="hero-content">
          <p class="hero-subtitle">PC PERFORMANCE LAB</p>
          <h1>⚡ Лаборатория разгона</h1>
          <p class="hero-text">
            Симулируйте разгон процессора, анализируйте стабильность
            системы и сохраняйте лучшие профили.
          </p>
        </div>
        <div class="hero-glow"></div>
      </section>

      <!-- MAIN GRID -->
      <div class="lab-grid">
        <!-- CONTROL PANEL -->
        <section class="panel control-panel">
          <div class="panel-header">
            <h2>🎛️ Параметры разгона</h2>
          </div>

          <div class="form-group">
            <label>Частота CPU</label>
            <div class="range-info">
              <span>{{ form.frequency.toFixed(1) }} GHz</span>
            </div>
            <input
              v-model.number="form.frequency"
              type="range"
              min="2"
              max="7"
              step="0.1"
              class="slider"
            />
          </div>

          <div class="form-group">
            <label>Напряжение</label>
            <div class="range-info">
              <span>{{ form.voltage.toFixed(2) }} V</span>
            </div>
            <input
              v-model.number="form.voltage"
              type="range"
              min="0.8"
              max="2"
              step="0.01"
              class="slider"
            />
          </div>

          <div class="button-group">
            <button
              class="btn-primary"
              @click="simulate"
              :disabled="loading"
            >
              {{ loading ? 'Симуляция...' : '🚀 Запустить тест' }}
            </button>

            <button
              class="btn-secondary"
              @click="saveProfile"
              :disabled="!simulation"
            >
              💾 Сохранить профиль
            </button>
          </div>
        </section>

        <!-- RESULTS -->
        <section class="panel result-panel">
          <div class="panel-header">
            <h2>📊 Результаты</h2>
          </div>

          <div v-if="simulation" class="results">
            <div class="result-card">
              <p>🌡️ Температура</p>
              <h3 :class="{ hot: simulation.temperature > 90 }">
                {{ simulation.temperature.toFixed(1) }}°C
              </h3>
            </div>

            <div class="result-card">
              <p>🛡️ Стабильность</p>
              <h3 :class="{ danger: simulation.stability < 50 }">
                {{ simulation.stability.toFixed(1) }}%
              </h3>
            </div>

            <div class="status-box" :class="simulation.success ? 'success' : 'fail'">
              <span v-if="simulation.success">✅ Разгон стабилен</span>
              <span v-else>❌ Система нестабильна</span>
            </div>
          </div>

          <div v-else class="empty-state">
            <p>Запустите симуляцию разгона</p>
          </div>
        </section>
      </div>

      <!-- SAVED PROFILES -->
      <section class="profiles-section panel">
        <div class="panel-header">
          <h2>💾 Сохранённые профили</h2>
          <button class="btn-refresh" @click="loadProfiles">🔄</button>
        </div>

        <div v-if="profiles.length" class="profiles-grid">
          <div
            v-for="profile in profiles"
            :key="profile.Id"
            class="profile-card"
          >
            <div class="profile-top">
              <h3>{{ profile.CpuId || 'Custom CPU' }}</h3>
              <span class="profile-date">{{ formatDate(profile.CreatedAt) }}</span>
            </div>

            <div class="profile-stats">
              <div>
                <span>⚡ Частота</span>
                <strong>{{ profile.Frequency }} GHz</strong>
              </div>
              <div>
                <span>🔋 Напряжение</span>
                <strong>{{ profile.Voltage }} V</strong>
              </div>
              <div>
                <span>🌡️ Температура</span>
                <strong>{{ profile.Temperature.toFixed(1) }}°C</strong>
              </div>
              <div>
                <span>🛡️ Стабильность</span>
                <strong>{{ profile.Stability.toFixed(1) }}%</strong>
              </div>
            </div>
          </div>
        </div>

        <div v-else class="empty-state">
          <p>Пока нет сохранённых профилей</p>
        </div>
      </section>
    </div>
  </div>
</template>

<script>
import { api } from '@/api'
import '@/assets/styles/pages/LabPage.css'
import AppHeader from '@/components/AppHeader.vue' 

export default {
  name: 'LabPage',

  // 🔥 Регистрация компонента
  components: {
    AppHeader
  },

  data() {
    return {
      user: JSON.parse(localStorage.getItem('user') || '{}'),
      
      loading: false,
      simulation: null,
      profiles: [],
      form: {
        frequency: 4.5,
        voltage: 1.2
      }
    }
  },

  async mounted() {
    await this.loadProfiles()
  },

  methods: {
    // 🔥 Метод выхода для хедера
    handleLogout() {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      this.$router.push('/')
    },

    async simulate() {
      this.loading = true
      try {
        const res = await api.post('/overclock/simulate', {
          Frequency: this.form.frequency,
          Voltage: this.form.voltage
        })
        this.simulation = res.data
      } catch (err) {
        console.error(err)
        alert('Ошибка симуляции')
      } finally {
        this.loading = false
      }
    },

    async saveProfile() {
      if (!this.simulation) return
      try {
        await api.post('/overclock/save', {
          CpuId: 'custom-cpu',
          Frequency: this.form.frequency,
          Voltage: this.form.voltage,
          Temperature: this.simulation.temperature,
          Stability: this.simulation.stability
        })
        alert('Профиль сохранён')
        await this.loadProfiles()
      } catch (err) {
        console.error(err)
        alert('Ошибка сохранения')
      }
    },

    async loadProfiles() {
      try {
        const res = await api.get('/overclock')
        this.profiles = res.data ?? []
      } catch (err) {
        console.error(err)
      }
    },

    formatDate(date) {
      return new Date(date).toLocaleDateString('ru-RU')
    }
  }
}
</script>
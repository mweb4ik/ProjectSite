<template>
  <head>
  
  </head>
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

            <h3
              :class="{
                hot: simulation.temperature > 90
              }"
            >
              {{ simulation.temperature.toFixed(1) }}°C
            </h3>
          </div>

          <div class="result-card">
            <p>🛡️ Стабильность</p>

            <h3
              :class="{
                danger: simulation.stability < 50
              }"
            >
              {{ simulation.stability.toFixed(1) }}%
            </h3>
          </div>

          <div
            class="status-box"
            :class="simulation.success ? 'success' : 'fail'"
          >
            <span v-if="simulation.success">
              ✅ Разгон стабилен
            </span>

            <span v-else>
              ❌ Система нестабильна
            </span>
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

        <button class="btn-refresh" @click="loadProfiles">
          🔄
        </button>
      </div>

      <div v-if="profiles.length" class="profiles-grid">

        <div
          v-for="profile in profiles"
          :key="profile.Id"
          class="profile-card"
        >
          <div class="profile-top">
            <h3>{{ profile.CpuId || 'Custom CPU' }}</h3>

            <span class="profile-date">
              {{ formatDate(profile.CreatedAt) }}
            </span>
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
</template>

<script>
import { api } from '@/api'

export default {
  name: 'LabPage',

  data() {
    return {
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
    async simulate() {
      this.loading = true

      try {
        const res = await api.post('/overclock/simulate', {
          Frequency: this.form.frequency,
          Voltage: this.form.voltage
        })

        this.simulation = res.data
      }
      catch (err) {
        console.error(err)
        alert('Ошибка симуляции')
      }
      finally {
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
      }
      catch (err) {
        console.error(err)
        alert('Ошибка сохранения')
      }
    },

    async loadProfiles() {
      try {
        const res = await api.get('/overclock')

        this.profiles = res.data ?? []
      }
      catch (err) {
        console.error(err)
      }
    },

    formatDate(date) {
      return new Date(date).toLocaleDateString('ru-RU')
    }
  }
}
</script>

<style scoped>
.lab-page {
  min-height: 100vh;
  padding: 40px;
  background:
    radial-gradient(circle at top, rgba(0,255,200,0.12), transparent 30%),
    #0b0f17;

  color: #fff;
}

/* HERO */

.hero {
  position: relative;
  margin-bottom: 40px;
  overflow: hidden;
  border-radius: 24px;
  padding: 50px;
  background:
    linear-gradient(
      135deg,
      rgba(0,255,170,0.08),
      rgba(0,140,255,0.08)
    );

  border: 1px solid rgba(255,255,255,0.08);
}

.hero-content {
  position: relative;
  z-index: 2;
}

.hero-subtitle {
  color: #00ffd0;
  letter-spacing: 3px;
  margin-bottom: 10px;
  font-size: 14px;
}

.hero h1 {
  font-size: 54px;
  margin-bottom: 20px;
  font-family: 'Orbitron', sans-serif;
}

.hero-text {
  max-width: 700px;
  color: #b7c0d8;
  line-height: 1.7;
  font-size: 18px;
}

.hero-glow {
  position: absolute;
  right: -100px;
  top: -100px;
  width: 400px;
  height: 400px;
  background: #00ffd0;
  opacity: 0.12;
  filter: blur(120px);
}

/* GRID */

.lab-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 25px;
  margin-bottom: 30px;
}

.panel {
  background: rgba(15,20,30,0.92);
  border: 1px solid rgba(255,255,255,0.08);
  border-radius: 24px;
  padding: 30px;
  backdrop-filter: blur(12px);
}

.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 25px;
}

.panel-header h2 {
  font-size: 24px;
  font-family: 'Orbitron', sans-serif;
}

/* FORM */

.form-group {
  margin-bottom: 28px;
}

.form-group label {
  display: block;
  margin-bottom: 12px;
  color: #d8e2ff;
}

.range-info {
  margin-bottom: 10px;
  color: #00ffd0;
  font-weight: bold;
}

.slider {
  width: 100%;
  accent-color: #00ffd0;
}

/* BUTTONS */

.button-group {
  display: flex;
  gap: 15px;
  margin-top: 25px;
}

.btn-primary,
.btn-secondary,
.btn-refresh {
  border: none;
  border-radius: 14px;
  cursor: pointer;
  transition: 0.25s;
  font-weight: 600;
}

.btn-primary {
  background: linear-gradient(135deg, #00ffd0, #0099ff);
  color: #000;
  padding: 14px 24px;
}

.btn-primary:hover {
  transform: translateY(-2px);
}

.btn-secondary {
  background: rgba(255,255,255,0.08);
  color: white;
  padding: 14px 24px;
}

.btn-secondary:hover {
  background: rgba(255,255,255,0.15);
}

.btn-refresh {
  background: rgba(255,255,255,0.08);
  color: white;
  width: 42px;
  height: 42px;
}

/* RESULTS */

.results {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.result-card {
  background: rgba(255,255,255,0.05);
  border-radius: 18px;
  padding: 24px;
}

.result-card p {
  color: #9fb0d3;
  margin-bottom: 10px;
}

.result-card h3 {
  font-size: 40px;
}

.hot {
  color: #ff5e5e;
}

.danger {
  color: #ffb347;
}

.status-box {
  padding: 18px;
  border-radius: 16px;
  text-align: center;
  font-weight: bold;
}

.status-box.success {
  background: rgba(0,255,120,0.12);
  color: #00ff88;
}

.status-box.fail {
  background: rgba(255,0,80,0.12);
  color: #ff5c8a;
}

/* PROFILES */

.profiles-section {
  margin-top: 20px;
}

.profiles-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 20px;
}

.profile-card {
  background: rgba(255,255,255,0.04);
  border-radius: 18px;
  padding: 22px;
  border: 1px solid rgba(255,255,255,0.05);
}

.profile-top {
  display: flex;
  justify-content: space-between;
  margin-bottom: 18px;
}

.profile-top h3 {
  font-size: 18px;
}

.profile-date {
  color: #8ea0c7;
  font-size: 14px;
}

.profile-stats {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.profile-stats span {
  display: block;
  color: #8ea0c7;
  margin-bottom: 4px;
}

.profile-stats strong {
  font-size: 18px;
}

/* EMPTY */

.empty-state {
  padding: 50px 20px;
  text-align: center;
  color: #7f8ca8;
}

/* MOBILE */

@media (max-width: 1000px) {
  .lab-grid {
    grid-template-columns: 1fr;
  }

  .hero h1 {
    font-size: 42px;
  }
}

@media (max-width: 700px) {
  .lab-page {
    padding: 20px;
  }

  .hero {
    padding: 30px;
  }

  .hero h1 {
    font-size: 34px;
  }

  .button-group {
    flex-direction: column;
  }
}
</style>
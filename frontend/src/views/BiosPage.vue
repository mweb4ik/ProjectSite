<template>
  <div class="page">
    <AppHeader />

    <div class="content">
      <h1>💾 BIOS Центр</h1>

      <!-- Проверка CPU -->
      <div class="card">
        <h3>🔍 Проверка CPU поддержки</h3>

        <input v-model="cpuId" placeholder="CPU ID (cpu-intel-1)" />
        <input v-model="biosId" placeholder="BIOS ID (bios-1)" />

        <button class="btn-section" @click="checkCpu">
          Проверить
        </button>

        <p v-if="cpuResult !== null">
          {{ cpuResult ? '✅ Поддерживается' : '❌ Не поддерживается' }}
        </p>
      </div>

      <!-- Проверка обновления -->
      <div class="card">
        <h3>⚡ Проверка обновления BIOS</h3>

        <input v-model="current" placeholder="Текущая версия" />
        <input v-model="target" placeholder="Новая версия" />

        <button class="btn-section" @click="checkUpdate">
          Проверить риск
        </button>

        <p v-if="risk">
          Риск: <b>{{ risk }}</b>
        </p>
      </div>

      <!-- Список BIOS -->
      <div class="card">
        <h3>📦 Доступные BIOS</h3>

        <div v-for="b in bios" :key="b.id" class="bios-item">
          <p><b>{{ b.version }}</b> ({{ new Date(b.releaseDate).toLocaleDateString() }})</p>
          <p>{{ b.description }}</p>
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
      cpuId: '',
      biosId: '',
      cpuResult: null,

      current: '',
      target: '',
      risk: '',

      bios: []
    }
  },

  async mounted() {
    const res = await api.get('/bios')
    this.bios = res.data
  },

  methods: {
    async checkCpu() {
      const res = await api.post('/bios/check-cpu', {
        cpuId: this.cpuId,
        biosId: this.biosId
      })
      this.cpuResult = res.data
    },

    async checkUpdate() {
      const res = await api.post('/bios/check-update', null, {
        params: {
          currentVersion: this.current,
          targetVersion: this.target
        }
      })
      this.risk = res.data.risk
    }
  }
}
</script>

<style scoped>
.card {
  background: #1e1e1e;
  padding: 20px;
  margin-top: 20px;
  border-radius: 12px;
}

input {
  margin: 5px 0;
  padding: 10px;
  border-radius: 8px;
  border: none;
  background: #2a2a2a;
  color: white;
}

.bios-item {
  border-bottom: 1px solid #333;
  padding: 10px 0;
}
</style>
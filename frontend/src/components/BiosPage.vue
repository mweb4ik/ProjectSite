<template>
    <AppHeader :user="user" @logout="handleLogout" />
  <div class="bios-page">
    <div class="bios-hero">
      <h1 class="bios-title">BIOS / UEFI CENTER</h1>

      <p class="bios-subtitle">
        База прошивок, проверка совместимости CPU и симуляция обновления BIOS
      </p>
    </div>

    <!-- Loadding -->
    <div v-if="loading" class="loading">
      Загрузка BIOS...
    </div>

    <div v-else>
      <!-- Check updates-->
      <section class="bios-section glass">
        <h2>⚡ Калькулятор риска обновления</h2>

        <div class="update-grid">
          <select v-model="currentVersion">
            <option disabled value="">Текущая версия</option>

            <option
              v-for="bios in biosList"
              :key="bios.Id"
              :value="bios.Version"
            >
              {{ bios.Version }}
            </option>
          </select>

          <select v-model="targetVersion">
            <option disabled value="">Целевая версия</option>

            <option
              v-for="bios in biosList"
              :key="bios.Id"
              :value="bios.Version"
            >
              {{ bios.Version }}
            </option>
          </select>

          <button
            class="btn-primary"
            @click="checkUpdate"
          >
            Проверить
          </button>
        </div>

        <div
          v-if="updateResult"
          class="risk-box"
          :class="updateResult.risk.toLowerCase()"
        >
          <h3>Risk: {{ updateResult.risk }}</h3>

          <p>
            {{ updateResult.current }}
            →
            {{ updateResult.target }}
          </p>

          <p>
            Изменение стабильности:
            {{ updateResult.stabilityChange }}%
          </p>
        </div>
      </section>

      <!-- Simulation-->
      <section class="bios-section glass">
        <h2>🧠 Симулятор обновления BIOS</h2>

        <button
          class="btn-primary"
          @click="startSimulation"
          :disabled="isUpdating"
        >
          {{ isUpdating ? 'Обновление...' : 'Запустить обновление' }}
        </button>

        <div class="progress-wrapper">
          <div
            class="progress-fill"
            :style="{ width: progress + '%' }"
          ></div>
        </div>

        <div class="steps">
          <div
            v-for="(step, index) in flashSteps"
            :key="index"
            class="step"
            :class="{ active: currentStep >= index }"
          >
            {{ step }}
          </div>
        </div>
      </section>

      <!-- CPU support -->
      <section class="bios-section glass">
        <h2>🧩 CPU ↔ BIOS Compatibility</h2>

        <div class="cpu-check">
          <input
            v-model="cpuId"
            placeholder="Введите CPU ID"
          />

          <select v-model="selectedBiosId">
            <option disabled value="">
              Выберите BIOS
            </option>

            <option
              v-for="bios in biosList"
              :key="bios.Id"
              :value="bios.Id"
            >
              {{ bios.Version }}
            </option>
          </select>

          <button
            class="btn-primary"
            @click="checkCpuSupport"
          >
            Проверить
          </button>
        </div>

        <div
          v-if="cpuSupport !== null"
          class="cpu-result"
          :class="cpuSupport ? 'success' : 'error'"
        >
          {{
            cpuSupport
              ? '✅ CPU поддерживается'
              : '❌ CPU не поддерживается'
          }}
        </div>
      </section>

      <!-- Timeline BIOS -->
      <section class="bios-section">
        <h2 class="timeline-title">
          📜 История версий BIOS
        </h2>

        <div class="timeline">
          <div
            v-for="bios in biosList"
            :key="bios.Id"
            class="timeline-item"
          >
            <div class="timeline-dot"></div>

            <div class="bios-card">
              <div class="bios-header">
                <h2>{{ bios.Version }}</h2>

                <span
                  class="bios-badge"
                  :class="{ beta: bios.IsBeta }"
                >
                  {{ bios.IsBeta ? 'BETA' : 'STABLE' }}
                </span>
              </div>

              <p class="bios-description">
                {{ bios.Description }}
              </p>

              <div class="bios-info">
                <span>
                  📅 {{ formatDate(bios.ReleaseDate) }}
                </span>

                <span>
                  ⚡ Stability:
                  {{ bios.Stability }}%
                </span>
              </div>

              <div class="stability-bar">
                <div
                  class="stability-fill"
                  :style="{ width: bios.Stability + '%' }"
                ></div>
              </div>

              <div class="bios-actions">
                <button class="btn-secondary">
                  📥 Update
                </button>

                <button class="btn-danger">
                  ↩ Rollback
                </button>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'          
import AppHeader from '@/components/AppHeader.vue' 
import '@/assets/styles/pages/BiosPage.css'
import api from '@/api'

const router = useRouter()
const biosList = ref([])
const loading = ref(true)

const currentVersion = ref('')
const targetVersion = ref('')
const updateResult = ref(null)

const cpuId = ref('')
const selectedBiosId = ref('')
const cpuSupport = ref(null)

const progress = ref(0)
const isUpdating = ref(false)
const currentStep = ref(-1)
const user = ref(JSON.parse(localStorage.getItem('user') || '{}'))
const handleLogout = () => {
  localStorage.removeItem('token')
  localStorage.removeItem('user')
  router.push('/')
}
const flashSteps = [
  'Проверка файла BIOS',
  'Подготовка памяти',
  'Очистка старой прошивки',
  'Запись новой версии',
  'Проверка целостности',
  'Перезагрузка системы'
]

onMounted(async () => {
  try {
    const res = await api.get('/bios')
    biosList.value = res.data
  } catch (e) {
    console.error('Ошибка BIOS:', e)
  } finally {
    loading.value = false
  }
})

const checkUpdate = async () => {
  try {
    const res = await api.post('/bios/check-update', {
      currentVersion: currentVersion.value,
      targetVersion: targetVersion.value
    })

    updateResult.value = res.data
  } catch (e) {
    console.error(e)
  }
}

const checkCpuSupport = async () => {
  try {
    const res = await api.post('/bios/check-cpu', {
      cpuId: cpuId.value,
      biosId: selectedBiosId.value
    })

    cpuSupport.value = res.data
  } catch (e) {
    cpuSupport.value = false
  }
}

const startSimulation = () => {
  progress.value = 0
  currentStep.value = -1
  isUpdating.value = true

  let step = 0

  const interval = setInterval(() => {
    progress.value += 16
    currentStep.value++

    step++

    if (step >= flashSteps.length) {
      clearInterval(interval)

      progress.value = 100

      setTimeout(() => {
        isUpdating.value = false
      }, 1000)
    }
  }, 1000)
}

const formatDate = (date) => {
  return new Date(date).toLocaleDateString()
}
</script>
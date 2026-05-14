<template>
  <div class="details-page">
    <AppHeader :user="user" @logout="logout" />

    <main class="content">
      <button @click="router.back()" class="btn-back">
        ← Назад
      </button>

      <div v-if="loading">Загрузка...</div>
      <div v-else-if="error">{{ error }}</div>

      <div v-else-if="component" class="product-card">

        <div class="card-header">
          <span class="badge">{{ component.category }}</span>

          <h1>{{ component.name }}</h1>

          <div class="price">
            {{ component.price }} {{ component.currency }}
          </div>
        </div>

        <div class="card-body">

          <div class="image-wrapper">
            <img
              :src="getImageUrl(component.imageUrl)"
              :alt="component.name"
              @error="onImageError"
            />
          </div>

          <div class="specs">
            <p><strong>Описание:</strong> {{ component.specifications }}</p>

            <p v-if="component.socket">
              <strong>Сокет:</strong> {{ component.socket }}
            </p>

            <p v-if="component.powerConsumption">
              <strong>Потребление:</strong>
              {{ component.powerConsumption }} Вт
            </p>
          </div>

        </div>

      </div>
    </main>
  </div>
</template>

<script setup>
// Загрузка стилей страницы деталей компонентов
import '@/assets/styles/pages/ComponentsDetailsPage.css'
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppHeader from '@/components/AppHeader.vue'
import api from '@/api'
import { mapComponent } from '@/utils/mapper'

const route = useRoute()
const router = useRouter()

// Данные пользователя из localStorage
const user = ref(JSON.parse(localStorage.getItem('user') || 'null'))

const component = ref(null)
const loading = ref(true)
const error = ref(null)

// Формирование URL изображения компонента
const getImageUrl = (path) => {
  if (!path) return ''
  const base = api.defaults.baseURL?.replace('/api', '') || ''
  return `${base}/${path.replace(/^\/+/, '').replace('wwwroot/', '')}`
}

// Обработка ошибок загрузки изображений
const onImageError = (e) => {
  e.target.style.display = 'none'
}

// Загрузка данных компонента по ID
const loadComponent = async () => {
  loading.value = true
  error.value = null

  try {
    const id = route.params.id
    const res = await api.get(`/components/${id}`)

    component.value = mapComponent(res.data)

    // Отправка флага просмотра компонента
    try {
      await api.post('/user-stats/track-component', JSON.stringify(id), {
        headers: { 'Content-Type': 'application/json' }
      })
    } catch (e) {
      console.warn('Tracking error', e)
    }

  } catch (e) {
    console.error(e)
    error.value = 'Ошибка загрузки компонента'
  } finally {
    loading.value = false
  }
}

// Выход из аккаунта с очисткой localStorage
const logout = () => {
  localStorage.clear()
  router.push('/')
}

onMounted(loadComponent)
</script>
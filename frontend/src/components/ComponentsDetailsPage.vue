<template>
  <div class="details-page">
    <AppHeader :user="user" @logout="logout" />

    <main class="content">
      <button @click="router.back()" class="btn-back">← Назад</button>

      <div v-if="loading">Загрузка...</div>
      <div v-else-if="error">{{ error }}</div>

      <div v-else-if="component" class="product-card">

        <div class="card-header">
          <span class="badge">
            {{ component.Category }}
          </span>

          <h1>{{ component.Name }}</h1>

          <div class="price">
            {{ component.Price }} {{ component.Currency }}
          </div>
        </div>

        <div class="card-body">

          <div class="image-wrapper">
            <img
              :src="getImageUrl(component.ImageUrl)"
              :alt="component.Name"
              @error="onImageError"
            />
          </div>

          <div class="specs">

            <p><strong>Описание:</strong> {{ component.Specifications }}</p>

            <p v-if="component.Socket">
              <strong>Сокет:</strong> {{ component.Socket }}
            </p>

            <p v-if="component.PowerConsumption">
              <strong>Потребление:</strong> {{ component.PowerConsumption }} Вт
            </p>

          </div>

        </div>

      </div>
    </main>
  </div>
</template>

<script setup>
import '@/assets/styles/pages/ComponentsDetailsPage.css'
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppHeader from '@/components/AppHeader.vue'
import api from '@/api'
import { mapComponent } from '@/utils/mapper'

const route = useRoute()
const router = useRouter()

const user = ref(JSON.parse(localStorage.getItem('user') || 'null'))
const component = ref(null)
const loading = ref(true)
const error = ref(null)

const getImageUrl = (path) => {
  if (!path) return ''
  const base = api.defaults.baseURL.replace('/api', '')
  return `${base}/${path.replace(/^\/+/, '').replace('wwwroot/', '')}`
}

const onImageError = (e) => {
  e.target.style.display = 'none'
}

const loadComponent = async () => {
  loading.value = true
  error.value = null

  try {
    const id = route.params.id
    const res = await api.get(`/components/${id}`)

    component.value = mapComponent(res.data)

  } catch (e) {
    error.value = 'Ошибка загрузки компонента'
  } finally {
    loading.value = false
  }
}

const logout = () => {
  localStorage.clear()
  router.push('/')
}

onMounted(loadComponent)
</script>
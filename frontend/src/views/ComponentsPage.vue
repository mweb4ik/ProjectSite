<template>
  <div class="content" v-if="item">
    <h1>{{ item.title }}</h1>

    <div class="card">
      <p v-for="(value, key) in item" :key="key">
        <b>{{ key }}:</b> {{ value }}
      </p>
    </div>
  </div>

  <div v-else>
    <h1>Компонент не найден</h1>
  </div>
</template>

<script>
import { components } from '@/data/components'

export default {
  data() {
    return {
      item: null
    }
  },
  mounted() {
    const type = this.$route.params.type
    this.item = components[type]
  }
}
</script>
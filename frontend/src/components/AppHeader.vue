<template>
  <header class="header">
    <h1>Познай устройство компьютера</h1>
    
    <div class="header-right">
      <!-- Инфо о пользователе -->
      <div class="user-info" v-if="user && user.Email">
        <span class="user-email">{{ user.Email }}</span>
        <span class="role-badge" :class="roleClass">{{ user.Role }}</span>
        <button class="btn-logout" @click="$emit('logout')">Выйти</button>
      </div>

      <!-- Навигация (перенесено из HomePage) -->
      <nav class="header-nav">
        <button class="btn-nav all-components" @click="goTo('all')">🖥️ Все комплектующие</button>
        <button class="btn-nav" @click="goTo('lab')">⚡ Лаборатория</button>
        <button class="btn-nav" @click="goTo('quiz')">🧠 Викторина</button>
        <button class="btn-nav" @click="goTo('builder')">🛠️ Сборка ПК</button>
        <button class="btn-nav" @click="goTo('bios')">💾 BIOS / UEFI</button>
        <button class="btn-nav" @click="goTo('profile')">👤 Кабинет</button>
        <button class="btn-nav" @click="goTo('admin')">🛡️ Админ</button>
      </nav>
    </div>
  </header>
</template>
  
<script>
export default {
  name: "AppHeader",
  props: {
    user: Object
  },
  computed: {
    roleClass() {
      const role = this.user?.Role?.toLowerCase() || '';
      switch (role) {
        case "admin": return "admin-badge"
        case "standard": return "user-badge"
        case "user": return "user-badge"
        default: return "guest-badge"
      }
    }
  },
  methods: {
    goTo(page) {
      const componentPages = ['videocard', 'processor', 'motherboard', 'cooling', 'ram', 'storage'];
      if (page === 'all') {
        this.$router.push('/component/all');
      } else {
        this.$router.push(componentPages.includes(page) ? `/component/${page}` : `/${page}`);
      }
    }
  }
}
</script>
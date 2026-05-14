<template>
    <header class="header">
      <h1>Познай устройство компьютера</h1>
  
      <div class="user-info" v-if="user && user.Email">
        <span class="user-email">{{ user.Email }}</span>
        <span class="role-badge" :class="roleClass">{{ user.Role }}</span>
        <button class="btn-logout" @click="$emit('logout')">Выйти</button>
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
    }
  }
  </script>
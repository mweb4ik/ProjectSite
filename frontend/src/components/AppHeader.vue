<template>
    <header class="header">
      <h1>Познай устройство компьютера</h1>
  
      <div class="user-info">
        <span class="user-email">{{ user.email }}</span>
        <span class="role-badge" :class="roleClass">{{ user.role }}</span>
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
        switch (this.user.role?.toLowerCase()) {
          case "admin": return "admin-badge"
          case "user": return "user-badge"
          default: return "guest-badge"
        }
      }
    }
  }
  </script>
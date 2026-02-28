<template>
  <div id="app">
    <h1>Познай Внутреннее устройство компьютера</h1>
    <img src="/images/pc.png" alt="Компьютер">
    <button @click="login">Войти</button>
    <button @click="register">Зарегистрироваться</button>
    <div v-if="isLoggedIn" class="welcome">
      Добро пожаловать, {{ username }}!
    </div>
  </div>
</template>

<script>
export default {
  name: 'App',
  data() {
    return {
      isLoggedIn: false,
      username: 'Пользователь'
    }
  },
  methods: {
    login() {
      this.isLoggedIn = true;
      alert('Вы вошли в систему!');
    },
    register() {
      alert('Функция регистрации будет реализована позже');
    }
  }
}
</script>

<style>
body {
  background: #0D0D0D;
  color: #FFFFFF;
  font-family: 'Exo 2', sans-serif;
  margin: 0;
  padding: 0;
  text-align: center;
}

#app {
  max-width: 1200px;
  margin: 0 auto;
  padding: 40px 20px;
}

h1 {
  font-size: 48px;
  margin: 30px 0;
  background: linear-gradient(135deg, #00FF9D, #00A3FF);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

img {
  max-width: 50%;
  border: 2px solid #00A3FF;
  border-radius: 12px;
  margin: 30px 0;
  box-shadow: 0 0 30px rgba(0, 163, 255, 0.5);
}
img:hover{transform: TranslateY(4px);
box-shadow:0 0 30px rgba(0, 255, 157, 0.1);
}
button {
  background: linear-gradient(135deg, #00FF9D, #00A3FF);
  border: 2px solid #00FF9D;
  color: #000;
  padding: 12px 24px;
  margin: 10px;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

button:hover {
  transform: translateY(-3px);
  box-shadow: 0 0 20px #00FF9D;
}

.welcome {
  margin-top: 30px;
  font-size: 24px;
  color: #00FF9D;
}
</style>
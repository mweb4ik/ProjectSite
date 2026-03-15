<template>
  <div id="app">
    <!-- Loading Skeleton -->
    <div v-if="isLoading" class="skeleton-container">
      <div class="skeleton-buttons">
        <div class="skeleton-btn"></div>
        <div class="skeleton-btn"></div>
        <div class="skeleton-btn"></div>
      </div>
      <div class="skeleton-title"></div>
      <div class="skeleton-image"></div>
    </div>
    
    <!-- Main Content -->
    <div v-else>
      <div class="particle-container">
        <span class="particle" v-for="i in 50" :key="i" :style="getParticleStyle(i)"></span>
      </div>
      
      <div class="button-grid">
        <button @click="loginAdmin" class="btn-primary">
          Войти как администратор
        </button>
        <button @click="loginUser" class="btn-primary">
           Войти как пользователь
        </button>
        <button @click="loginGuest" class="btn-primary">
          Войти как гость
        </button>
      </div>
      
      <h1 class="title-glitch">Познай внутреннее устройство компьютера</h1>
      
      <div class="image-container">
        <img src="/images/pc.png" alt="Computer" class="pc-image" @load="onImageLoad">
        <div class="image-glow"></div>
      </div>
      
      <div v-if="isLoggedIn" class="welcome">
        Приветствую, {{ username }}!
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'App',
  
  data() {
    return {
      isLoggedIn: false,
      username: 'User',
      isLoading: true,
      imageLoaded: false
    }
  },
  
  methods: {
    getParticleStyle(index) {
      const x = Math.random() * 100
      const duration = 5 + Math.random() * 10
      const delay = Math.random() * 5
      const size = 2 + Math.random() * 4
      return {
        left: x + '%',
        animationDuration: duration + 's',
        animationDelay: delay + 's',
        width: size + 'px',
        height: size + 'px'
      }
    },
    
    onImageLoad() {
      this.imageLoaded = true
    },
    
    loginAdmin() {
      this.isLoggedIn = true
      this.username = 'Администратор'
      this.showNotification('Вход как администратор', 'успех')
    },
    
    loginUser() {
      this.isLoggedIn = true
      this.username = 'Пользователь'
      this.showNotification('Вход как пользователь', 'успех')
    },
    
    loginGuest() {
      this.isLoggedIn = true
      this.username = 'Гость'
      this.showNotification('Вход как гость', 'информация')
    },
    
    showNotification(message, type) {
      console.log('[' + type + ']: ' + message)
    }
  },
  
  mounted() {
    setTimeout(() => {
      this.isLoading = false
    }, 1200)
  }
}
</script>

<style>
/* ========== Global Styles ========== */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  background: linear-gradient(135deg, #0D0D0D 0%, #1a1a2e 100%);
  color: #FFFFFF;
  font-family: 'Exo 2', sans-serif;
  min-height: 100vh;
  overflow-x: hidden;
}

#app {
  max-width: 1200px;
  margin: 0 auto;
  padding: 60px 20px;
  position: relative;
  z-index: 1;
}

/* ========== Loading Skeleton ========== */
.skeleton-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 40px;
  padding: 40px 20px;
}

.skeleton-buttons {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
  max-width: 700px;
  width: 100%;
}

.skeleton-btn {
  height: 52px;
  background: linear-gradient(90deg, #1a1a2e 25%, #2a2a4e 50%, #1a1a2e 75%);
  background-size: 200% 100%;
  border-radius: 14px;
  animation: skeleton-loading 1.5s ease-in-out infinite;
}

.skeleton-title {
  width: 80%;
  height: 50px;
  background: linear-gradient(90deg, #1a1a2e 25%, #2a2a4e 50%, #1a1a2e 75%);
  background-size: 200% 100%;
  border-radius: 12px;
  animation: skeleton-loading 1.5s ease-in-out infinite;
  animation-delay: 0.2s;
}

.skeleton-image {
  width: 55%;
  height: 300px;
  background: linear-gradient(90deg, #1a1a2e 25%, #2a2a4e 50%, #1a1a2e 75%);
  background-size: 200% 100%;
  border-radius: 20px;
  animation: skeleton-loading 1.5s ease-in-out infinite;
  animation-delay: 0.4s;
}

@keyframes skeleton-loading {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

/* ========== Typography ========== */
h1 {
  font-family: 'Orbitron', sans-serif;
  font-size: 42px;
  font-weight: 700;
  letter-spacing: 3px;
  text-transform: uppercase;
  margin: 40px 0;
  background: linear-gradient(135deg, #00FF9D 0%, #00A3FF 50%, #00FF9D 100%);
  background-size: 200% auto;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  animation: gradientShift 3s ease infinite;
  text-shadow: 0 0 30px rgba(0, 255, 157, 0.3);
}

@keyframes gradientShift {
  0%, 100% { background-position: 0% center; }
  50% { background-position: 100% center; }
}

.welcome {
  font-family: 'Exo 2', sans-serif;
  font-size: 28px;
  font-weight: 600;
  letter-spacing: 0.8px;
  margin-top: 40px;
  padding: 20px 40px;
  background: linear-gradient(135deg, rgba(0, 255, 157, 0.1), rgba(0, 163, 255, 0.1));
  border: 1px solid rgba(0, 255, 157, 0.3);
  border-radius: 16px;
  display: inline-block;
  animation: welcomePulse 2s ease infinite;
}

.welcome-icon {
  margin-right: 10px;
  animation: sparkle 1.5s ease infinite;
}

@keyframes welcomePulse {
  0%, 100% { box-shadow: 0 0 20px rgba(0, 255, 157, 0.2); }
  50% { box-shadow: 0 0 40px rgba(0, 255, 157, 0.4); }
}

@keyframes sparkle {
  0%, 100% { transform: scale(1) rotate(0deg); }
  50% { transform: scale(1.2) rotate(10deg); }
}

/* ========== Buttons ========== */
.button-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
  max-width: 700px;
  margin: 50px auto;
}

button {
  background: linear-gradient(135deg, #00FF9D 0%, #00A3FF 100%);
  color: #0D0D0D;
  padding: 16px 28px;
  font-family: 'Rajdhani', sans-serif;
  border: none;
  border-radius: 14px;
  font-size: 17px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  letter-spacing: 0.8px;
  text-transform: uppercase;
  position: relative;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
}

button::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.3), transparent);
  transition: left 0.5s;
}

button:hover::before {
  left: 100%;
}

button:hover {
  transform: translateY(-5px) scale(1.02);
  box-shadow: 0 10px 40px rgba(0, 255, 157, 0.4),
              0 0 60px rgba(0, 163, 255, 0.2);
}

button:active {
  transform: translateY(-2px) scale(0.98);
}

.btn-icon {
  font-size: 20px;
  filter: drop-shadow(0 0 5px rgba(255, 255, 255, 0.5));
}

/* ========== Image Container ========== */
.image-container {
  position: relative;
  display: inline-block;
  margin: 50px 0;
}

.pc-image {
  max-width: 55%;
  display: block;
  border-radius: 20px;
  box-shadow: 0 0 40px rgba(0, 163, 255, 0.4);
  transition: all 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  position: relative;
  z-index: 2;
}

.pc-image:hover {
  transform: translateY(-8px) scale(1.02);
  box-shadow: 0 20px 60px rgba(0, 255, 157, 0.3),
              0 0 80px rgba(0, 163, 255, 0.2);
}

.image-glow {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 80%;
  height: 80%;
  background: radial-gradient(circle, rgba(0, 255, 157, 0.3) 0%, transparent 70%);
  filter: blur(40px);
  z-index: 1;
  animation: glowPulse 3s ease-in-out infinite;
}

@keyframes glowPulse {
  0%, 100% { opacity: 0.5; transform: translate(-50%, -50%) scale(1); }
  50% { opacity: 1; transform: translate(-50%, -50%) scale(1.1); }
}

/* ========== Particles ========== */
.particle-container {
  width: 100%;
  height: 100%;
  position: fixed;
  top: 0;
  left: 0;
  pointer-events: none;
  overflow: hidden;
  z-index: 0;
}

.particle {
  position: absolute;
  background: rgba(255, 255, 255, 0.8);
  border-radius: 50%;
  animation: fall linear infinite;
  opacity: 0;
}

.particle:nth-child(odd) {
  background: linear-gradient(135deg, #00FF9D, #00A3FF);
  box-shadow: 0 0 8px rgba(0, 255, 157, 0.8);
}

.particle:nth-child(even) {
  background: linear-gradient(135deg, #00A3FF, #FFFFFF);
  box-shadow: 0 0 8px rgba(0, 163, 255, 0.8);
}

.particle:nth-child(3n) {
  border-radius: 0%;
}

.particle:nth-child(5n) {
  border-radius: 30%;
}

@keyframes fall {
  0% {
    transform: translateY(-10px) translateX(0) rotate(0deg);
    opacity: 0;
  }
  10% {
    opacity: 1;
  }
  90% {
    opacity: 1;
  }
  100% {
    transform: translateY(100vh) translateX(30px) rotate(360deg);
    opacity: 0;
  }
}

/* ========== Responsive Design ========== */
@media (max-width: 768px) {
  .button-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 15px;
  }
  
  h1 {
    font-size: 32px;
  }
  
  .pc-image {
    max-width: 70%;
  }
  
  .skeleton-buttons {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .skeleton-image {
    width: 70%;
  }
}

@media (max-width: 460px) {
  .button-grid {
    grid-template-columns: 1fr;
    max-width: 300px;
  }
  
  h1 {
    font-size: 26px;
    letter-spacing: 1px;
  }
  
  button {
    padding: 14px 20px;
    font-size: 15px;
  }
  
  .pc-image {
    max-width: 90%;
  }
  
  #app {
    padding: 30px 15px;
  }
  
  .skeleton-buttons {
    grid-template-columns: 1fr;
  }
}

/* ========== Additional Effects ========== */
html {
  scroll-behavior: smooth;
}

body {
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

button {
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
  user-select: none;
}
</style>

<template>
  <div id="app">
    <div class="particle-container">
      <span class = "particle"></span>
        <span class = "particle"></span>
          <span class = "particle"></span>
            <span class = "particle"></span>
              <span class = "particle"></span>
                <span class = "particle"></span>
    </div>
     <div class="button-grid">
    <button @click="loginAdmin">Войти как администратор</button>
    <button @click="loginUser">Войти как пользователь</button>
    <button @click="loginGuest">Войти как гость</button>
    </div>
    <h1>познай внутреннее устройство компьютера</h1>
    <img src="/images/pc.png" alt="Компьютер">
   
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
    createParticle() {
      const particle = document.createElement('span')
      particle.classList.add('particle')
      
      const x = Math.random() * 100
      const duration = 5 + Math.random() * 10
      const delay = Math.random() * 5
      const size = 2 + Math.random() * 4
      
      particle.style.left = x + '%'
      particle.style.animationDuration = duration + 's'
      particle.style.animationDelay = delay + 's'
      particle.style.width = size + 'px'
      particle.style.height = size + 'px'
      
      const container = document.querySelector('.particle-container')
      if (container) {
        container.appendChild(particle)
      }
    },
    
    initParticles() {
      for (let i = 0; i < 30; i++) {
        this.createParticle()
      }
    },
    
    loginAdmin() {
      this.isLoggedIn = true
      this.username = 'Администратор'
      alert('Вы вошли как администратор!')
    },
    
    loginUser() {
      this.isLoggedIn = true
      this.username = 'Пользователь'
      alert('Вы вошли как пользователь!')
    },
    
    loginGuest() {
      this.isLoggedIn = true
      this.username = 'Гость'
      alert('Вы вошли как гость!')
    },
    
    register() {
      alert('Функция регистрации будет реализована позже')
    }
  },
  
  mounted() {
    this.initParticles()
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
  font-size: 35px;
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
.button-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  max-width: 600px;
  margin: 35px auto;
}

button {
  background: linear-gradient(135deg, #00FF9D, #00A3FF);
  border: 2px solid #00FF9D;
  color: #000;
  padding: 12px 24px;
  margin:0;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;/*при наведении*/
  transition: all 0.3s ease;
}
button:hover {
  transform: translateY(-3px);
  box-shadow: 0 0 20px #00FF9D;
}
.particle-container{
  width:100%;
  height:100%;
  position:fixed;/*отображение поверх*/
  pointer-events:none;/*не мешать кликам*/    
  overflow: hidden;
  z-index: -1;  /*на заднем фоне поверх кнопок */
}
.particle {
  position: absolute;
  width: 4px;/* размер частицы */
  height: 4px;
  background: rgba(255, 255, 255, 0.8); /* белый прозрачностью */
  border-radius: 50%;   /*круг */
  animation: fall linear infinite;
}
.welcome {
  margin-top: 30px;
  font-size: 24px;
  color: #00FF9D;
}
@media(max-width:768px){
  .button-grid{
    grid-template-columns: repeat(2, 1fr);
    }
  }
@media (max-width: 460px) {
  .button-grid {
    grid-template-columns: 1fr;  
  }

  h1 {
    font-size: 24px;
  }
  
  button {
    padding: 14px 20px; 
    font-size: 14px;
  }
  
  img {
    max-width: 90%; 
  }
}
@keyframes fall {
  0% {
    transform: translateY(-10px) translateX(0);
    opacity: 0;
  }
  10% {
    opacity: 1;
  }
  90% {
    opacity: 1;
  }
  100% {
    transform: translateY(100vh) translateX(20px); /*падение вниз + немного в сторону */
    opacity: 0;
  }
}
</style>
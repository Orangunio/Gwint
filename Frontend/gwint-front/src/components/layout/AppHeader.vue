<template>
  <v-app-bar
      elevation="4"
      class="app-header"
      :class="{ 'header-scrolled': isScrolled }"
  >
    <v-app-bar-title>
      <div class="d-flex align-center ga-2 header-brand" @click="router.push({ name: 'home' })">
        <v-icon icon="mdi-cards-playing" color="amber-darken-2" size="32" />
        <span class="header-title">GWINT</span>
      </div>
    </v-app-bar-title>

    <template #append>
      <div class="d-flex align-center ga-2 mr-2">
        <template v-if="playerStore.isLoggedIn">
          <v-chip color="amber-darken-2" variant="outlined" size="small">
            <v-icon start icon="mdi-sword" />
            Poziom {{ playerStore.player?.level ?? 1 }}
          </v-chip>

          <v-menu>
            <template #activator="{ props }">
              <v-btn v-bind="props" icon variant="text">
                <v-icon icon="mdi-account-circle" size="28" />
              </v-btn>
            </template>

            <v-list density="compact" min-width="180">
              <v-list-item
                  prepend-icon="mdi-account"
                  :title="playerStore.displayName"
                  subtitle="Zalogowany gracz"
              />

              <v-divider />

              <v-list-item
                  prepend-icon="mdi-logout"
                  title="Wyloguj"
                  @click="handleLogout"
              />
            </v-list>
          </v-menu>
        </template>

        <v-btn
            v-else
            color="amber-darken-2"
            variant="outlined"
            size="small"
            prepend-icon="mdi-login"
            @click="goToLogin"
        >
          Zaloguj się
        </v-btn>

        <v-btn
            icon
            variant="text"
            @click="appStore.toggleTheme()"
        >
          <v-icon :icon="appStore.theme === 'dark' ? 'mdi-weather-sunny' : 'mdi-weather-night'" />
        </v-btn>
      </div>
    </template>
  </v-app-bar>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { usePlayerStore } from '@/stores/player'
import { useAppStore } from '@/stores/app'

const router = useRouter()
const playerStore = usePlayerStore()
const appStore = useAppStore()

const isScrolled = ref(false)

const handleScroll = () => {
  isScrolled.value = window.scrollY > 20
}

function goToLogin() {
  router.push({ name: 'login' })
}

function handleLogout() {
  playerStore.logout()
  appStore.addNotification('Zostałeś wylogowany.')
  router.push({ name: 'home' })
}

onMounted(() => window.addEventListener('scroll', handleScroll))
onUnmounted(() => window.removeEventListener('scroll', handleScroll))
</script>

<style scoped>
.header-title {
  font-family: 'MedievalSharp', serif;
  font-size: 1.6rem;
  font-weight: 900;
  letter-spacing: 0.3rem;
  background: linear-gradient(135deg, #ffd740, #ff6d00);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.app-header {
  background: rgba(18, 18, 18, 0.95) !important;
  backdrop-filter: blur(12px);
  border-bottom: 1px solid rgba(255, 215, 64, 0.15);
}

.header-brand {
  cursor: pointer;
}
</style>
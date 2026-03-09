<template>
  <v-app :theme="appStore.theme">
    <AppHeader @open-login="loginDialog = true" />

    <v-main>
      <HeroSection
          @start-game="handleStartGame"
          @learn-more="scrollToMenu"
      />

      <section id="menu-section" class="py-16">
        <v-container>
          <v-row justify="center" class="mb-10">
            <v-col cols="12" class="text-center">
              <h2 class="section-title mb-3">Co chcesz zrobić?</h2>
              <div class="section-divider" />
            </v-col>
          </v-row>

          <v-row justify="center" class="mb-8">
            <v-col cols="12" sm="8" md="6">
              <PlayerStats />
            </v-col>
          </v-row>

          <v-row justify="center">
            <v-col
                v-for="item in menuItems"
                :key="item.title"
                cols="12"
                sm="6"
                md="4"
                lg="3"
            >
              <MenuCard
                  :title="item.title"
                  :description="item.description"
                  :icon="item.icon"
                  :icon-color="item.iconColor"
                  :glow-color="item.glowColor"
                  :badge="item.badge"
                  :badge-color="item.badgeColor"
                  :disabled="item.disabled"
                  @click="item.action"
              />
            </v-col>
          </v-row>
        </v-container>
      </section>
    </v-main>

    <AppFooter />

    <v-snackbar
        v-for="(note, i) in appStore.notifications"
        :key="i"
        :model-value="true"
        color="amber-darken-2"
        rounded="lg"
        :timeout="3000"
        @update:model-value="appStore.removeNotification(i)"
    >
      {{ note }}
    </v-snackbar>
  </v-app>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppFooter from '@/components/layout/AppFooter.vue'
import HeroSection from '@/components/home/HeroSection.vue'
import MenuCard from '@/components/home/MenuCard.vue'
import PlayerStats from '@/components/home/PlayerStats.vue'
import { useAppStore } from '@/stores/app'
import { usePlayerStore } from '@/stores/player'

const appStore = useAppStore()
const playerStore = usePlayerStore()

const loginDialog = ref(false)
const loginName = ref('')

const menuItems = [
  {
    title: 'Gra Online',
    description: 'Zmierz się z innymi graczami w rozgrywce online w czasie rzeczywistym.',
    icon: 'mdi-sword-cross',
    iconColor: 'amber-darken-2',
    glowColor: 'rgba(255, 215, 64, 0.12)',
    badge: 'Dostępne',
    badgeColor: 'green',
    disabled: false,
    action: () => handleStartGame(),
  },
  {
    title: 'Gra z Botem',
    description: 'Trenuj swoje umiejętności, grając przeciwko komputerowemu przeciwnikowi.',
    icon: 'mdi-robot',
    iconColor: 'blue-lighten-2',
    glowColor: 'rgba(66, 165, 245, 0.12)',
    badge: 'Trening',
    badgeColor: 'blue',
    disabled: true,
    action: () => {},
  },
  {
    title: 'Kolekcja Kart',
    description: 'Przeglądaj swoją kolekcję kart i buduj potężne decki.',
    icon: 'mdi-cards',
    iconColor: 'purple-lighten-2',
    glowColor: 'rgba(186, 104, 200, 0.12)',
    badge: undefined,
    badgeColor: undefined,
    disabled: true,
    action: () => {},
  },
  {
    title: 'Ranking',
    description: 'Sprawdź swoje miejsce na liście najlepszych graczy świata.',
    icon: 'mdi-trophy',
    iconColor: 'orange-lighten-2',
    glowColor: 'rgba(255, 167, 38, 0.12)',
    badge: undefined,
    badgeColor: undefined,
    disabled: true,
    action: () => {},
  },
]

function handleStartGame() {
  if (!playerStore.isLoggedIn) {
    loginDialog.value = true
    appStore.addNotification('Zaloguj się, aby rozpocząć grę!')
    return
  }
  appStore.addNotification('Szukanie przeciwnika... (wkrótce!)')
}

function scrollToMenu() {
  document.getElementById('menu-section')?.scrollIntoView({ behavior: 'smooth' })
}

</script>

<style scoped>
.section-title {
  font-size: clamp(1.5rem, 3vw, 2.2rem);
  font-weight: 800;
  letter-spacing: 0.05rem;
}

.section-divider {
  width: 60px;
  height: 3px;
  background: linear-gradient(90deg, #ffd740, #ff6d00);
  border-radius: 2px;
  margin-inline: auto;
}

.login-card {
  background: rgba(30, 30, 30, 0.98) !important;
  border: 1px solid rgba(255, 215, 64, 0.15);
}
</style>
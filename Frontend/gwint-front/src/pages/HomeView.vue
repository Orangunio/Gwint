<template>
  <div>
    <AppHeader />

    <v-main>
      <HeroSection
        @learn-more="scrollToMenu"
        @start-game="handleStartGame"
      />

      <section id="menu-section" class="py-16">
        <v-container>
          <v-row class="mb-10" justify="center">
            <v-col class="text-center" cols="12">
              <h2 class="section-title mb-3">Co chcesz zrobić?</h2>
              <div class="section-divider" />
            </v-col>
          </v-row>

          <v-row class="mb-8" justify="center">
            <v-col cols="12" md="6" sm="8">
              <PlayerStats />
            </v-col>
          </v-row>

          <v-row justify="center">
            <v-col
              v-for="item in menuItems"
              :key="item.title"
              cols="12"
              lg="3"
              md="4"
              sm="6"
            >
              <MenuCard
                :badge="item.badge"
                :badge-color="item.badgeColor"
                :description="item.description"
                :disabled="item.disabled"
                :glow-color="item.glowColor"
                :icon="item.icon"
                :icon-color="item.iconColor"
                :title="item.title"
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
      :key="`${note}-${i}`"
      color="amber-darken-2"
      :model-value="true"
      rounded="lg"
      :timeout="3000"
      @update:model-value="appStore.removeNotification(i)"
    >
      {{ note }}
    </v-snackbar>
  </div>
</template>

<script setup lang="ts">
  import { computed } from 'vue'
  import { useRouter } from 'vue-router'

  import HeroSection from '@/components/home/HeroSection.vue'
  import MenuCard from '@/components/home/MenuCard.vue'
  import PlayerStats from '@/components/home/PlayerStats.vue'
  import AppFooter from '@/components/layout/AppFooter.vue'
  import AppHeader from '@/components/layout/AppHeader.vue'

  import { useAppStore } from '@/stores/app'
  import { usePlayerStore } from '@/stores/player'

  const router = useRouter()
  const appStore = useAppStore()
  const playerStore = usePlayerStore()

  function handleStartGame () {
    if (!playerStore.isLoggedIn) {
      appStore.addNotification('Zaloguj się, aby rozpocząć grę.')
      router.push({ name: 'login' })
      return
    }

    appStore.addNotification('Szukanie przeciwnika... (wkrótce!)')
  }

  function scrollToMenu () {
    document.querySelector('#menu-section')?.scrollIntoView({ behavior: 'smooth' })
  }

  const menuItems = computed(() => [
    {
      title: 'Gra Online',
      description: 'Zmierz się z innymi graczami w rozgrywce online w czasie rzeczywistym.',
      icon: 'mdi-sword-cross',
      iconColor: 'amber-darken-2',
      glowColor: 'rgba(255, 215, 64, 0.12)',
      badge: playerStore.isLoggedIn ? 'Dostępne' : 'Wymaga logowania',
      badgeColor: playerStore.isLoggedIn ? 'green' : 'red',
      disabled: false,
      action: handleStartGame,
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
  ])
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
</style>

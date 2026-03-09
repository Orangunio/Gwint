<template>
  <v-card
      v-if="playerStore.isLoggedIn && playerStore.player"
      class="stats-card"
      rounded="xl"
      elevation="0"
  >
    <v-card-text class="pa-6">
      <div class="d-flex align-center ga-4 mb-6">
        <v-avatar size="56" color="amber-darken-2" class="player-avatar">
          <v-icon icon="mdi-account" size="32" color="white" />
        </v-avatar>
        <div>
          <div class="player-name">{{ playerStore.player.name }}</div>
          <v-chip size="x-small" color="amber-darken-2" variant="tonal">
            Poziom {{ playerStore.player.level }}
          </v-chip>
        </div>
        <v-spacer />
        <div class="winrate-badge">
          <div class="wr-value">{{ playerStore.winRate }}</div>
          <div class="wr-label text-caption">Win Rate</div>
        </div>
      </div>

      <v-row>
        <v-col v-for="stat in stats" :key="stat.label" cols="4">
          <div class="stat-block text-center">
            <v-icon :icon="stat.icon" :color="stat.color" size="20" class="mb-1" />
            <div class="stat-val" :style="{ color: stat.color }">{{ stat.value }}</div>
            <div class="text-caption text-medium-emphasis">{{ stat.label }}</div>
          </div>
        </v-col>
      </v-row>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { usePlayerStore } from '@/stores/player'

const playerStore = usePlayerStore()

const stats = computed(() => [
  {
    label: 'Wygrane',
    value: playerStore.player?.wins ?? 0,
    icon: 'mdi-trophy',
    color: '#ffd740',
  },
  {
    label: 'Przegrane',
    value: playerStore.player?.losses ?? 0,
    icon: 'mdi-skull',
    color: '#ef5350',
  },
  {
    label: 'Razem',
    value: (playerStore.player?.wins ?? 0) + (playerStore.player?.losses ?? 0),
    icon: 'mdi-cards',
    color: '#90caf9',
  },
])
</script>

<style scoped>
.stats-card {
  background: rgba(255, 215, 64, 0.04) !important;
  border: 1px solid rgba(255, 215, 64, 0.15);
}

.player-avatar {
  box-shadow: 0 0 16px rgba(255, 215, 64, 0.3);
}

.player-name {
  font-size: 1.1rem;
  font-weight: 700;
  margin-bottom: 4px;
}

.winrate-badge {
  text-align: center;
}

.wr-value {
  font-size: 1.4rem;
  font-weight: 900;
  color: #ffd740;
  line-height: 1;
}

.wr-label {
  color: rgba(255, 255, 255, 0.5);
  letter-spacing: 0.05rem;
}

.stat-block {
  padding: 0.5rem;
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.02);
}

.stat-val {
  font-size: 1.5rem;
  font-weight: 800;
  line-height: 1.2;
}
</style>
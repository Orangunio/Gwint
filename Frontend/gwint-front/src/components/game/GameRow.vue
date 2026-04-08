<template>
  <div
    class="game-row"
    :class="{
      'game-row--droppable': canDrop,
      'game-row--weather': hasWeather,
    }"
    @click="canDrop && $emit('drop')"
  >
    <!-- Etykieta rzędu -->
    <div class="row-label">
      <v-icon :icon="rowIcon" size="14" :color="rowColor" />
      <span class="row-label-text">{{ rowName }}</span>
      <div v-if="hasWeather" class="weather-indicator">
        <v-icon :icon="weatherIcon" size="12" color="blue-lighten-3" />
      </div>
    </div>

    <!-- Wynik rzędu -->
    <div class="row-score" :class="{ 'row-score--active': rowScore > 0 }">
      {{ rowScore }}
    </div>

    <!-- Karty w rzędzie -->
    <div class="row-cards">
      <div v-if="cards.length === 0" class="row-empty">
        <span v-if="canDrop" class="text-caption">Zagraj kartę</span>
      </div>
      <div class="cards-scroll">
        <GameCard
          v-for="card in cards"
          :key="card.id"
          :card="card"
          size="small"
        />
      </div>
    </div>

    <!-- Wskaźnik Rogu Dowódcy -->
    <div class="horn-indicator" :class="{ 'horn-indicator--active': hornActive }">
      <v-icon icon="mdi-bugle" size="18" />
      <span class="horn-indicator-label">Róg</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { GameCard as GameCardType } from '@/stores/signalr'
import GameCard from './GameCard.vue'

const props = withDefaults(
  defineProps<{
    cards: GameCardType[]
    rowIndex: number
    rowScore: number
    hornActive?: boolean
    canDrop?: boolean
    frostActive?: boolean
    fogActive?: boolean
    rainActive?: boolean
  }>(),
  {
    hornActive: false,
    canDrop: false,
    frostActive: false,
    fogActive: false,
    rainActive: false,
  },
)

defineEmits<{ drop: [] }>()

const rowNames = ['Piechota', 'Dystans', 'Oblężenie']
const rowIcons = ['mdi-sword', 'mdi-bow-arrow', 'mdi-cannon']
const rowColors = ['#ef9a9a', '#a5d6a7', '#90caf9']

const rowName = computed(() => rowNames[props.rowIndex] ?? `Rząd ${props.rowIndex + 1}`)
const rowIcon = computed(() => rowIcons[props.rowIndex] ?? 'mdi-cards')
const rowColor = computed(() => rowColors[props.rowIndex] ?? 'white')

const hasWeather = computed(() => {
  if (props.rowIndex === 0) return props.frostActive
  if (props.rowIndex === 1) return props.fogActive
  if (props.rowIndex === 2) return props.rainActive
  return false
})

const weatherIcon = computed(() => {
  if (props.rowIndex === 0) return 'mdi-snowflake'
  if (props.rowIndex === 1) return 'mdi-weather-fog'
  return 'mdi-weather-pouring'
})
</script>

<style scoped>
.game-row {
  display: flex;
  align-items: center;
  gap: 8px;
  min-height: 76px;
  padding: 4px 8px;
  border-radius: 8px;
  border: 1px solid rgba(255, 255, 255, 0.05);
  background: rgba(0, 0, 0, 0.2);
  transition: all 0.2s ease;
}

.game-row--droppable {
  border-color: rgba(255, 215, 64, 0.2);
  cursor: pointer;
}

.game-row--droppable:hover {
  background: rgba(255, 215, 64, 0.05);
  border-color: rgba(255, 215, 64, 0.4);
}

.game-row--weather {
  border-color: rgba(100, 180, 255, 0.3);
  background: rgba(100, 180, 255, 0.04);
}

.row-label {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  min-width: 52px;
  flex-shrink: 0;
}

.row-label-text {
  font-size: 9px;
  color: rgba(255, 255, 255, 0.4);
  letter-spacing: 0.05rem;
  text-transform: uppercase;
  text-align: center;
}

.weather-indicator {
  margin-top: 2px;
}

.row-score {
  min-width: 36px;
  height: 36px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: 800;
  background: rgba(255, 255, 255, 0.05);
  color: rgba(255, 255, 255, 0.4);
  flex-shrink: 0;
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.row-score--active {
  background: rgba(255, 215, 64, 0.12);
  color: #ffd740;
  border-color: rgba(255, 215, 64, 0.3);
}

.row-cards {
  flex: 1;
  min-width: 0;
  overflow: hidden;
}

.row-empty {
  height: 68px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: rgba(255, 255, 255, 0.2);
}

.cards-scroll {
  display: flex;
  gap: 4px;
  overflow-x: auto;
  padding: 4px 2px;
  scrollbar-width: thin;
  scrollbar-color: rgba(255, 215, 64, 0.2) transparent;
}

.cards-scroll::-webkit-scrollbar {
  height: 3px;
}

.cards-scroll::-webkit-scrollbar-track {
  background: transparent;
}

.cards-scroll::-webkit-scrollbar-thumb {
  background: rgba(255, 215, 64, 0.2);
  border-radius: 2px;
}

/* ─── Wskaźnik Rogu Dowódcy ─────────────────────────────────────── */

.horn-indicator {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 3px;
  flex-shrink: 0;
  width: 36px;
  padding: 4px 0;
  border-radius: 6px;
  border: 1px solid rgba(255, 255, 255, 0.07);
  background: rgba(0, 0, 0, 0.2);
  color: rgba(255, 255, 255, 0.15);
  transition: all 0.3s ease;
}

.horn-indicator--active {
  color: #ffd740;
  border-color: rgba(255, 215, 64, 0.4);
  background: rgba(255, 215, 64, 0.1);
  box-shadow: 0 0 8px rgba(255, 215, 64, 0.2), inset 0 0 8px rgba(255, 215, 64, 0.05);
}

.horn-indicator-label {
  font-size: 8px;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: inherit;
}
</style>
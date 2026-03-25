<template>
  <div
    class="game-card"
    :class="{
      'game-card--playable': playable,
      'game-card--selected': selected,
      'game-card--champion': card.isChampion,
      'game-card--special': card.isSpecial,
      'game-card--commander': card.isCommander,
      'game-card--small': size === 'small',
    }"
    :title="`${card.name} | Siła: ${card.finalStrength ?? card.strength}`"
    @click="playable && $emit('play', card)"
  >
    <!-- Siła karty -->
    <div class="card-strength" :class="strengthClass">
      {{ card.isSpecial ? '★' : (card.finalStrength ?? card.strength) }}
    </div>

    <!-- Ikona umiejętności -->
    <div v-if="card.isChampion" class="card-badge card-badge--champion">
      <v-icon icon="mdi-crown" size="10" />
    </div>
    <div v-else-if="card.isCommander" class="card-badge card-badge--commander">
      <v-icon icon="mdi-star" size="10" />
    </div>
    <div v-else-if="abilityIcon" class="card-badge">
      <v-icon :icon="abilityIcon" size="10" />
    </div>

    <!-- Nazwa karty -->
    <div class="card-name">{{ shortName }}</div>

    <!-- Efekt hover -->
    <div class="card-shine" />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { GameCard } from '@/stores/signalr'

const props = withDefaults(
  defineProps<{
    card: GameCard
    playable?: boolean
    selected?: boolean
    size?: 'normal' | 'small'
  }>(),
  {
    playable: false,
    selected: false,
    size: 'normal',
  },
)

defineEmits<{
  play: [card: GameCard]
}>()

// Skrócona nazwa karty (max 12 znaków)
const shortName = computed(() => {
  const name = props.card.name
  return name.length > 12 ? name.slice(0, 11) + '…' : name
})

// Kolor siły
const strengthClass = computed(() => {
  if (props.card.isSpecial) return 'strength--special'
  if (props.card.isChampion) return 'strength--champion'
  const base = props.card.strength
  const final = props.card.finalStrength ?? base
  if (final > base) return 'strength--buffed'
  if (final < base) return 'strength--debuffed'
  return 'strength--normal'
})

// Ikony umiejętności (ability enum z backendu)
const abilityIcons: Record<number, string> = {
  1: 'mdi-account-group',   // braterstwo
  2: 'mdi-eye-off',          // szpieg
  3: 'mdi-run',              // zwinność
  4: 'mdi-heart-plus',       // wskrzeszenie
  5: 'mdi-link',             // więź
  6: 'mdi-arrow-up',         // wyższe morale
  7: 'mdi-fire',             // pozoga jednostki
  8: 'mdi-bugle',            // róg dowódcy jednostki
  10: 'mdi-bugle',           // róg dowódcy (spec)
  11: 'mdi-fire',            // pozoga (spec)
  12: 'mdi-human-dolly',     // manekin (spec)
  13: 'mdi-snowflake',       // mróz
  14: 'mdi-weather-fog',     // mgła
  15: 'mdi-weather-pouring', // deszcz
  16: 'mdi-weather-sunny',   // czyste niebo
}

const abilityIcon = computed(() => abilityIcons[props.card.ability] ?? null)
</script>

<style scoped>
.game-card {
  position: relative;
  width: 56px;
  height: 84px;
  border-radius: 6px;
  background: linear-gradient(160deg, #2a2a1e 0%, #1a1a12 100%);
  border: 1px solid rgba(180, 140, 40, 0.4);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-end;
  padding: 4px 3px;
  cursor: default;
  transition: all 0.2s ease;
  user-select: none;
  flex-shrink: 0;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.4);
  overflow: hidden;
}

.game-card--small {
  width: 44px;
  height: 66px;
}

.game-card--playable {
  cursor: pointer;
  border-color: rgba(255, 215, 64, 0.5);
}

.game-card--playable:hover {
  transform: translateY(-8px) scale(1.05);
  border-color: #ffd740;
  box-shadow: 0 8px 20px rgba(255, 215, 64, 0.3);
  z-index: 10;
}

.game-card--selected {
  transform: translateY(-12px);
  border-color: #ffd740 !important;
  box-shadow: 0 0 0 2px #ffd740, 0 8px 20px rgba(255, 215, 64, 0.4);
}

.game-card--champion {
  border-color: rgba(255, 200, 50, 0.7);
  background: linear-gradient(160deg, #2e2a10 0%, #1e1a08 100%);
}

.game-card--special {
  border-color: rgba(150, 100, 255, 0.5);
  background: linear-gradient(160deg, #1e1530 0%, #120e20 100%);
}

.game-card--commander {
  border-color: rgba(255, 100, 50, 0.6);
  background: linear-gradient(160deg, #2e1010 0%, #1e0808 100%);
  width: 64px;
  height: 96px;
}

.card-strength {
  position: absolute;
  top: 4px;
  left: 4px;
  font-size: 14px;
  font-weight: 900;
  line-height: 1;
  min-width: 18px;
  text-align: center;
}

.strength--normal { color: #fff; }
.strength--buffed { color: #4caf50; }
.strength--debuffed { color: #f44336; }
.strength--champion { color: #ffd740; }
.strength--special { color: #ce93d8; font-size: 16px; }

.card-badge {
  position: absolute;
  top: 4px;
  right: 4px;
  width: 14px;
  height: 14px;
  border-radius: 50%;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  color: rgba(255, 215, 64, 0.8);
}

.card-badge--champion { color: #ffd740; }
.card-badge--commander { color: #ff6d00; }

.card-name {
  font-size: 7px;
  font-weight: 600;
  text-align: center;
  color: rgba(255, 255, 255, 0.75);
  line-height: 1.2;
  max-width: 100%;
  word-break: break-word;
  letter-spacing: 0.02rem;
}

.game-card--small .card-name {
  font-size: 6px;
}

.card-shine {
  position: absolute;
  inset: 0;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.06) 0%, transparent 50%);
  pointer-events: none;
  border-radius: 6px;
}
</style>
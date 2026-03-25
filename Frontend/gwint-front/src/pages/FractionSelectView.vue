<template>
  <section class="fraction-page">
    <div class="fraction-bg" />

    <v-container class="fraction-main fill-height">
      <v-row align="center" justify="center" class="fill-height">
        <v-col cols="12" md="10" lg="8">

          <div class="text-center mb-8">
            <h1 class="fraction-title mb-2">
              Wybierz <span class="title-glow">Frakcję</span>
            </h1>
            <p class="text-medium-emphasis">
              Pokój: <strong data-testid="fraction-room-id" class="text-amber-darken-2">{{ roomId }}</strong>
            </p>
          </div>

          <v-row justify="center">
            <v-col
              v-for="fraction in fractions"
              :key="fraction.id"
              cols="12"
              sm="6"
              md="3"
            >
              <v-card
                  :data-testid="`fraction-card-${fraction.id}`"
                  class="fraction-card pa-5 text-center"
                  :class="{
                  'fraction-card--selected': selectedFraction === fraction.id,
                  'fraction-card--confirmed': isConfirmed,
                  }"
                  rounded="xl"
                  elevation="0"
                  @click="!isConfirmed && selectFraction(fraction.id)"
              >
                <div class="fraction-glow" :style="{ '--glow': fraction.color }" />
                <v-icon
                  :icon="fraction.icon"
                  size="48"
                  :color="fraction.color"
                  class="mb-3"
                />
                <h3 class="fraction-name mb-1">{{ fraction.name }}</h3>
                <p class="text-caption text-medium-emphasis">{{ fraction.description }}</p>

                <v-chip
                  v-if="selectedFraction === fraction.id"
                  color="amber-darken-2"
                  size="x-small"
                  class="mt-3"
                >
                  Wybrana
                </v-chip>
              </v-card>
            </v-col>
          </v-row>

          <div class="mt-8 text-center">
            <template v-if="!isConfirmed">
              <v-btn
                  data-testid="confirm-fraction-button"
                  color="amber-darken-2"
                  size="x-large"
                  class="confirm-btn"
                  :disabled="!selectedFraction"
                  prepend-icon="mdi-check"
                  @click="confirmFraction"
              >
                Potwierdź wybór
              </v-btn>
            </template>

            <template v-else-if="!bothConfirmed">
              <div data-testid="fraction-waiting-status" class="d-flex align-center justify-center ga-3">
                <v-progress-circular size="24" width="2" indeterminate color="amber-darken-2" />
                <span class="text-medium-emphasis">Oczekiwanie na wybór przeciwnika...</span>
              </div>
            </template>

            <template v-else>
              <v-alert type="success" variant="tonal" density="comfortable" class="mb-4">
                Obaj gracze wybrali frakcje! Gra za chwilę się rozpocznie...
              </v-alert>
            </template>
          </div>

          <v-alert
            v-if="error"
            class="mt-4"
            type="error"
            variant="tonal"
            density="comfortable"
          >
            {{ error }}
          </v-alert>

        </v-col>
      </v-row>
    </v-container>
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSignalRStore } from '@/stores/signalr'

const route = useRoute()
const router = useRouter()
const signalRStore = useSignalRStore()

const roomId = route.params.roomId as string
const selectedFraction = ref<number | null>(null)
const isConfirmed = ref(false)
const opponentConfirmed = ref(false)
// NAPRAWKA: zwykły ref zamiast computed (bo zapisywaliśmy do computed = błąd)
const bothConfirmed = ref(false)
const error = ref<string | null>(null)

const fractions = [
  { id: 1, name: 'Nilfgaard', icon: 'mdi-sun', color: 'amber-darken-2', description: 'Imperium południa. Szpiedzy i kontrola.' },
  { id: 2, name: 'Królestwa Północy', icon: 'mdi-shield', color: 'blue-lighten-2', description: 'Waleczna piechota i oblężenie.' },
  { id: 3, name: "Scoia'tael", icon: 'mdi-leaf', color: 'green-lighten-2', description: 'Elfy i krasnoludy. Zwinność i zasadzki.' },
  { id: 4, name: 'Potwory', icon: 'mdi-skull', color: 'red-lighten-2', description: 'Hordy stworów. Siła w liczbach.' },
]

let myFraction: number | null = null
let opponentFraction: number | null = null
// NAPRAWKA: flaga żeby tylko jeden gracz wysłał StartGame
let gameStartInitiated = false

onMounted(() => {
  if (!signalRStore.isConnected) {
    router.push({ name: 'lobby' })
    return
  }

  signalRStore.roomConnection?.on('FractionSelected', (fraction: number) => {
    opponentFraction = fraction
    opponentConfirmed.value = true
    checkBothReady()
  })
})

onUnmounted(() => {
  signalRStore.roomConnection?.off('FractionSelected')
})

function selectFraction(id: number) {
  selectedFraction.value = id
}

async function confirmFraction() {
  if (!selectedFraction.value) return
  myFraction = selectedFraction.value
  isConfirmed.value = true

  await signalRStore.roomConnection?.invoke('BroadcastFraction', roomId, selectedFraction.value)

  checkBothReady()
}

async function checkBothReady() {
  if (!myFraction || !opponentFraction) return
  if (gameStartInitiated) return

  bothConfirmed.value = true

  // NAPRAWKA: tylko host (gracz który stworzył pokój, czyli ma roomId w store) startuje grę
  const amIHost = signalRStore.amIHost

  try {
    await signalRStore.connectToGame()

    if (amIHost) {
      // NAPRAWKA: ustawiamy flagę PRZED invoke żeby drugi gracz nie zdążył też wysłać
      gameStartInitiated = true
      await signalRStore.startGame(myFraction, opponentFraction)
    }

    await router.push({ name: 'game', params: { roomId } })
  } catch (e) {
    error.value = 'Nie można rozpocząć gry.'
    gameStartInitiated = false
  }
}
</script>

<style scoped>
.fraction-page {
  position: relative;
  min-height: 100vh;
  display: flex;
  align-items: center;
}

.fraction-bg {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(ellipse at 15% 40%, rgba(255, 111, 0, 0.08) 0%, transparent 55%),
    radial-gradient(ellipse at 85% 60%, rgba(255, 215, 64, 0.06) 0%, transparent 50%);
}

.fraction-main {
  position: relative;
  z-index: 1;
  padding-top: 80px;
}

.fraction-title {
  font-size: clamp(2rem, 4vw, 3.5rem);
  font-weight: 900;
  letter-spacing: 0.1rem;
}

.title-glow {
  background: linear-gradient(135deg, #ffd740 0%, #ff6d00 50%, #ffd740 100%);
  background-size: 200% auto;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  animation: shimmer 3s linear infinite;
}

.fraction-card {
  background: rgba(255, 255, 255, 0.03) !important;
  border: 1px solid rgba(255, 255, 255, 0.08);
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.fraction-card:hover { transform: translateY(-6px); border-color: rgba(255, 215, 64, 0.2) !important; }
.fraction-card--selected { border-color: rgba(255, 215, 64, 0.5) !important; background: rgba(255, 215, 64, 0.06) !important; transform: translateY(-4px); }
.fraction-card--confirmed { cursor: default; opacity: 0.7; }
.fraction-card--selected.fraction-card--confirmed { opacity: 1; }

.fraction-glow {
  position: absolute;
  inset: 0;
  background: radial-gradient(circle at 50% 0%, var(--glow), transparent 70%);
  opacity: 0;
  transition: opacity 0.3s;
  pointer-events: none;
}

.fraction-card--selected .fraction-glow,
.fraction-card:hover .fraction-glow { opacity: 0.15; }

.fraction-name { font-size: 1rem; font-weight: 700; }
.confirm-btn { font-weight: 700; letter-spacing: 0.05rem; }

@keyframes shimmer {
  0% { background-position: 0% center; }
  100% { background-position: 200% center; }
}
</style>
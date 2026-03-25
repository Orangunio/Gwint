<template>
  <section class="lobby-page">
    <div class="lobby-bg" />

    <AppHeader />

    <v-main class="lobby-main">
      <v-container class="fill-height">
        <v-row align="center" class="fill-height" justify="center">
          <v-col cols="12" md="8" lg="6">

            <!-- Brak logowania -->
            <v-card v-if="!playerStore.isLoggedIn" class="gwint-card text-center pa-8" rounded="xl" elevation="0">
              <v-icon icon="mdi-lock" size="48" color="amber-darken-2" class="mb-4" />
              <h2 class="mb-3">Wymagane logowanie</h2>
              <p class="text-medium-emphasis mb-6">Aby grać, musisz się zalogować.</p>
              <v-btn color="amber-darken-2" :to="{ name: 'login' }">
                Zaloguj się
              </v-btn>
            </v-card>

            <template v-else>

              <!-- Oczekiwanie w pokoju -->
              <v-card
                v-if="phase === 'waiting'"
                class="gwint-card pa-8 text-center"
                rounded="xl"
                elevation="0"
              >
                <div class="card-glow" />
                <v-icon icon="mdi-sword-cross" size="48" color="amber-darken-2" class="mb-4" />
                <h2 class="gwint-title mb-2">
                  Pokój:
                  <span data-testid="room-code" class="text-amber-darken-2">{{ signalRStore.roomId }}</span>
                </h2>
                <p class="text-medium-emphasis mb-2">Udostępnij ten kod znajomemu</p>

                <v-btn
                  variant="outlined"
                  color="amber-darken-2"
                  size="small"
                  class="mb-6"
                  prepend-icon="mdi-content-copy"
                  @click="copyRoomId"
                >
                  Kopiuj kod
                </v-btn>

                <v-divider class="mb-6" />

                <div class="waiting-players mb-6">
                  <div
                    v-for="i in 2"
                    :key="i"
                    class="player-slot"
                    :class="{ 'player-slot--filled': i === 1 || signalRStore.isRoomReady }"
                  >
                    <v-icon
                      :icon="i === 1 || signalRStore.isRoomReady ? 'mdi-account-check' : 'mdi-account-clock'"
                      :color="i === 1 || signalRStore.isRoomReady ? 'green' : 'grey'"
                      size="28"
                    />
                    <span class="ml-2">
                      {{ i === 1 ? playerStore.displayName : (signalRStore.isRoomReady ? 'Przeciwnik dołączył!' : 'Oczekiwanie...') }}
                    </span>
                  </div>
                </div>

                <v-btn
                    data-testid="start-room-game-button"
                    v-if="signalRStore.isRoomReady"
                    color="amber-darken-2"
                    size="x-large"
                    class="gwint-btn mb-4"
                    prepend-icon="mdi-play"
                    :loading="isStarting"
                    @click="startGame"
                >
                  Zacznij grę!
                </v-btn>

                <div v-else class="d-flex align-center justify-center ga-2 text-medium-emphasis">
                  <v-progress-circular size="20" width="2" indeterminate color="amber-darken-2" />
                  <span class="text-body-2">Czekanie na gracza...</span>
                </div>

                <v-btn
                    data-testid="leave-room-button"
                    variant="text"
                    size="small"
                    class="mt-4"
                    @click="leaveRoom"
                >
                  Opuść pokój
                </v-btn>
              </v-card>

              <!-- Wybór akcji (główny ekran lobby) -->
              <template v-else>
                <div class="text-center mb-8">
                  <h1 class="lobby-title mb-2">
                    <span class="title-glow">Lobby</span>
                  </h1>
                  <p class="text-medium-emphasis">
                    Witaj, <strong>{{ playerStore.displayName }}</strong>! Stwórz lub dołącz do pokoju.
                  </p>
                </div>

                <v-row>
                  <!-- Stwórz pokój -->
                  <v-col cols="12" sm="6">
                    <v-card
                        data-testid="create-room-card"
                        class="gwint-card action-card pa-6"
                        rounded="xl"
                        elevation="0"
                        @click="handleCreate"
                    >
                      <div class="card-glow" />
                      <div class="text-center">
                        <v-icon icon="mdi-plus-circle" size="48" color="amber-darken-2" class="mb-4" />
                        <h3 class="mb-2">Stwórz pokój</h3>
                        <p class="text-body-2 text-medium-emphasis">
                          Wygeneruj unikalny kod i zaproś znajomego.
                        </p>
                      </div>
                    </v-card>
                  </v-col>

                  <!-- Dołącz do pokoju -->
                  <v-col cols="12" sm="6">
                    <v-card
                        data-testid="join-room-card"
                        class="gwint-card action-card pa-6"
                        rounded="xl"
                        elevation="0"
                        :class="{ 'action-card--open': phase === 'join' }"
                        @click="phase = 'join'"
                    >
                      <div class="card-glow" />
                      <div class="text-center">
                        <v-icon icon="mdi-login" size="48" color="blue-lighten-2" class="mb-4" />
                        <h3 class="mb-2">Dołącz do pokoju</h3>
                        <p class="text-body-2 text-medium-emphasis">
                          Wpisz kod pokoju od znajomego.
                        </p>
                      </div>
                    </v-card>
                  </v-col>
                </v-row>

                <!-- Formularz dołączania -->
                <v-expand-transition>
                  <v-card
                      data-testid="join-room-form"
                      v-if="phase === 'join'"
                      class="gwint-card mt-4 pa-6"
                      rounded="xl"
                      elevation="0"
                  >
                    <h3 class="mb-4">Wpisz kod pokoju</h3>
                    <v-text-field
                      v-model="joinCode"
                      label="Kod pokoju (6 znaków)"
                      variant="outlined"
                      maxlength="6"
                      class="mb-4"
                      :disabled="isJoining"
                      @keyup.enter="handleJoin"
                    />
                    <div class="d-flex ga-3">
                      <v-btn
                          data-testid="join-submit-button"
                          color="blue-lighten-2"
                          :loading="isJoining"
                          :disabled="joinCode.length < 6"
                          @click="handleJoin"
                      >
                        Dołącz
                      </v-btn>
                      <v-btn data-testid="join-cancel-button" variant="text" @click="phase = 'menu'">Anuluj</v-btn>
                    </div>
                  </v-card>
                </v-expand-transition>

                <!-- Error -->
                <v-alert
                  v-if="signalRStore.error"
                  class="mt-4"
                  type="error"
                  variant="tonal"
                  density="comfortable"
                >
                  {{ signalRStore.error }}
                </v-alert>
              </template>
            </template>

          </v-col>
        </v-row>
      </v-container>
    </v-main>

    <AppFooter />
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppFooter from '@/components/layout/AppFooter.vue'
import { usePlayerStore } from '@/stores/player'
import { useSignalRStore } from '@/stores/signalr'
import { useAppStore } from '@/stores/app'

const router = useRouter()
const playerStore = usePlayerStore()
const signalRStore = useSignalRStore()
const appStore = useAppStore()

type Phase = 'menu' | 'join' | 'waiting'
const phase = ref<Phase>('menu')
const joinCode = ref('')
const isJoining = ref(false)
const isStarting = ref(false)

onMounted(async () => {
  if (!playerStore.isLoggedIn) return
  await signalRStore.connectToRoom()

  // Nasłuchuj na GameStarted z RoomHub → przejdź do wyboru frakcji
  signalRStore.roomConnection?.on('GameStarted', async (roomId: string) => {
    await router.push({ name: 'fraction-select', params: { roomId } })
  })
})

onUnmounted(() => {
  signalRStore.roomConnection?.off('GameStarted')
})

async function handleCreate() {
  try {
    const roomId = await signalRStore.createRoom(playerStore.displayName)
    phase.value = 'waiting'
    appStore.addNotification(`Pokój ${roomId} stworzony!`)
  } catch {
    signalRStore.error = 'Nie można stworzyć pokoju.'
  }
}

async function handleJoin() {
  if (joinCode.value.length < 6) return
  isJoining.value = true
  try {
    await signalRStore.joinRoom(joinCode.value.toUpperCase(), playerStore.displayName)
    phase.value = 'waiting'
  } catch {
    signalRStore.error = 'Nie można dołączyć do pokoju. Sprawdź kod.'
  } finally {
    isJoining.value = false
  }
}

async function startGame() {
  if (!signalRStore.roomId) return
  isStarting.value = true
  try {
    await signalRStore.startRoomGame(signalRStore.roomId)
  } catch {
    signalRStore.error = 'Nie można rozpocząć gry.'
    isStarting.value = false
  }
}

function leaveRoom() {
  signalRStore.roomId = null
  signalRStore.isRoomReady = false
  phase.value = 'menu'
}

function copyRoomId() {
  if (signalRStore.roomId) {
    navigator.clipboard.writeText(signalRStore.roomId)
    appStore.addNotification('Kod pokoju skopiowany!')
  }
}
</script>

<style scoped>
.lobby-page {
  position: relative;
  min-height: 100vh;
}

.lobby-bg {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(ellipse at 20% 30%, rgba(255, 111, 0, 0.07) 0%, transparent 55%),
    radial-gradient(ellipse at 80% 70%, rgba(255, 215, 64, 0.06) 0%, transparent 50%);
}

.lobby-main {
  position: relative;
  z-index: 1;
}

.lobby-title {
  font-size: clamp(2.5rem, 5vw, 4rem);
  font-weight: 900;
  letter-spacing: 0.3rem;
}

.title-glow {
  background: linear-gradient(135deg, #ffd740 0%, #ff6d00 50%, #ffd740 100%);
  background-size: 200% auto;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  animation: shimmer 3s linear infinite;
}

.gwint-card {
  background: rgba(255, 255, 255, 0.03) !important;
  border: 1px solid rgba(255, 255, 255, 0.07);
  position: relative;
  overflow: hidden;
}

.card-glow {
  position: absolute;
  inset: 0;
  background: radial-gradient(circle at 50% 0%, rgba(255, 215, 64, 0.1), transparent 70%);
  pointer-events: none;
}

.action-card {
  cursor: pointer;
  transition: all 0.3s ease;
}

.action-card:hover {
  transform: translateY(-4px);
  border-color: rgba(255, 215, 64, 0.25) !important;
}

.gwint-title {
  font-size: 1.4rem;
  font-weight: 700;
}

.gwint-btn {
  font-weight: 700;
  letter-spacing: 0.05rem;
}

.waiting-players {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.player-slot {
  display: flex;
  align-items: center;
  padding: 12px 16px;
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.07);
  color: rgba(255, 255, 255, 0.4);
}

.player-slot--filled {
  color: rgba(255, 255, 255, 0.9);
  border-color: rgba(100, 200, 100, 0.3);
}

@keyframes shimmer {
  0% { background-position: 0% center; }
  100% { background-position: 200% center; }
}
</style>
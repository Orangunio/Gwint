<template>
  <div class="game-view">

    <!-- Loading -->
    <div v-if="!signalRStore.game" class="game-loading">
      <v-progress-circular size="60" width="3" indeterminate color="amber-darken-2" />
      <p class="mt-4 text-medium-emphasis">Ładowanie gry...</p>
    </div>

    <template v-else>
      <!-- ═══ TOPBAR ═══ -->
      <div class="game-topbar">
        <div class="player-info player-info--opponent">
          <v-icon icon="mdi-account" size="18" color="red-lighten-2" />
          <span>{{ signalRStore.opponentPlayer?.login ?? 'Przeciwnik' }}</span>
          <div class="round-gems">
            <div
              v-for="i in 2"
              :key="i"
              class="round-gem"
              :class="{ 'round-gem--won': opponentRoundsWon >= i }"
            />
          </div>
        </div>

        <div class="game-round-info">
          <span class="round-label">Runda</span>
          <v-chip color="amber-darken-2" size="small" variant="tonal">
            {{ currentRound }}
          </v-chip>
        </div>

        <div class="player-info player-info--me">
          <div class="round-gems">
            <div
              v-for="i in 2"
              :key="i"
              class="round-gem"
              :class="{ 'round-gem--won': myRoundsWon >= i }"
            />
          </div>
          <span>{{ signalRStore.myPlayer?.login ?? 'Ty' }}</span>
          <v-icon icon="mdi-account" size="18" color="amber-darken-2" />
        </div>
      </div>

      <!-- ═══ PLANSZA ═══ -->
      <div class="game-board">

        <!-- Rzędy przeciwnika (odwrócone — najdalszy rząd na górze) -->
        <div class="board-side board-side--opponent">
          <div class="side-score">
            <div class="total-score" :class="{ 'total-score--leading': opponentLeading }">
              {{ opponentTotal }}
            </div>
            <div class="text-caption text-medium-emphasis">pkt</div>
          </div>

          <div class="rows-stack">
            <GameRow
              v-for="(row, idx) in opponentRows"
              :key="idx"
              :cards="row"
              :row-index="2 - idx"
              :row-score="opponentRowScores[2 - idx]"
              :frost-active="signalRStore.game.board?.frostActive"
              :fog-active="signalRStore.game.board?.fogActive"
              :rain-active="signalRStore.game.board?.rainActive"
            />
          </div>
        </div>

        <!-- Separator ze statusem tury -->
        <div class="board-separator">
          <div class="turn-indicator" :class="{ 'turn-indicator--my': signalRStore.myTurn }">
            <v-icon
              :icon="signalRStore.myTurn ? 'mdi-sword' : 'mdi-timer-sand'"
              size="20"
              :color="signalRStore.myTurn ? 'amber-darken-2' : 'grey'"
            />
            <span>{{ signalRStore.myTurn ? 'Twoja tura' : 'Tura przeciwnika' }}</span>
          </div>
          <!-- Pogoda aktywna -->
          <div class="weather-bar">
            <div v-if="board?.frostActive" class="weather-chip">
              <v-icon icon="mdi-snowflake" size="14" color="blue-lighten-3" />
            </div>
            <div v-if="board?.fogActive" class="weather-chip">
              <v-icon icon="mdi-weather-fog" size="14" color="blue-lighten-3" />
            </div>
            <div v-if="board?.rainActive" class="weather-chip">
              <v-icon icon="mdi-weather-pouring" size="14" color="blue-lighten-3" />
            </div>
          </div>
        </div>

        <!-- Rzędy gracza -->
        <div class="board-side board-side--me">
          <div class="side-score">
            <div class="total-score" :class="{ 'total-score--leading': myLeading }">
              {{ myTotal }}
            </div>
            <div class="text-caption text-medium-emphasis">pkt</div>
          </div>

          <div class="rows-stack">
            <GameRow
              v-for="(row, idx) in myRows"
              :key="idx"
              :cards="row"
              :row-index="idx"
              :row-score="myRowScores[idx]"
              :can-drop="signalRStore.myTurn && !!selectedCard && canPlayInRow(selectedCard, idx)"
              :frost-active="board?.frostActive"
              :fog-active="board?.fogActive"
              :rain-active="board?.rainActive"
              @drop="playInRow(selectedCard!, idx)"
            />
          </div>
        </div>
      </div>

      <!-- ═══ RĘKA GRACZA ═══ -->
      <div class="player-hand-area">
        <div class="hand-info">
          <span class="text-caption text-medium-emphasis">
            Karty w ręce: {{ signalRStore.myHand.length }}
          </span>
          <span class="text-caption text-medium-emphasis ml-4">
            Cmentarz: {{ signalRStore.myGraveyard.length }}
          </span>
        </div>

        <div class="hand-cards">
          <!-- Karta dowódcy -->
          <div class="commander-slot">
            <GameCard
              v-if="signalRStore.myCommander"
              :card="signalRStore.myCommander"
              :playable="signalRStore.myTurn && !commanderUsed"
              :selected="selectedCard?.id === signalRStore.myCommander?.id"
              @play="selectCard(signalRStore.myCommander!)"
            />
            <div v-else class="commander-empty">
              <v-icon icon="mdi-account-off" size="24" color="grey-darken-1" />
            </div>
          </div>

          <div class="hand-divider" />

          <!-- Karty w ręce -->
          <div class="hand-scroll">
            <GameCard
              v-for="card in signalRStore.myHand"
              :key="card.id"
              :card="card"
              :playable="signalRStore.myTurn"
              :selected="selectedCard?.id === card.id"
              @play="selectCard(card)"
            />
          </div>
        </div>

        <!-- Akcje -->
        <div class="hand-actions">
          <v-btn
            v-if="selectedCard"
            color="amber-darken-2"
            size="small"
            prepend-icon="mdi-close"
            variant="text"
            @click="selectedCard = null"
          >
            Odznacz
          </v-btn>

          <v-btn
            v-if="signalRStore.myTurn"
            color="grey"
            size="small"
            variant="outlined"
            prepend-icon="mdi-flag"
            :disabled="!!selectedCard"
            @click="pass"
          >
            Pasuj
          </v-btn>
        </div>
      </div>

      <!-- ═══ DIALOG: Wybór rzędu (Zwinność) ═══ -->
      <v-dialog v-model="showAgilityDialog" max-width="400" persistent>
        <v-card class="gwint-dialog" rounded="xl" elevation="0">
          <v-card-title class="pa-6 pb-2">Wybierz rząd</v-card-title>
          <v-card-text class="pa-6">
            <p class="text-medium-emphasis mb-4">
              Karta ma umiejętność <strong>Zwinność</strong>. Wybierz rząd do zagrania:
            </p>
            <div class="d-flex ga-3 justify-center">
              <v-btn
                v-for="row in agilityRows"
                :key="row.value"
                :color="row.color"
                variant="outlined"
                @click="resolveAgility(row.value)"
              >
                {{ row.label }}
              </v-btn>
            </div>
          </v-card-text>
        </v-card>
      </v-dialog>

      <!-- ═══ DIALOG: Wskrzeszenie ═══ -->
      <v-dialog v-model="showResurrectionDialog" max-width="500" persistent>
        <v-card class="gwint-dialog" rounded="xl" elevation="0">
          <v-card-title class="pa-6 pb-2">Wskrzeszenie</v-card-title>
          <v-card-text class="pa-6">
            <p class="text-medium-emphasis mb-4">Wybierz kartę z cmentarza do wskrzeszenia:</p>
            <div class="d-flex flex-wrap ga-3 justify-center">
              <GameCard
                v-for="card in revivableCards"
                :key="card.id"
                :card="card"
                :playable="true"
                @play="resolveResurrection(card)"
              />
            </div>
          </v-card-text>
          <v-card-actions class="pa-4">
            <v-btn variant="text" @click="signalRStore.pendingAction = null">Anuluj</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>

      <!-- ═══ DIALOG: Manekin (Decoy) ═══ -->
      <v-dialog v-model="showDecoyDialog" max-width="600" persistent>
        <v-card class="gwint-dialog" rounded="xl" elevation="0">
          <v-card-title class="pa-6 pb-2">Manekin do ćwiczeń</v-card-title>
          <v-card-text class="pa-6">
            <p class="text-medium-emphasis mb-4">Wybierz kartę z planszy do zabrania na rękę:</p>
            <div class="d-flex flex-wrap ga-3 justify-center">
              <GameCard
                v-for="card in myBoardCards"
                :key="card.id"
                :card="card"
                :playable="!card.isChampion"
                @play="resolveDecoy(card)"
              />
            </div>
          </v-card-text>
          <v-card-actions class="pa-4">
            <v-btn variant="text" @click="signalRStore.pendingAction = null">Anuluj</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>

    </template>
  </div>
  <div style="position:fixed;top:0;left:0;z-index:9999;background:black;color:lime;font-size:11px;padding:8px;max-width:400px;max-height:300px;overflow:auto">
  <div>isGameConnected: {{ signalRStore.isGameConnected }}</div>
  <div>gameConnectionId: {{ signalRStore.gameConnectionId }}</div>
  <div>game is null: {{ signalRStore.game === null }}</div>
  <div>game.player1: {{ signalRStore.game?.player1?.connectionId }}</div>
  <div>game.player2: {{ signalRStore.game?.player2?.connectionId }}</div>
  <div>amIPlayer1: {{ signalRStore.amIPlayer1 }}</div>
  <div>amIHost: {{ signalRStore.amIHost }}</div>
  <div>myTurn: {{ signalRStore.myTurn }}</div>
  <div>myHand.length: {{ signalRStore.myHand.length }}</div>
</div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSignalRStore, type GameCard as GameCardType } from '@/stores/signalr'
import GameCard from '@/components/game/GameCard.vue'
import GameRow from '@/components/game/GameRow.vue'

const route = useRoute()
const router = useRouter()
const signalRStore = useSignalRStore()
const roomId = route.params.roomId as string

const selectedCard = ref<GameCardType | null>(null)
const commanderUsed = ref(false)
const currentRound = ref(1)

// ─── COMPUTED ───────────────────────────────────────────────────────

const board = computed(() => signalRStore.game?.board)
const amIP1 = computed(() => signalRStore.amIPlayer1)

const myRows = computed(() => {
  if (!board.value) return [[], [], []]
  return amIP1.value
    ? [board.value.player1FirstCardRow, board.value.player1SecondCardRow, board.value.player1ThirdCardRow]
    : [board.value.player2FirstCardRow, board.value.player2SecondCardRow, board.value.player2ThirdCardRow]
})

const opponentRows = computed(() => {
  if (!board.value) return [[], [], []]
  // Odwrócone — rząd oblężenia na górze, piechota przy linii środkowej
  return amIP1.value
    ? [board.value.player2ThirdCardRow, board.value.player2SecondCardRow, board.value.player2FirstCardRow]
    : [board.value.player1ThirdCardRow, board.value.player1SecondCardRow, board.value.player1FirstCardRow]
})

const myRowScores = computed(() => {
  const scores = board.value?.rowScores
  if (!scores || scores.length === 0) return [0, 0, 0]
  
  const p = amIP1.value ? 0 : 1
  return [
    scores[p]?.[0] ?? 0,
    scores[p]?.[1] ?? 0,
    scores[p]?.[2] ?? 0
  ]
})

const opponentRowScores = computed(() => {
  const scores = board.value?.rowScores
  if (!scores || scores.length === 0) return [0, 0, 0]
  
  const p = amIP1.value ? 1 : 0
  return [
    scores[p]?.[0] ?? 0,
    scores[p]?.[1] ?? 0,
    scores[p]?.[2] ?? 0
  ]
})

const myTotal = computed(() => myRowScores.value.reduce((a, b) => a + b, 0))
const opponentTotal = computed(() => opponentRowScores.value.reduce((a, b) => a + b, 0))
const myLeading = computed(() => myTotal.value > opponentTotal.value)
const opponentLeading = computed(() => opponentTotal.value > myTotal.value)

const myRoundsWon = computed(() =>
  amIP1.value
    ? (signalRStore.game?.player1RoundsWon ?? 0)
    : (signalRStore.game?.player2RoundsWon ?? 0),
)
const opponentRoundsWon = computed(() =>
  amIP1.value
    ? (signalRStore.game?.player2RoundsWon ?? 0)
    : (signalRStore.game?.player1RoundsWon ?? 0),
)

const myBoardCards = computed(() => [
  ...myRows.value[0],
  ...myRows.value[1],
  ...myRows.value[2],
])

const revivableCards = computed(() =>
  signalRStore.myGraveyard.filter(c => !c.isChampion && !c.isSpecial),
)

// Dialogi
const showAgilityDialog = computed(() => signalRStore.pendingAction === 'agility')
const showResurrectionDialog = computed(() => signalRStore.pendingAction === 'resurrection')
const showDecoyDialog = computed(() => signalRStore.pendingAction === 'decoy')

const agilityRows = [
  { label: 'Piechota', value: 1, color: 'red-lighten-2' },
  { label: 'Dystans', value: 2, color: 'green-lighten-2' },
  { label: 'Oblężenie', value: 3, color: 'blue-lighten-2' },
]

// ─── LOGIKA PLACE ───────────────────────────────────────────────────

// Enum Place z backendu
// 0=bez rzędu, 1=R1, 2=R2, 3=R3, 12=R1+R2, 23=R2+R3, 13=R1+R3, 123=wszystkie
function canPlayInRow(card: GameCardType, rowIdx: number): boolean {
  if (card.isSpecial) return false // specjalne grajesz bezpośrednio
  const p = card.place
  const row = rowIdx + 1 // backend: 1,2,3
  return (
    p === row ||
    (p === 12 && (row === 1 || row === 2)) ||
    (p === 23 && (row === 2 || row === 3)) ||
    (p === 13 && (row === 1 || row === 3)) ||
    p === 123
  )
}

// ─── AKCJE ──────────────────────────────────────────────────────────

function selectCard(card: GameCardType) {
  if (!signalRStore.myTurn) return

  if (selectedCard.value?.id === card.id) {
    selectedCard.value = null
    return
  }

  selectedCard.value = card

  // Karta specjalna — zagraj od razu (nie wymaga wyboru rzędu)
  if (card.isSpecial) {
    playCardDirectly(card)
    return
  }

  // Karta z jednym możliwym rzędem — zagraj od razu
  const validRows = [0, 1, 2].filter(i => canPlayInRow(card, i))
  if (validRows.length === 1 && card.ability !== 3 /* zwinność */) {
    playInRow(card, validRows[0])
    return
  }

  // Karta dowódcy — zagraj od razu
  if (card.isCommander) {
    playCardDirectly(card)
    return
  }
}

async function playInRow(card: GameCardType, rowIdx: number) {
  if (!signalRStore.myTurn || !card) return
  if (!canPlayInRow(card, rowIdx)) return

  selectedCard.value = null
  if (card.isCommander) commanderUsed.value = true

  try {
    await signalRStore.playCard(card, rowIdx + 1)
  } catch (e) {
    console.error('Błąd zagrania karty', e)
  }
}

async function playCardDirectly(card: GameCardType) {
  selectedCard.value = null
  if (card.isCommander) commanderUsed.value = true
  try {
    await signalRStore.playCard(card)
  } catch (e) {
    console.error('Błąd zagrania karty', e)
  }
}

async function pass() {
  try {
    await signalRStore.playerPass()
  } catch (e) {
    console.error('Błąd pasowania', e)
  }
}

async function resolveAgility(row: number) {
  if (!signalRStore.pendingCardId) return
  await signalRStore.resolveAgility(signalRStore.pendingCardId, row)
}

async function resolveResurrection(card: GameCardType) {
  await signalRStore.resolveResurrection(card.id)
}

async function resolveDecoy(card: GameCardType) {
  await signalRStore.resolveDecoy(card.id)
}

// ─── LIFECYCLE ──────────────────────────────────────────────────────

onMounted(async () => {
  if (!signalRStore.isGameConnected) {
    await router.push({ name: 'lobby' })
    return
  }
 
  if (signalRStore.amIHost) {
    await new Promise(resolve => setTimeout(resolve, 500))
    await signalRStore.chooseFirstPlayer()
  }
})

onUnmounted(async () => {
  await signalRStore.disconnect()
})
</script>

<style scoped>
.game-view {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background: #0d1a0d;
  overflow: hidden;
  color: white;
}

/* ─── Topbar ─── */
.game-topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 16px;
  background: rgba(0, 0, 0, 0.5);
  border-bottom: 1px solid rgba(255, 215, 64, 0.15);
  flex-shrink: 0;
  height: 48px;
}

.player-info {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.85rem;
  font-weight: 600;
}

.round-gems {
  display: flex;
  gap: 4px;
}

.round-gem {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.round-gem--won {
  background: #ffd740;
  border-color: #ffa000;
}

.game-round-info {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.75rem;
  color: rgba(255, 255, 255, 0.5);
  text-transform: uppercase;
  letter-spacing: 0.1rem;
}

/* ─── Plansza ─── */
.game-board {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  padding: 4px 12px;
  gap: 2px;
}

.board-side {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 0;
}

.side-score {
  min-width: 48px;
  text-align: center;
  flex-shrink: 0;
}

.total-score {
  font-size: 1.8rem;
  font-weight: 900;
  color: rgba(255, 255, 255, 0.3);
  line-height: 1;
}

.total-score--leading {
  color: #ffd740;
  text-shadow: 0 0 12px rgba(255, 215, 64, 0.4);
}

.rows-stack {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

/* ─── Separator ─── */
.board-separator {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 6px 12px;
  background: rgba(0, 0, 0, 0.3);
  border-top: 1px solid rgba(255, 215, 64, 0.1);
  border-bottom: 1px solid rgba(255, 215, 64, 0.1);
  flex-shrink: 0;
}

.turn-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.8rem;
  color: rgba(255, 255, 255, 0.4);
}

.turn-indicator--my {
  color: #ffd740;
  font-weight: 700;
}

.weather-bar {
  display: flex;
  gap: 6px;
}

.weather-chip {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: rgba(100, 180, 255, 0.15);
  border: 1px solid rgba(100, 180, 255, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
}

/* ─── Ręka gracza ─── */
.player-hand-area {
  background: rgba(0, 0, 0, 0.5);
  border-top: 1px solid rgba(255, 215, 64, 0.15);
  padding: 8px 12px;
  flex-shrink: 0;
}

.hand-info {
  margin-bottom: 6px;
}

.hand-cards {
  display: flex;
  align-items: flex-end;
  gap: 8px;
  overflow: hidden;
}

.commander-slot {
  flex-shrink: 0;
}

.commander-empty {
  width: 64px;
  height: 96px;
  border-radius: 6px;
  border: 1px dashed rgba(255, 255, 255, 0.15);
  display: flex;
  align-items: center;
  justify-content: center;
}

.hand-divider {
  width: 1px;
  height: 80px;
  background: rgba(255, 215, 64, 0.2);
  flex-shrink: 0;
}

.hand-scroll {
  display: flex;
  gap: 4px;
  overflow-x: auto;
  padding: 8px 4px 4px;
  scrollbar-width: thin;
  scrollbar-color: rgba(255, 215, 64, 0.2) transparent;
  align-items: flex-end;
}

.hand-scroll::-webkit-scrollbar {
  height: 3px;
}

.hand-scroll::-webkit-scrollbar-thumb {
  background: rgba(255, 215, 64, 0.2);
  border-radius: 2px;
}

.hand-actions {
  margin-top: 8px;
  display: flex;
  gap: 8px;
}

/* ─── Loading ─── */
.game-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100vh;
  background: #0d1a0d;
}

/* ─── Dialogi ─── */
.gwint-dialog {
  background: rgba(20, 20, 15, 0.95) !important;
  border: 1px solid rgba(255, 215, 64, 0.2) !important;
  backdrop-filter: blur(12px);
}
</style>
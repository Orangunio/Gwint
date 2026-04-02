<template>
  <div class="game-view">

    <!-- Loading -->
    <div v-if="!signalRStore.game" class="game-loading">
      <v-progress-circular size="60" width="3" indeterminate color="amber-darken-2" />
      <p class="mt-4 text-medium-emphasis">Ładowanie gry...</p>
    </div>

    <template v-else>

      <!-- ═══════════════════════════════════════════════════════ -->
      <!-- GÓRA — pasek przeciwnika                               -->
      <!-- ═══════════════════════════════════════════════════════ -->
      <div class="opponent-bar">
        <!-- Dowódca przeciwnika (widoczny) -->
        <div class="opponent-commander-slot">
          <div class="slot-label">Dowódca</div>
          <GameCard
            v-if="opponentCommander"
            :card="opponentCommander"
            :playable="false"
            class="commander-card"
          />
          <div v-else class="card-placeholder">
            <v-icon icon="mdi-account-off" size="20" color="grey-darken-1" />
          </div>
          <div class="slot-name">{{ signalRStore.opponentPlayer?.login ?? 'Przeciwnik' }}</div>
        </div>

        <!-- Karty na ręce przeciwnika (rewers) -->
        <div class="opponent-hand-area">
          <div class="slot-label">Ręka ({{ opponentHandCount }})</div>
          <div class="opponent-hand-cards">
            <div
              v-for="i in Math.min(opponentHandCount, 12)"
              :key="i"
              class="card-back"
              :style="{ transform: `rotate(${(i - Math.min(opponentHandCount, 12) / 2) * 3}deg) translateY(${Math.abs(i - Math.min(opponentHandCount, 12) / 2) * 2}px)` }"
            />
          </div>
        </div>

        <!-- Talia i cmentarz przeciwnika -->
        <div class="opponent-decks">
          <div class="deck-pile">
            <div class="deck-icon-stack">
              <div v-for="i in Math.min(5, opponentDeckCount)" :key="i" class="mini-card-back"
                :style="{ bottom: `${i * 1.5}px`, right: `${i * 0.5}px` }" />
            </div>
            <div class="deck-badge">{{ opponentDeckCount }}</div>
            <div class="slot-label mt-1">Talia</div>
          </div>
          <div class="deck-pile">
            <v-icon icon="mdi-grave-stone" size="28" color="grey-darken-1" />
            <div class="deck-badge graveyard-badge">{{ opponentGraveyardCount }}</div>
            <div class="slot-label mt-1">Cmentarz</div>
          </div>
        </div>
      </div>

      <!-- ═══════════════════════════════════════════════════════ -->
      <!-- GŁÓWNA CZĘŚĆ — lewa belka + plansza                    -->
      <!-- ═══════════════════════════════════════════════════════ -->
      <div class="game-main">

        <!-- LEWA BELKA — score, rundy, pogoda -->
        <div class="side-panel">

          <!-- Wynik i rundy przeciwnika -->
          <div class="score-block score-block--opponent">
            <div class="score-gems">
              <div v-for="i in 2" :key="i"
                class="score-gem" :class="{ 'score-gem--won': opponentRoundsWon >= i }" />
            </div>
            <div class="score-value" :class="{ 'score-value--leading': opponentLeading }">
              {{ opponentTotal }}
            </div>
            <div class="score-sub">pkt</div>
          </div>

          <!-- Efekty pogodowe (centrum belki) -->
          <div class="weather-section">
            <div class="weather-title">Pogoda</div>
            <div class="weather-icons">
              <div v-if="board?.frostActive" class="weather-icon weather-icon--frost"
                title="Mróz">
                <v-icon icon="mdi-snowflake" size="18" />
              </div>
              <div v-if="board?.fogActive" class="weather-icon weather-icon--fog"
                title="Mgła">
                <v-icon icon="mdi-weather-fog" size="18" />
              </div>
              <div v-if="board?.rainActive" class="weather-icon weather-icon--rain"
                title="Deszcz">
                <v-icon icon="mdi-weather-pouring" size="18" />
              </div>
              <div v-if="!board?.frostActive && !board?.fogActive && !board?.rainActive"
                class="weather-none">
                <v-icon icon="mdi-weather-sunny" size="16" color="grey-darken-2" />
              </div>
            </div>

            <!-- Wskaźnik tury -->
            <div class="turn-block" :class="{ 'turn-block--active': signalRStore.myTurn }">
              <v-icon
                :icon="signalRStore.myTurn ? 'mdi-sword' : 'mdi-timer-sand'"
                size="20"
              />
              <span>{{ signalRStore.myTurn ? 'Twoja tura' : 'Przeciwnik' }}</span>
            </div>

            <!-- Runda -->
            <div class="round-indicator">
              <span class="round-label-text">Runda</span>
              <span class="round-number">{{ currentRound }}</span>
            </div>
          </div>

          <!-- Wynik i rundy gracza -->
          <div class="score-block score-block--me">
            <div class="score-value" :class="{ 'score-value--leading': myLeading }">
              {{ myTotal }}
            </div>
            <div class="score-sub">pkt</div>
            <div class="score-gems">
              <div v-for="i in 2" :key="i"
                class="score-gem" :class="{ 'score-gem--won': myRoundsWon >= i }" />
            </div>
          </div>
        </div>

        <!-- PLANSZA GRY — rzędy -->
        <div class="board-area">

          <!-- Rzędy przeciwnika (od góry: oblężenie → dystans → piechota) -->
          <div class="board-rows board-rows--opponent">
            <GameRow
              v-for="(row, idx) in opponentRows"
              :key="'opp-' + idx"
              :cards="row"
              :row-index="2 - idx"
              :row-score="opponentRowScores[2 - idx]"
              :frost-active="board?.frostActive"
              :fog-active="board?.fogActive"
              :rain-active="board?.rainActive"
            />
          </div>

          <!-- Linia środkowa -->
          <div class="board-divider">
            <div class="divider-line" />
            <div class="divider-ornament">⚔</div>
            <div class="divider-line" />
          </div>

          <!-- Rzędy gracza (od góry: piechota → dystans → oblężenie) -->
          <div class="board-rows board-rows--me">
            <GameRow
              v-for="(row, idx) in myRows"
              :key="'my-' + idx"
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

      <!-- ═══════════════════════════════════════════════════════ -->
      <!-- DÓŁ — ręka gracza                                      -->
      <!-- ═══════════════════════════════════════════════════════ -->
      <div class="player-bar">

        <!-- Dowódca gracza (lewa strona) -->
        <div class="player-commander-slot">
          <div class="slot-label">Dowódca</div>
          <GameCard
            v-if="signalRStore.myCommander"
            :card="signalRStore.myCommander"
            :playable="signalRStore.myTurn && !commanderUsed"
            :selected="selectedCard?.id === signalRStore.myCommander?.id"
            class="commander-card"
            @play="selectCard(signalRStore.myCommander!)"
          />
          <div v-else class="card-placeholder">
            <v-icon icon="mdi-account-off" size="20" color="grey-darken-1" />
          </div>
        </div>

        <!-- Karty na ręce (środek) -->
        <div class="player-hand">
          <div class="hand-header">
            <span class="slot-label">Ręka ({{ signalRStore.myHand.length }})</span>
            <div class="hand-actions">
              <v-btn
                v-if="selectedCard"
                color="amber-darken-2"
                size="x-small"
                prepend-icon="mdi-close"
                variant="text"
                @click="selectedCard = null"
              >
                Odznacz
              </v-btn>
              <v-btn
                v-if="signalRStore.myTurn"
                color="grey"
                size="x-small"
                variant="outlined"
                prepend-icon="mdi-flag"
                :disabled="!!selectedCard"
                @click="pass"
              >
                Pasuj
              </v-btn>
            </div>
          </div>

          <div class="hand-cards-scroll">
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

        <!-- Talia i cmentarz gracza (prawa strona) -->
        <div class="player-decks">
          <div class="deck-pile">
            <div class="deck-icon-stack">
              <div v-for="i in Math.min(5, myDeckCount)" :key="i" class="mini-card-stack"
                :style="{ bottom: `${i * 1.5}px`, right: `${i * 0.5}px` }" />
            </div>
            <div class="deck-badge">{{ myDeckCount }}</div>
            <div class="slot-label mt-1">Talia</div>
          </div>
          <div class="deck-pile">
            <v-icon icon="mdi-grave-stone" size="28" color="grey-darken-1" />
            <div class="deck-badge graveyard-badge">{{ myGraveyardCount }}</div>
            <div class="slot-label mt-1">Cmentarz</div>
          </div>
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

    <!-- Debug overlay -->
    <!-- <div style="position:fixed;top:0;left:0;z-index:9999;background:black;color:lime;font-size:11px;padding:8px;max-width:400px;max-height:300px;overflow:auto">
      <div>isGameConnected: {{ signalRStore.isGameConnected }}</div>
      <div>gameConnectionId: {{ signalRStore.gameConnectionId }}</div>
      <div>game is null: {{ signalRStore.game === null }}</div>
      <div>game.player1: {{ signalRStore.game?.player1?.connectionId }}</div>
      <div>game.player2: {{ signalRStore.game?.player2?.connectionId }}</div>
      <div>amIPlayer1: {{ signalRStore.amIPlayer1 }}</div>
      <div>amIHost: {{ signalRStore.amIHost }}</div>
      <div>myTurn: {{ signalRStore.myTurn }}</div>
      <div>myHand.length: {{ signalRStore.myHand.length }}</div>
    </div> -->
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

// Dowódca przeciwnika — zakładamy że store ma opponentCommander lub wyciągamy z game
const opponentCommander = computed(() => {
  if (!signalRStore.game) return null
  return amIP1.value
    ? signalRStore.game.player2CommanderCard ?? null
    : signalRStore.game.player1CommanderCard ?? null
})

const myRows = computed(() => {
  if (!board.value) return [[], [], []]
  return amIP1.value
    ? [board.value.player1FirstCardRow, board.value.player1SecondCardRow, board.value.player1ThirdCardRow]
    : [board.value.player2FirstCardRow, board.value.player2SecondCardRow, board.value.player2ThirdCardRow]
})

const opponentRows = computed(() => {
  if (!board.value) return [[], [], []]
  return amIP1.value
    ? [board.value.player2ThirdCardRow, board.value.player2SecondCardRow, board.value.player2FirstCardRow]
    : [board.value.player1ThirdCardRow, board.value.player1SecondCardRow, board.value.player1FirstCardRow]
})

const myRowScores = computed(() => {
  const scores = board.value?.rowScores
  if (!scores || scores.length === 0) return [0, 0, 0]
  const p = amIP1.value ? 0 : 1
  return [scores[p]?.[0] ?? 0, scores[p]?.[1] ?? 0, scores[p]?.[2] ?? 0]
})

const opponentRowScores = computed(() => {
  const scores = board.value?.rowScores
  if (!scores || scores.length === 0) return [0, 0, 0]
  const p = amIP1.value ? 1 : 0
  return [scores[p]?.[0] ?? 0, scores[p]?.[1] ?? 0, scores[p]?.[2] ?? 0]
})

const myTotal = computed(() => myRowScores.value.reduce((a, b) => a + b, 0))
const opponentTotal = computed(() => opponentRowScores.value.reduce((a, b) => a + b, 0))
const myLeading = computed(() => myTotal.value > opponentTotal.value)
const opponentLeading = computed(() => opponentTotal.value > myTotal.value)

const myRoundsWon = computed(() =>
  amIP1.value ? (signalRStore.game?.player1RoundsWon ?? 0) : (signalRStore.game?.player2RoundsWon ?? 0)
)
const opponentRoundsWon = computed(() =>
  amIP1.value ? (signalRStore.game?.player2RoundsWon ?? 0) : (signalRStore.game?.player1RoundsWon ?? 0)
)

const myBoardCards = computed(() => [...myRows.value[0], ...myRows.value[1], ...myRows.value[2]])
const revivableCards = computed(() => signalRStore.myGraveyard.filter(c => !c.isChampion && !c.isSpecial))

const showAgilityDialog = computed(() => signalRStore.pendingAction === 'agility')
const showResurrectionDialog = computed(() => signalRStore.pendingAction === 'resurrection')
const showDecoyDialog = computed(() => signalRStore.pendingAction === 'decoy')

const agilityRows = [
  { label: 'Piechota', value: 1, color: 'red-lighten-2' },
  { label: 'Dystans', value: 2, color: 'green-lighten-2' },
  { label: 'Oblężenie', value: 3, color: 'blue-lighten-2' },
]

const myDeckCount = computed(() => {
  if (!signalRStore.game) return 0
  return signalRStore.amIPlayer1
    ? (signalRStore.game.player1CardsInDeck?.length ?? 0)
    : (signalRStore.game.player2CardsInDeck?.length ?? 0)
})

const opponentDeckCount = computed(() => {
  if (!signalRStore.game) return 0
  return signalRStore.amIPlayer1
    ? (signalRStore.game.player2CardsInDeck?.length ?? 0)
    : (signalRStore.game.player1CardsInDeck?.length ?? 0)
})

const myGraveyardCount = computed(() => signalRStore.myGraveyard.length)

const opponentGraveyardCount = computed(() => {
  if (!signalRStore.game) return 0
  return signalRStore.amIPlayer1
    ? (signalRStore.game.player2CardsOnDisplay?.length ?? 0)
    : (signalRStore.game.player1CardsOnDisplay?.length ?? 0)
})

const opponentHandCount = computed(() => {
  if (!signalRStore.game) return 0
  return signalRStore.amIPlayer1
    ? (signalRStore.game.player2CardsOnHand?.length ?? 0)
    : (signalRStore.game.player1CardsOnHand?.length ?? 0)
})

// ─── LOGIKA PLACE ───────────────────────────────────────────────────

function canPlayInRow(card: GameCardType, rowIdx: number): boolean {
  if (card.isSpecial) return false
  const p = card.place
  const row = rowIdx + 1
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
  if (selectedCard.value?.id === card.id) { selectedCard.value = null; return }
  selectedCard.value = card
  if (card.isSpecial) { playCardDirectly(card); return }
  const validRows = [0, 1, 2].filter(i => canPlayInRow(card, i))
  if (validRows.length === 1 && card.ability !== 3) { playInRow(card, validRows[0]); return }
  if (card.isCommander) { playCardDirectly(card); return }
}

async function playInRow(card: GameCardType, rowIdx: number) {
  if (!signalRStore.myTurn || !card) return
  if (!canPlayInRow(card, rowIdx)) return
  selectedCard.value = null
  if (card.isCommander) commanderUsed.value = true
  try { await signalRStore.playCard(card, rowIdx + 1) }
  catch (e) { console.error('Błąd zagrania karty', e) }
}

async function playCardDirectly(card: GameCardType) {
  selectedCard.value = null
  if (card.isCommander) commanderUsed.value = true
  try { await signalRStore.playCard(card) }
  catch (e) { console.error('Błąd zagrania karty', e) }
}

async function pass() {
  try { await signalRStore.playerPass() }
  catch (e) { console.error('Błąd pasowania', e) }
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

watch(() => signalRStore.gameConnectionId, (newId) => {
  console.log('gameConnectionId zmienione na:', newId)
})
</script>

<style scoped>
/* ═══════════════════════════════════════════════════════════════════ */
/*  SZKIELET WIDOKU                                                   */
/* ═══════════════════════════════════════════════════════════════════ */

.game-view {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background:
    radial-gradient(ellipse at 50% 0%, rgba(60, 90, 40, 0.5) 0%, transparent 60%),
    radial-gradient(ellipse at 50% 100%, rgba(20, 40, 15, 0.8) 0%, transparent 60%),
    #0d1a0d;
  overflow: hidden;
  color: white;
  font-family: 'Georgia', serif;
}

/* ═══════════════════════════════════════════════════════════════════ */
/*  GÓRA — pasek przeciwnika                                          */
/* ═══════════════════════════════════════════════════════════════════ */

.opponent-bar {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 6px 16px;
  background: linear-gradient(180deg, rgba(0,0,0,0.7) 0%, rgba(0,0,0,0.3) 100%);
  border-bottom: 1px solid rgba(255, 215, 64, 0.12);
  flex-shrink: 0;
  min-height: 110px;
}

.opponent-commander-slot {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  flex-shrink: 0;
  width: 72px;
}

.opponent-hand-area {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.opponent-hand-cards {
  display: flex;
  align-items: flex-end;
  justify-content: center;
  height: 72px;
  position: relative;
}

/* Rewers karty przeciwnika */
.card-back {
  width: 44px;
  height: 64px;
  border-radius: 4px;
  background:
    linear-gradient(135deg, #1a2a1a 0%, #0d1a0d 50%, #1a2a1a 100%);
  border: 1px solid rgba(255, 215, 64, 0.25);
  box-shadow: 0 2px 8px rgba(0,0,0,0.5);
  position: absolute;
  transition: transform 0.2s ease;
  /* Wzór na rewersie */
  background-image:
    repeating-linear-gradient(
      45deg,
      rgba(255, 215, 64, 0.05) 0px,
      rgba(255, 215, 64, 0.05) 1px,
      transparent 1px,
      transparent 8px
    );
}

.opponent-decks {
  display: flex;
  flex-direction: column;
  gap: 8px;
  align-items: center;
  flex-shrink: 0;
  width: 72px;
}

/* ═══════════════════════════════════════════════════════════════════ */
/*  ŚRODEK — lewa belka + plansza                                     */
/* ═══════════════════════════════════════════════════════════════════ */

.game-main {
  flex: 1;
  display: flex;
  overflow: hidden;
  min-height: 0;
}

/* LEWA BELKA */
.side-panel {
  width: 88px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  background: linear-gradient(180deg,
    rgba(0,0,0,0.6) 0%,
    rgba(10,20,10,0.8) 50%,
    rgba(0,0,0,0.6) 100%
  );
  border-right: 1px solid rgba(255, 215, 64, 0.1);
  padding: 8px 4px;
  gap: 0;
}

/* Score block */
.score-block {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: 8px 0;
  width: 100%;
}

.score-block--opponent {
  border-bottom: 1px solid rgba(255, 215, 64, 0.08);
  padding-bottom: 12px;
}

.score-block--me {
  border-top: 1px solid rgba(255, 215, 64, 0.08);
  padding-top: 12px;
}

.score-value {
  font-size: 2rem;
  font-weight: 900;
  color: rgba(255, 255, 255, 0.25);
  line-height: 1;
  font-family: 'Georgia', serif;
  transition: color 0.3s ease, text-shadow 0.3s ease;
}

.score-value--leading {
  color: #ffd740;
  text-shadow: 0 0 16px rgba(255, 215, 64, 0.5), 0 0 32px rgba(255, 215, 64, 0.2);
}

.score-sub {
  font-size: 0.65rem;
  color: rgba(255, 255, 255, 0.3);
  text-transform: uppercase;
  letter-spacing: 0.1em;
}

.score-gems {
  display: flex;
  gap: 5px;
}

.score-gem {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.08);
  border: 1px solid rgba(255, 255, 255, 0.15);
  transition: all 0.3s ease;
}

.score-gem--won {
  background: radial-gradient(circle at 30% 30%, #ffe066, #ffd740, #ffa000);
  border-color: #ffa000;
  box-shadow: 0 0 8px rgba(255, 215, 64, 0.6);
}

/* Sekcja pogody */
.weather-section {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 8px 0;
}

.weather-title {
  font-size: 0.6rem;
  text-transform: uppercase;
  letter-spacing: 0.12em;
  color: rgba(255, 255, 255, 0.25);
}

.weather-icons {
  display: flex;
  flex-direction: column;
  gap: 4px;
  align-items: center;
}

.weather-icon {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid;
}

.weather-icon--frost {
  background: rgba(100, 180, 255, 0.15);
  border-color: rgba(100, 180, 255, 0.4);
  color: #90caf9;
}

.weather-icon--fog {
  background: rgba(150, 150, 200, 0.15);
  border-color: rgba(150, 150, 200, 0.4);
  color: #b0bec5;
}

.weather-icon--rain {
  background: rgba(80, 140, 255, 0.15);
  border-color: rgba(80, 140, 255, 0.4);
  color: #64b5f6;
}

.weather-none {
  opacity: 0.3;
}

/* Wskaźnik tury */
.turn-block {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  padding: 6px;
  border-radius: 8px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: rgba(0,0,0,0.3);
  color: rgba(255, 255, 255, 0.3);
  font-size: 0.55rem;
  text-align: center;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  transition: all 0.3s ease;
  width: 100%;
  box-sizing: border-box;
}

.turn-block--active {
  color: #ffd740;
  border-color: rgba(255, 215, 64, 0.3);
  background: rgba(255, 215, 64, 0.08);
  box-shadow: inset 0 0 12px rgba(255, 215, 64, 0.05);
}

.round-indicator {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1px;
}

.round-label-text {
  font-size: 0.55rem;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  color: rgba(255,255,255,0.25);
}

.round-number {
  font-size: 1.4rem;
  font-weight: 900;
  color: rgba(255, 215, 64, 0.6);
  line-height: 1;
}

/* PLANSZA GRY */
.board-area {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  padding: 4px 8px;
  gap: 0;
  scrollbar-width: thin;
  scrollbar-color: rgba(255, 215, 64, 0.1) transparent;
}

.board-rows {
  display: flex;
  flex-direction: column;
  gap: 3px;
  padding: 4px 0;
}

.board-divider {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 0;
  flex-shrink: 0;
}

.divider-line {
  flex: 1;
  height: 1px;
  background: linear-gradient(90deg, transparent, rgba(255, 215, 64, 0.2), transparent);
}

.divider-ornament {
  font-size: 1rem;
  color: rgba(255, 215, 64, 0.3);
  flex-shrink: 0;
}

/* ═══════════════════════════════════════════════════════════════════ */
/*  DÓŁ — ręka gracza                                                 */
/* ═══════════════════════════════════════════════════════════════════ */

.player-bar {
  display: flex;
  align-items: flex-end;
  gap: 12px;
  padding: 8px 16px;
  background: linear-gradient(0deg, rgba(0,0,0,0.75) 0%, rgba(0,0,0,0.3) 100%);
  border-top: 1px solid rgba(255, 215, 64, 0.15);
  flex-shrink: 0;
  min-height: 120px;
}

.player-commander-slot {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  flex-shrink: 0;
  width: 72px;
}

.player-hand {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.hand-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 4px;
}

.hand-actions {
  display: flex;
  gap: 6px;
}

.hand-cards-scroll {
  display: flex;
  gap: 4px;
  overflow-x: auto;
  padding: 4px 4px 0;
  align-items: flex-end;
  scrollbar-width: thin;
  scrollbar-color: rgba(255, 215, 64, 0.15) transparent;
}

.hand-cards-scroll::-webkit-scrollbar { height: 3px; }
.hand-cards-scroll::-webkit-scrollbar-thumb {
  background: rgba(255, 215, 64, 0.15);
  border-radius: 2px;
}

.player-decks {
  display: flex;
  flex-direction: column;
  gap: 8px;
  align-items: center;
  flex-shrink: 0;
  width: 72px;
}

/* ═══════════════════════════════════════════════════════════════════ */
/*  WSPÓLNE — talia, cmentarz, sloty                                  */
/* ═══════════════════════════════════════════════════════════════════ */

.deck-pile {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  position: relative;
}

.deck-icon-stack {
  position: relative;
  width: 44px;
  height: 60px;
}

.mini-card-back,
.mini-card-stack {
  position: absolute;
  width: 44px;
  height: 60px;
  border-radius: 3px;
  border: 1px solid rgba(255, 215, 64, 0.2);
  background: linear-gradient(135deg, #1a2a1a, #0d1a0d);
  background-image: repeating-linear-gradient(
    45deg,
    rgba(255, 215, 64, 0.04) 0px,
    rgba(255, 215, 64, 0.04) 1px,
    transparent 1px,
    transparent 8px
  );
}

.mini-card-stack {
  background: linear-gradient(135deg, #2a1a1a, #1a0d0d);
  border-color: rgba(255, 100, 64, 0.2);
}

.deck-badge {
  position: absolute;
  bottom: -4px;
  right: -4px;
  min-width: 20px;
  height: 20px;
  background: #0d1a0d;
  color: #ffd740;
  font-size: 0.7rem;
  font-weight: 800;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  border: 2px solid #ffd740;
  z-index: 10;
}

.graveyard-badge {
  color: #ef9a9a;
  border-color: #ef9a9a;
}

.slot-label {
  font-size: 0.6rem;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  color: rgba(255, 255, 255, 0.3);
  text-align: center;
}

.slot-name {
  font-size: 0.65rem;
  color: rgba(255, 255, 255, 0.5);
  text-align: center;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 72px;
}

.card-placeholder {
  width: 52px;
  height: 76px;
  border-radius: 5px;
  border: 1px dashed rgba(255, 255, 255, 0.12);
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0,0,0,0.2);
}

.commander-card {
  /* Lekkie podświetlenie dowódcy */
  filter: drop-shadow(0 0 6px rgba(255, 215, 64, 0.2));
}

/* ═══════════════════════════════════════════════════════════════════ */
/*  LOADING                                                           */
/* ═══════════════════════════════════════════════════════════════════ */

.game-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100vh;
  background: #0d1a0d;
}

/* ═══════════════════════════════════════════════════════════════════ */
/*  DIALOGI                                                           */
/* ═══════════════════════════════════════════════════════════════════ */

.gwint-dialog {
  background: rgba(15, 20, 12, 0.97) !important;
  border: 1px solid rgba(255, 215, 64, 0.2) !important;
  backdrop-filter: blur(16px);
}
</style>
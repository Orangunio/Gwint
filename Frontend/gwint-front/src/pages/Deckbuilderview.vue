<template>
  <div class="deck-builder">

    <!-- ══ NAGŁÓWEK ══════════════════════════════════════════════ -->
    <div class="deck-builder__header">
      <div class="deck-builder__header-inner">
        <h1 class="deck-builder__title">Konstruktor Talii</h1>
        <p class="deck-builder__subtitle">Wybierz frakcję i ułóż swoją talię</p>
      </div>
    </div>

    <!-- ══ ZAKŁADKI FRAKCJI ══════════════════════════════════════ -->
    <div class="fraction-tabs">
      <button
        class="fraction-tab"
        :class="{ 'fraction-tab--active-nilfgaard': selectedFraction === 1 }"
        @click="switchFraction(1)"
      >
        <span class="fraction-tab__icon">⚜</span>
        <span class="fraction-tab__name">Nilfgaard</span>
      </button>
      <button
        class="fraction-tab"
        :class="{ 'fraction-tab--active-north': selectedFraction === 2 }"
        @click="switchFraction(2)"
      >
        <span class="fraction-tab__icon">⚔</span>
        <span class="fraction-tab__name">Królestwa Północy</span>
      </button>
      <button
        class="fraction-tab"
        :class="{ 'fraction-tab--active-scoia': selectedFraction === 3 }"
        @click="switchFraction(3)"
      >
        <span class="fraction-tab__icon">🍃</span>
        <span class="fraction-tab__name">Scoia'tael</span>
      </button>
      <button
        class="fraction-tab"
        :class="{ 'fraction-tab--active-monsters': selectedFraction === 4 }"
        @click="switchFraction(4)"
      >
        <span class="fraction-tab__icon">☠</span>
        <span class="fraction-tab__name">Potwory</span>
      </button>
    </div>

    <!-- ══ GŁÓWNY UKŁAD ══════════════════════════════════════════ -->
    <div class="deck-builder__main">

      <!-- LEWA: Pula kart -->
      <div class="card-pool-wrapper">
        <!-- Wyszukiwarka -->
        <div class="card-pool__search">
          <input
            v-model="searchQuery"
            class="search-input"
            type="text"
            placeholder="Szukaj karty..."
          />
        </div>

        <!-- Ładowanie -->
        <div v-if="loading" class="card-pool__loading">
          <v-progress-circular indeterminate color="amber-darken-2" size="40" />
          <p class="mt-3">Ładowanie kart...</p>
        </div>

        <div v-else class="card-pool">

          <!-- Dowódcy -->
          <template v-if="filteredCommanderCards.length">
            <div class="card-pool__section-label">Dowódcy</div>
            <div class="card-pool__grid">
              <GwintDeckCard
                v-for="card in filteredCommanderCards"
                :key="card.id"
                :card="card"
                :in-deck="selectedCommander?.id === card.id"
                @click="handleCardClick"
                @hover="hoveredCard = $event"
                @unhover="hoveredCard = null"
              />
            </div>
          </template>

          <!-- Mistrzowie -->
          <template v-if="filteredChampionCards.length">
            <div class="card-pool__section-label">Mistrzowie</div>
            <div class="card-pool__grid">
              <GwintDeckCard
                v-for="card in filteredChampionCards"
                :key="card.id"
                :card="card"
                :in-deck="deckCardIds.has(card.id)"
                @click="handleCardClick"
                @hover="hoveredCard = $event"
                @unhover="hoveredCard = null"
              />
            </div>
          </template>

          <!-- Jednostki -->
          <template v-if="filteredRegularCards.length">
            <div class="card-pool__section-label">Jednostki</div>
            <div class="card-pool__grid">
              <GwintDeckCard
                v-for="card in filteredRegularCards"
                :key="card.id"
                :card="card"
                :in-deck="deckCardIds.has(card.id)"
                @click="handleCardClick"
                @hover="hoveredCard = $event"
                @unhover="hoveredCard = null"
              />
            </div>
          </template>

          <!-- Specjalne -->
          <template v-if="filteredSpecialCards.length">
            <div class="card-pool__section-label">Karty specjalne</div>
            <div class="card-pool__grid">
              <GwintDeckCard
                v-for="card in filteredSpecialCards"
                :key="card.id"
                :card="card"
                :in-deck="deckCardIds.has(card.id)"
                @click="handleCardClick"
                @hover="hoveredCard = $event"
                @unhover="hoveredCard = null"
              />
            </div>
          </template>

        </div>
      </div>

      <!-- PRAWA: Panel talii -->
      <div class="deck-panel">

        <!-- Dowódca -->
        <div class="deck-panel__commander">
          <div class="deck-panel__section-label">Dowódca</div>
          <div v-if="selectedCommander" class="deck-panel__commander-card" @click="removeCommander">
            <div class="deck-item__strength strength--commander">✦</div>
            <span class="deck-item__name" style="color: #e8cc7a;">{{ selectedCommander.name }}</span>
            <span class="deck-item__remove">✕</span>
          </div>
          <div v-else class="deck-panel__commander-empty">Brak dowódcy</div>
        </div>

        <!-- Statystyki -->
        <div class="deck-panel__stats">
          <div class="deck-panel__section-label">Talia</div>
          <div class="deck-stats">
            <span class="stat-pill stat-pill--total">{{ totalStrength }} pkt</span>
            <span class="stat-pill" :class="deckCards.length < 25 ? 'stat-pill--warn' : 'stat-pill--cards'">
              {{ deckCards.length }}/25+ kart
            </span>
            <span class="stat-pill" :class="specialCount > 10 ? 'stat-pill--danger' : 'stat-pill--spec'">
              {{ specialCount }}/10 spec.
            </span>
          </div>

          <!-- Walidacja -->
          <div class="deck-validation" :class="validationClass">
            {{ validationMessage }}
          </div>
        </div>

        <!-- Lista kart w talii -->
        <div class="deck-list">
          <div
            v-for="card in sortedDeckCards"
            :key="card.id"
            class="deck-item"
            @click="removeCard(card)"
          >
            <div class="deck-item__strength" :class="deckItemStrengthClass(card)">
              {{ card.isSpecial ? '✦' : card.strength }}
            </div>
            <span class="deck-item__name">{{ card.name }}</span>
            <span class="deck-item__place" :class="PLACES[card.place]?.css ?? 'place--any'">
              {{ PLACES[card.place]?.label ?? '–' }}
            </span>
            <span class="deck-item__remove">✕</span>
          </div>

          <div v-if="deckCards.length === 0" class="deck-list__empty">
            Pusta talia
          </div>
        </div>

        <!-- Przycisk zapisz -->
        <button class="save-btn" :disabled="!canSave || saving" @click="saveDeck">
          <v-progress-circular v-if="saving" indeterminate size="14" width="2" color="amber-darken-2" class="mr-2" />
          <span>{{ saving ? 'Zapisuję...' : 'Zapisz talię' }}</span>
        </button>

      </div>
    </div>

    <!-- ══ TOOLTIP KARTY ══════════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="tooltip">
        <div v-if="hoveredCard" class="card-tooltip" :style="tooltipStyle">
          <div class="card-tooltip__name">{{ hoveredCard.name }}</div>
          <div class="card-tooltip__row">
            <span>Frakcja</span>
            <span>{{ FRACTION_NAMES[hoveredCard.fraction] ?? '–' }}</span>
          </div>
          <div class="card-tooltip__row">
            <span>Typ</span>
            <span>{{ cardTypeLabel(hoveredCard) }}</span>
          </div>
          <div v-if="!hoveredCard.isSpecial && !hoveredCard.isCommander" class="card-tooltip__row">
            <span>Siła</span>
            <span>{{ hoveredCard.strength }}</span>
          </div>
          <div v-if="!hoveredCard.isSpecial" class="card-tooltip__row">
            <span>Rząd</span>
            <span>{{ PLACES[hoveredCard.place]?.label ?? '–' }}</span>
          </div>
          <div v-if="ABILITIES[hoveredCard.ability]" class="card-tooltip__ability">
            {{ ABILITIES[hoveredCard.ability]?.name }}
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import GwintDeckCard from '@/pages/Gwintcard.vue'
import { API_BASE_URL } from '@/api/config'

// ── Typy ─────────────────────────────────────────────────────────

interface CardData {
  id: number
  name: string
  fraction: number
  ability: number
  strength: number
  place: number
  isChampion: boolean
  isCommander: boolean
  isSpecial: boolean
}

// ── Stałe ────────────────────────────────────────────────────────

const FRACTION_NAMES: Record<number, string> = {
  1: 'Nilfgaard',
  2: 'Królestwa Północy',
  3: "Scoia'tael",
  4: 'Potwory',
}

const PLACES: Record<number, { label: string; css: string }> = {
  0:   { label: '–',         css: 'place--any' },
  1:   { label: 'Piechota',  css: 'place--infantry' },
  2:   { label: 'Dystans',   css: 'place--ranged' },
  3:   { label: 'Oblężenie', css: 'place--siege' },
  12:  { label: 'Zwinność',  css: 'place--agile' },
  23:  { label: 'Zwinność',  css: 'place--agile' },
  13:  { label: 'Zwinność',  css: 'place--agile' },
  123: { label: 'Dowódca',   css: 'place--any' },
}

const ABILITIES: Record<number, { name: string; css: string } | null> = {
  0:  null,
  1:  { name: 'Szpieg',       css: 'ab--spy' },
  2:  { name: 'Braterstwo',   css: 'ab--bond' },
  3:  { name: 'Zwinność',     css: 'ab--agility' },
  4:  { name: 'Werbowanie',   css: 'ab--muster' },
  5:  { name: 'Morale',       css: 'ab--morale' },
  6:  { name: 'Medyk',        css: 'ab--medic' },
  7:  { name: 'Berserker',    css: 'ab--berserker' },
  8:  { name: 'Spalenie',     css: 'ab--scorch' },
  9:  { name: 'Mistrz',       css: 'ab--champion' },
  10: { name: 'Róg Dowódcy',  css: 'ab--horn' },
  11: { name: 'Mróz',         css: 'ab--frost' },
  12: { name: 'Manekin',      css: 'ab--decoy' },
  13: { name: 'Mgła',         css: 'ab--fog' },
  14: { name: 'Deszcz',       css: 'ab--rain' },
  15: { name: 'Przegonienie', css: 'ab--clear' },
  20: { name: 'Wskrzeszenie', css: 'ab--medic' },
}

// ── State ─────────────────────────────────────────────────────────

const router = useRouter()

const selectedFraction = ref<number>(1)
const allCards         = ref<CardData[]>([])
const deckCards        = ref<CardData[]>([])
const selectedCommander = ref<CardData | null>(null)
const searchQuery      = ref('')
const loading          = ref(false)
const saving           = ref(false)
const hoveredCard      = ref<CardData | null>(null)

// ── Tooltip pozycja ───────────────────────────────────────────────

const tooltipStyle = ref({ left: '0px', top: '0px' })

function updateTooltipPosition(e: MouseEvent) {
  const x = e.clientX + 16
  const y = e.clientY - 20
  tooltipStyle.value = {
    left: `${Math.min(x, window.innerWidth - 175)}px`,
    top:  `${Math.min(y, window.innerHeight - 200)}px`,
  }
}

onMounted(() => {
  window.addEventListener('mousemove', updateTooltipPosition)
  fetchCards()
})

// ── API ───────────────────────────────────────────────────────────

function getPlayerId(): number {
  const id = Number(localStorage.getItem('gwint_player_id'))
  return Number.isFinite(id) && id > 0 ? id : 0
}

function getAuthToken(): string {
  return localStorage.getItem('gwint_token') || ''
}

async function fetchCards() {
  loading.value = true
  try {
    const playerId = getPlayerId()
    const fraction = selectedFraction.value

    console.log(`[fetchCards] PlayerId=${playerId}, Fraction=${fraction}`)

    // 1. Pobierz karty gracza (jego aktualna talia)
    const deckRes = await fetch(
      `${API_BASE_URL}/player-deck/get-player-fraction-deck/${playerId}/${fraction}`,
      {
        headers: {
          Authorization: `Bearer ${getAuthToken()}`
        }
      }
    )

    let deckData: any[] = []
    if (deckRes.ok) {
      deckData = await deckRes.json()
    } else {
      console.warn('Deck endpoint:', deckRes.status)
    }

    // 2. Pobierz pełną pulę kart frakcji
    const poolRes = await fetch(`${API_BASE_URL}/player-deck/available-cards/${fraction}`)
    if (!poolRes.ok) throw new Error('Nie udało się pobrać puli kart')

    const poolData: CardData[] = await poolRes.json()
    allCards.value = poolData

    // 3. Ustaw karty, które gracz ma w talii
    const deckCardIds = new Set(deckData.map(d => d.cardId || d.CardId))

    deckCards.value = poolData.filter(c => deckCardIds.has(c.id))

    // Dowódca
    selectedCommander.value = deckCards.value.find(c => c.isCommander) || null
    
    if (selectedCommander.value) {
      deckCards.value = deckCards.value.filter(c => !c.isCommander)
    }

    console.log(`✅ Załadowano pulę: ${poolData.length} kart | W talii: ${deckCards.value.length}`)

  } catch (err: any) {
    console.error('fetchCards error:', err)
  } finally {
    loading.value = false
  }
}

async function saveDeck() {
  if (!canSave.value) return
  saving.value = true

  try {
    const playerId = getPlayerId()
    const fraction = selectedFraction.value
    const newCommanderId = selectedCommander.value?.id

    // 1. Pobierz aktualną talię gracza
    const currentDeckRes = await fetch(
      `${API_BASE_URL}/player-deck/get-player-fraction-deck/${playerId}/${fraction}`,
      {
        headers: { Authorization: `Bearer ${getAuthToken()}` }
      }
    )

    let currentData: any[] = []
    if (currentDeckRes.ok) {
      currentData = await currentDeckRes.json()
    }

    const existingCardIds = new Set(currentData.map((item: any) => item.cardId || item.CardId))
    const existingCommanderId = currentData.find((item: any) => item.IsCommander || item.isCommander)?.cardId

    // 2. Przygotuj karty do usunięcia i dodania
    const cardIdsToRemove: number[] = []
    const cardIdsToAdd: number[] = []

    // Jeśli zmieniamy dowódcę → usuń starego
    if (existingCommanderId && newCommanderId && existingCommanderId !== newCommanderId) {
      cardIdsToRemove.push(existingCommanderId)
    }

    // Dodaj nowego dowódcę (jeśli go nie ma)
    if (newCommanderId && !existingCardIds.has(newCommanderId)) {
      cardIdsToAdd.push(newCommanderId)
    }

    // Dodaj zwykłe karty (tylko nowe)
    deckCards.value.forEach(card => {
      if (!existingCardIds.has(card.id)) {
        cardIdsToAdd.push(card.id)
      }
    })

    const bodyData = {
      playerId: playerId,
      fraction: fraction,
      cardIdsToAdd: cardIdsToAdd,
      cardIdsToRemove: cardIdsToRemove
    }

    console.log("📤 Wysyłam:", bodyData)
    console.log("Usuwam dowódcę:", cardIdsToRemove)
    console.log("Dodaję:", cardIdsToAdd)

    const res = await fetch(`${API_BASE_URL}/player-deck/update-deck`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${getAuthToken()}`,
      },
      body: JSON.stringify(bodyData)
    })

    if (!res.ok) {
      const errorText = await res.text()
      throw new Error(errorText)
    }

    alert("✅ Talia zapisana pomyślnie!")
    await router.push({ name: 'lobby' })

  } catch (err: any) {
    console.error(err)
    alert(`Błąd zapisu: ${err.message}`)
  } finally {
    saving.value = false
  }
}

// ── Zmiana frakcji ────────────────────────────────────────────────

function switchFraction(fraction: number) {
  if (selectedFraction.value === fraction) return
  selectedFraction.value = fraction
  deckCards.value = []
  selectedCommander.value = null
  searchQuery.value = ''
  fetchCards()
}

// ── Obsługa kart ──────────────────────────────────────────────────

function handleCardClick(card: CardData) {
  if (card.isCommander) {
    selectedCommander.value = selectedCommander.value?.id === card.id ? null : card
    return
  }

  if (deckCardIds.value.has(card.id)) return

  if (card.isSpecial && specialCount.value >= 10) {
    alert("Maksymalnie 10 kart specjalnych!")
    return
  }

  deckCards.value.push(card)
}

function removeCard(card: CardData) {
  deckCards.value = deckCards.value.filter(c => c.id !== card.id)
}

function removeCommander() {
  selectedCommander.value = null
}

// ── Computed: filtry ──────────────────────────────────────────────

const deckCardIds = computed(() => new Set(deckCards.value.map(c => c.id)))

const filteredCards = computed(() => {
  const q = searchQuery.value.toLowerCase()
  return allCards.value.filter(c =>
    !q || c.name.toLowerCase().includes(q)
  )
})

const filteredCommanderCards = computed(() => filteredCards.value.filter(c => c.isCommander))
const filteredChampionCards  = computed(() => filteredCards.value.filter(c => c.isChampion && !c.isCommander))
const filteredRegularCards   = computed(() => filteredCards.value.filter(c => !c.isCommander && !c.isChampion && !c.isSpecial))
const filteredSpecialCards   = computed(() => filteredCards.value.filter(c => c.isSpecial))

// ── Computed: talia ───────────────────────────────────────────────

const sortedDeckCards = computed(() =>
  [...deckCards.value].sort((a, b) => {
    if (a.isSpecial !== b.isSpecial) return a.isSpecial ? 1 : -1
    return b.strength - a.strength
  })
)

const totalStrength  = computed(() => deckCards.value.reduce((s, c) => s + c.strength, 0))
const specialCount   = computed(() => deckCards.value.filter(c => c.isSpecial).length)

const canSave = computed(() =>
  !!selectedCommander.value &&
  deckCards.value.length >= 25 &&
  specialCount.value <= 10
)

// ── Walidacja ─────────────────────────────────────────────────────

const validationMessage = computed(() => {
  if (!selectedCommander.value)         return 'Wybierz dowódcę'
  if (specialCount.value > 10)          return 'Za dużo kart specjalnych'
  if (deckCards.value.length < 25)      return `Brakuje ${25 - deckCards.value.length} kart`
  return 'Talia gotowa!'
})

const validationClass = computed(() => {
  if (!selectedCommander.value || specialCount.value > 10) return 'deck-validation--bad'
  if (deckCards.value.length < 25)                          return 'deck-validation--warn'
  return 'deck-validation--ok'
})

// ── Helpery ───────────────────────────────────────────────────────

function deckItemStrengthClass(card: CardData) {
  if (card.isSpecial)   return 'strength--special'
  if (card.isCommander) return 'strength--commander'
  if (card.isChampion)  return 'strength--champion'
  return 'strength--normal'
}

function cardTypeLabel(card: CardData) {
  if (card.isCommander) return 'Dowódca'
  if (card.isChampion)  return 'Mistrz'
  if (card.isSpecial)   return 'Specjalna'
  return 'Jednostka'
}
</script>

<style scoped>
/* ══ LAYOUT ═══════════════════════════════════════════════════════ */

.deck-builder {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background:
    radial-gradient(ellipse at 50% 0%, rgba(40, 30, 10, 0.55) 0%, transparent 55%),
    #0d0f0a;
  color: #e8e0cc;
  font-family: 'Georgia', serif;
  overflow: hidden;
}

/* ── Nagłówek ────────────────────────────────────────────────────── */

.deck-builder__header {
  border-bottom: 1px solid rgba(200, 168, 75, 0.15);
  background: rgba(0, 0, 0, 0.3);
  padding: 14px 20px 10px;
  flex-shrink: 0;
  text-align: center;
}

.deck-builder__title {
  font-size: 1.4rem;
  font-weight: 700;
  letter-spacing: 0.18em;
  text-transform: uppercase;
  color: #e8cc7a;
}

.deck-builder__subtitle {
  font-size: 0.8rem;
  color: rgba(232, 224, 204, 0.4);
  margin-top: 2px;
}

/* ── Zakładki frakcji ────────────────────────────────────────────── */

.fraction-tabs {
  display: flex;
  border-bottom: 1px solid rgba(200, 168, 75, 0.15);
  flex-shrink: 0;
}

.fraction-tab {
  flex: 1;
  padding: 10px 8px;
  background: transparent;
  border: none;
  border-bottom: 2px solid transparent;
  cursor: pointer;
  color: rgba(232, 224, 204, 0.35);
  font-family: 'Georgia', serif;
  font-size: 0.75rem;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: all 0.2s;
}

.fraction-tab:hover { color: rgba(232, 224, 204, 0.75); background: rgba(255, 255, 255, 0.03); }

.fraction-tab--active-nilfgaard {
  color: #e8cc7a;
  border-bottom-color: #c8a84b;
  background: rgba(200, 168, 75, 0.06);
}

.fraction-tab--active-north {
  color: #7ab4e8;
  border-bottom-color: #4a7cba;
  background: rgba(74, 124, 186, 0.06);
}

.fraction-tab--active-scoia {
  color: #8fd28f;
  border-bottom-color: #4a9a4a;
  background: rgba(74, 154, 74, 0.06);
}

.fraction-tab--active-monsters {
  color: #d08080;
  border-bottom-color: #a04040;
  background: rgba(160, 64, 64, 0.06);
}

.fraction-tab__icon { font-size: 1.1rem; }

/* ── Główny układ ────────────────────────────────────────────────── */

.deck-builder__main {
  flex: 1;
  display: flex;
  overflow: hidden;
  min-height: 0;
}

/* ── Pula kart (lewa) ───────────────────────────────────────────── */

.card-pool-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  border-right: 1px solid rgba(200, 168, 75, 0.1);
}

.card-pool__search {
  padding: 8px 12px;
  border-bottom: 1px solid rgba(200, 168, 75, 0.1);
  background: rgba(0, 0, 0, 0.1);
  flex-shrink: 0;
}

.search-input {
  width: 100%;
  padding: 6px 12px;
  background: rgba(0, 0, 0, 0.3);
  border: 1px solid rgba(200, 168, 75, 0.18);
  border-radius: 4px;
  color: #e8e0cc;
  font-family: 'Georgia', serif;
  font-size: 0.85rem;
  outline: none;
  transition: border-color 0.15s;
}

.search-input:focus { border-color: rgba(200, 168, 75, 0.45); }
.search-input::placeholder { color: rgba(232, 224, 204, 0.25); }

.card-pool__loading {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: rgba(232, 224, 204, 0.4);
  font-size: 0.85rem;
}

.card-pool {
  flex: 1;
  overflow-y: auto;
  padding: 10px 12px;
  scrollbar-width: thin;
  scrollbar-color: rgba(200, 168, 75, 0.2) transparent;
}

.card-pool__section-label {
  font-size: 0.6rem;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  color: rgba(232, 224, 204, 0.3);
  padding: 6px 2px 4px;
  border-bottom: 1px solid rgba(200, 168, 75, 0.08);
  margin-bottom: 8px;
}

.card-pool__grid {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-bottom: 14px;
}

/* ── Panel talii (prawa) ─────────────────────────────────────────── */

.deck-panel {
  width: 230px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  background: rgba(0, 0, 0, 0.2);
}

.deck-panel__section-label {
  font-size: 0.6rem;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  color: rgba(232, 224, 204, 0.3);
  margin-bottom: 6px;
}

/* Dowódca */
.deck-panel__commander {
  padding: 8px 10px;
  border-bottom: 1px solid rgba(200, 168, 75, 0.1);
}

.deck-panel__commander-card {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 8px;
  border-radius: 4px;
  background: rgba(200, 168, 75, 0.07);
  border: 1px solid rgba(200, 168, 75, 0.22);
  cursor: pointer;
  transition: background 0.12s;
}

.deck-panel__commander-card:hover { background: rgba(200, 168, 75, 0.13); }

.deck-panel__commander-empty {
  padding: 8px;
  text-align: center;
  font-size: 0.65rem;
  color: rgba(232, 224, 204, 0.25);
  border: 1px dashed rgba(200, 168, 75, 0.15);
  border-radius: 4px;
  letter-spacing: 0.06em;
}

/* Statystyki */
.deck-panel__stats {
  padding: 8px 10px;
  border-bottom: 1px solid rgba(200, 168, 75, 0.1);
}

.deck-stats {
  display: flex;
  gap: 5px;
  flex-wrap: wrap;
  margin-bottom: 6px;
}

.stat-pill {
  font-size: 0.65rem;
  padding: 2px 7px;
  border-radius: 10px;
  border: 1px solid;
}

.stat-pill--total  { border-color: rgba(200, 168, 75, 0.35); color: #e8cc7a;  background: rgba(200, 168, 75, 0.07); }
.stat-pill--cards  { border-color: rgba(80, 150, 80, 0.35);  color: #80c080;  background: rgba(80, 150, 80, 0.07); }
.stat-pill--spec   { border-color: rgba(130, 60, 170, 0.35); color: #b070d0; background: rgba(130, 60, 170, 0.07); }
.stat-pill--warn   { border-color: rgba(200, 140, 30, 0.45); color: #d0a040; background: rgba(200, 140, 30, 0.09); }
.stat-pill--danger { border-color: rgba(200, 60, 60, 0.45);  color: #e08080; background: rgba(200, 60, 60, 0.09); }

.deck-validation {
  font-size: 0.65rem;
  padding: 4px 8px;
  border-radius: 4px;
  text-align: center;
  letter-spacing: 0.07em;
  text-transform: uppercase;
}

.deck-validation--ok   { background: rgba(40, 100, 40, 0.2);  color: #80c080; border: 1px solid rgba(40, 100, 40, 0.3); }
.deck-validation--warn { background: rgba(160, 100, 20, 0.2); color: #c8a030; border: 1px solid rgba(160, 100, 20, 0.3); }
.deck-validation--bad  { background: rgba(120, 30, 30, 0.2);  color: #d06060; border: 1px solid rgba(120, 30, 30, 0.3); }

/* Lista kart w talii */
.deck-list {
  flex: 1;
  overflow-y: auto;
  padding: 6px;
  scrollbar-width: thin;
  scrollbar-color: rgba(200, 168, 75, 0.15) transparent;
}

.deck-list__empty {
  text-align: center;
  font-size: 0.7rem;
  color: rgba(232, 224, 204, 0.25);
  padding: 20px;
  letter-spacing: 0.06em;
}

.deck-item {
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 4px 6px;
  border-radius: 4px;
  margin-bottom: 2px;
  cursor: pointer;
  border: 1px solid transparent;
  transition: background 0.12s;
}

.deck-item:hover {
  background: rgba(200, 168, 75, 0.05);
  border-color: rgba(200, 168, 75, 0.15);
}

.deck-item__strength {
  width: 22px;
  height: 22px;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: 'Georgia', serif;
  font-size: 11px;
  font-weight: 700;
  flex-shrink: 0;
}

.strength--normal    { background: rgba(20, 40, 10, 0.9); border: 1px solid #2a5010; color: #70b040; }
.strength--champion  { background: rgba(40, 30,  5, 0.9); border: 1px solid #604010; color: #c8a84b; }
.strength--commander { background: rgba(40, 30,  5, 0.9); border: 1px solid #604010; color: #e8cc7a; }
.strength--special   { background: rgba(30, 10, 40, 0.9); border: 1px solid #5a2a7a; color: #b060d0; }

.deck-item__name {
  flex: 1;
  font-size: 0.7rem;
  color: rgba(232, 224, 204, 0.8);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.deck-item__place {
  font-size: 0.6rem;
  padding: 1px 4px;
  border-radius: 2px;
  border: 1px solid;
  flex-shrink: 0;
}

/* Klasy rzędów dla deck-item__place */
.place--infantry { background: rgba(120, 20, 20, 0.45); color: #f08080; border-color: rgba(160, 40, 40, 0.5); }
.place--ranged   { background: rgba(20, 80, 20, 0.45);  color: #70c070; border-color: rgba(40, 120, 40, 0.5); }
.place--siege    { background: rgba(20, 40, 110, 0.45); color: #80b0e8; border-color: rgba(40, 80, 160, 0.5); }
.place--agile    { background: rgba(90, 65, 10, 0.45);  color: #c8a84b; border-color: rgba(130, 100, 20, 0.5); }
.place--special  { background: rgba(60, 10, 80, 0.45);  color: #c070e0; border-color: rgba(100, 30, 130, 0.5); }
.place--any      { background: rgba(40, 40, 40, 0.45);  color: #aaaaaa; border-color: rgba(80, 80, 80, 0.5); }

.deck-item__remove {
  font-size: 0.75rem;
  color: rgba(200, 60, 60, 0.45);
  flex-shrink: 0;
  transition: color 0.12s;
}

.deck-item:hover .deck-item__remove { color: rgba(200, 60, 60, 0.9); }

/* Przycisk zapisz */
.save-btn {
  margin: 8px;
  padding: 10px 16px;
  font-family: 'Georgia', serif;
  font-size: 0.7rem;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  background: rgba(200, 168, 75, 0.1);
  color: #e8cc7a;
  border: 1px solid rgba(200, 168, 75, 0.35);
  border-radius: 5px;
  cursor: pointer;
  transition: all 0.15s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.save-btn:hover:not(:disabled) {
  background: rgba(200, 168, 75, 0.2);
  border-color: rgba(200, 168, 75, 0.6);
}

.save-btn:disabled { opacity: 0.35; cursor: default; }

/* ══ TOOLTIP ══════════════════════════════════════════════════════ */

.card-tooltip {
  position: fixed;
  z-index: 9999;
  width: 168px;
  background: #0a0c08;
  border: 1px solid rgba(200, 168, 75, 0.35);
  border-radius: 6px;
  padding: 10px 12px;
  pointer-events: none;
  font-size: 0.7rem;
  line-height: 1.5;
  box-shadow: 0 10px 28px rgba(0, 0, 0, 0.85);
}

.card-tooltip__name {
  font-size: 0.8rem;
  color: #e8cc7a;
  margin-bottom: 6px;
  padding-bottom: 5px;
  border-bottom: 1px solid rgba(200, 168, 75, 0.2);
}

.card-tooltip__row {
  display: flex;
  justify-content: space-between;
  color: rgba(232, 224, 204, 0.6);
  margin: 2px 0;
  font-size: 0.68rem;
}

.card-tooltip__row span:last-child { color: rgba(232, 224, 204, 0.88); }

.card-tooltip__ability {
  margin-top: 5px;
  font-size: 0.68rem;
  color: #c8a84b;
  font-style: italic;
}

.tooltip-enter-active, .tooltip-leave-active { transition: opacity 0.1s; }
.tooltip-enter-from,  .tooltip-leave-to      { opacity: 0; }
</style>
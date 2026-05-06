<template>
  <div
    class="gwint-card"
    :class="[
      fractionClass,
      { 'gwint-card--commander': card.isCommander },
      { 'gwint-card--champion': card.isChampion },
      { 'gwint-card--special': card.isSpecial },
      { 'gwint-card--in-deck': inDeck },
      { 'gwint-card--selected': selected },
    ]"
    @click="handleClick"
    @mouseenter="$emit('hover', card)"
    @mouseleave="$emit('unhover')"
  >
    <!-- Pasek frakcji -->
    <div class="gwint-card__fraction-bar" />

    <div class="gwint-card__inner">
      <!-- Górny rząd: siła + rząd -->
      <div class="gwint-card__top-row">
        <div class="gwint-card__strength" :class="strengthClass">
          <span v-if="card.isSpecial || card.isCommander">✦</span>
          <span v-else>{{ card.strength }}</span>
        </div>
        <span class="gwint-card__place-badge" :class="placeInfo.css">
          {{ card.isSpecial ? 'Spec.' : placeInfo.label }}
        </span>
      </div>

      <!-- Nazwa karty -->
      <div class="gwint-card__name">{{ card.name }}</div>

      <!-- Ikona zdolności -->
      <div class="gwint-card__abilities">
        <div
          v-if="abilityInfo"
          class="gwint-card__ability-dot"
          :class="abilityInfo.css"
          :title="abilityInfo.name"
        />
      </div>
    </div>

    <!-- Pasek typologiczny na dole -->
    <div v-if="typeBarClass" class="gwint-card__type-bar" :class="typeBarClass" />

    <!-- Overlay "w talii" -->
    <div v-if="inDeck" class="gwint-card__in-deck-overlay">
      <span class="gwint-card__in-deck-check">✓</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

export interface GwintCardData {
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

const props = defineProps<{
  card: GwintCardData
  inDeck?: boolean
  selected?: boolean
}>()

const emit = defineEmits<{
  (e: 'click', card: GwintCardData): void
  (e: 'hover', card: GwintCardData): void
  (e: 'unhover'): void
}>()

function handleClick() {
  if (!props.inDeck) emit('click', props.card)
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
  1:  { name: 'Szpieg',          css: 'ab--spy' },
  2:  { name: 'Braterstwo',      css: 'ab--bond' },
  3:  { name: 'Zwinność',        css: 'ab--agility' },
  4:  { name: 'Werbowanie',      css: 'ab--muster' },
  5:  { name: 'Morale',          css: 'ab--morale' },
  6:  { name: 'Medyk',           css: 'ab--medic' },
  7:  { name: 'Berserker',       css: 'ab--berserker' },
  8:  { name: 'Spalenie',        css: 'ab--scorch' },
  9:  { name: 'Mistrz',          css: 'ab--champion' },
  10: { name: 'Róg Dowódcy',     css: 'ab--horn' },
  11: { name: 'Mróz',            css: 'ab--frost' },
  12: { name: 'Manekin',         css: 'ab--decoy' },
  13: { name: 'Mgła',            css: 'ab--fog' },
  14: { name: 'Deszcz',          css: 'ab--rain' },
  15: { name: 'Przegonienie',    css: 'ab--clear' },
  20: { name: 'Wskrzeszenie',    css: 'ab--medic' },
}

const placeInfo    = computed(() => PLACES[props.card.place] ?? PLACES[0])
const abilityInfo  = computed(() => ABILITIES[props.card.ability] ?? null)

const fractionClass = computed(() =>
  props.card.fraction === 1 ? 'gwint-card--nilfgaard' : 'gwint-card--north'
)

const strengthClass = computed(() => {
  if (props.card.isSpecial)   return 'strength--special'
  if (props.card.isCommander) return 'strength--commander'
  if (props.card.isChampion)  return 'strength--champion'
  return 'strength--normal'
})

const typeBarClass = computed(() => {
  if (props.card.isCommander) return 'type-bar--commander'
  if (props.card.isChampion)  return 'type-bar--champion'
  if (props.card.isSpecial)   return 'type-bar--special'
  return null
})
</script>

<style scoped>
/* ── Podstawowa karta ────────────────────────────────────────────── */
.gwint-card {
  width: 76px;
  height: 110px;
  border-radius: 4px;
  border: 1px solid rgba(200, 168, 75, 0.25);
  cursor: pointer;
  position: relative;
  overflow: hidden;
  transition: transform 0.15s ease, border-color 0.15s ease, box-shadow 0.15s ease;
  background: linear-gradient(160deg, #1a1a12 0%, #0d0d08 100%);
  flex-shrink: 0;
  user-select: none;
}

.gwint-card:hover:not(.gwint-card--in-deck) {
  transform: translateY(-4px) scale(1.05);
  border-color: rgba(200, 168, 75, 0.75);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.7), 0 0 10px rgba(200, 168, 75, 0.18);
  z-index: 10;
}

.gwint-card--selected {
  border-color: rgba(200, 168, 75, 0.9) !important;
  box-shadow: 0 0 0 2px rgba(200, 168, 75, 0.35) !important;
}

.gwint-card--in-deck {
  opacity: 0.5;
  cursor: default;
}

.gwint-card--commander { border-color: rgba(200, 168, 75, 0.45); }
.gwint-card--champion  { border-color: rgba(180, 100, 40, 0.45); }

/* ── Pasek frakcji (góra) ───────────────────────────────────────── */
.gwint-card__fraction-bar {
  height: 3px;
  width: 100%;
}

.gwint-card--nilfgaard .gwint-card__fraction-bar {
  background: linear-gradient(90deg, #7a6010, #c8a840, #7a6010);
}

.gwint-card--north .gwint-card__fraction-bar {
  background: linear-gradient(90deg, #1a3a6a, #4a7cba, #1a3a6a);
}

.gwint-card--special .gwint-card__fraction-bar {
  background: linear-gradient(90deg, #3a1a4a, #8a4aaa, #3a1a4a);
}

/* ── Zawartość ──────────────────────────────────────────────────── */
.gwint-card__inner {
  padding: 4px 4px 3px;
  height: calc(100% - 3px);
  display: flex;
  flex-direction: column;
}

.gwint-card__top-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 3px;
}

/* ── Siła ───────────────────────────────────────────────────────── */
.gwint-card__strength {
  width: 24px;
  height: 24px;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: 'Georgia', serif;
  font-size: 13px;
  font-weight: 700;
  flex-shrink: 0;
}

.strength--normal    { background: rgba(20, 40, 10, 0.9); border: 1px solid #2a5010; color: #70b040; }
.strength--champion  { background: rgba(40, 30,  5, 0.9); border: 1px solid #604010; color: #c8a84b; }
.strength--commander { background: rgba(40, 30,  5, 0.9); border: 1px solid #604010; color: #e8cc7a; font-size: 11px; }
.strength--special   { background: rgba(30, 10, 40, 0.9); border: 1px solid #5a2a7a; color: #b060d0; font-size: 11px; }

/* ── Etykietka rzędu ────────────────────────────────────────────── */
.gwint-card__place-badge {
  font-size: 7.5px;
  padding: 1px 3px;
  border-radius: 2px;
  font-family: 'Georgia', serif;
  letter-spacing: 0.04em;
  white-space: nowrap;
  border: 1px solid;
}

.place--infantry { background: rgba(120, 20, 20, 0.55); color: #f08080; border-color: rgba(160, 40, 40, 0.6); }
.place--ranged   { background: rgba(20, 80, 20, 0.55);  color: #70c070; border-color: rgba(40, 120, 40, 0.6); }
.place--siege    { background: rgba(20, 40, 110, 0.55); color: #80b0e8; border-color: rgba(40, 80, 160, 0.6); }
.place--agile    { background: rgba(90, 65, 10, 0.55);  color: #c8a84b; border-color: rgba(130, 100, 20, 0.6); }
.place--special  { background: rgba(60, 10, 80, 0.55);  color: #c070e0; border-color: rgba(100, 30, 130, 0.6); }
.place--any      { background: rgba(40, 40, 40, 0.55);  color: #aaaaaa; border-color: rgba(80, 80, 80, 0.6); }

/* ── Nazwa karty ────────────────────────────────────────────────── */
.gwint-card__name {
  font-size: 8.5px;
  line-height: 1.25;
  color: rgba(232, 224, 204, 0.85);
  flex: 1;
  display: flex;
  align-items: flex-end;
  word-break: break-word;
}

/* ── Zdolność ───────────────────────────────────────────────────── */
.gwint-card__abilities {
  display: flex;
  gap: 2px;
  margin-top: 3px;
}

.gwint-card__ability-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  flex-shrink: 0;
}

.ab--spy       { background: #c05030; }
.ab--bond      { background: #4080c0; }
.ab--agility   { background: #c09040; }
.ab--muster    { background: #40a040; }
.ab--morale    { background: #c0a030; }
.ab--medic     { background: #60b080; }
.ab--berserker { background: #c04040; }
.ab--scorch    { background: #e06020; }
.ab--champion  { background: #c8a84b; }
.ab--horn      { background: #d0c060; }
.ab--decoy     { background: #a060c0; }
.ab--frost     { background: #80c0e0; }
.ab--fog       { background: #8090b0; }
.ab--rain      { background: #4060a0; }
.ab--clear     { background: #60d060; }

/* ── Pasek typologiczny (dół) ───────────────────────────────────── */
.gwint-card__type-bar {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 2px;
}

.type-bar--commander { background: linear-gradient(90deg, transparent, #c8a84b, transparent); }
.type-bar--champion  { background: linear-gradient(90deg, transparent, #c06020, transparent); }
.type-bar--special   { background: linear-gradient(90deg, transparent, #8030a0, transparent); }

/* ── Overlay "w talii" ──────────────────────────────────────────── */
.gwint-card__in-deck-overlay {
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
}

.gwint-card__in-deck-check {
  color: #c8a84b;
  font-size: 20px;
}
</style>
<template>
  <section class="hero-section">
    <div class="hero-bg" />
    <div class="hero-overlay" />

    <v-container class="hero-content">
      <v-row justify="center" align="center" class="min-height-hero">
        <v-col cols="12" md="8" class="text-center">
          <div class="hero-badge mb-4">
            <v-chip
                color="amber-darken-2"
                variant="outlined"
                size="small"
                prepend-icon="mdi-sword-cross"
            >
              Gra karciana
            </v-chip>
          </div>

          <h1 class="hero-title mb-4">
            <span class="title-glow">GWINT</span>
          </h1>

          <p class="hero-subtitle mb-8 text-medium-emphasis">
            Strategiczna gra karciana, w której spryt i taktyka decydują o zwycięstwie.
            Zbierz swój deck, wybierz frakcję i poprowadź swoje wojska do chwały.
          </p>

          <div class="d-flex justify-center ga-4 flex-wrap">
            <v-btn
                color="amber-darken-2"
                size="x-large"
                variant="elevated"
                prepend-icon="mdi-sword"
                class="hero-btn-primary"
                @click="goToLobby"
            >
              Zagraj Teraz
            </v-btn>

            <v-btn
                color="white"
                size="x-large"
                variant="outlined"
                prepend-icon="mdi-book-open-variant"
                class="hero-btn-secondary"
                @click="$emit('learnMore')"
            >
              Jak Grać?
            </v-btn>
          </div>

          <div class="hero-stats mt-12">
            <v-row justify="center" class="ga-4">
              <v-col
                  v-for="stat in heroStats"
                  :key="stat.label"
                  cols="auto"
              >
                <div class="stat-item text-center">
                  <div class="stat-value">{{ stat.value }}</div>
                  <div class="stat-label text-caption text-medium-emphasis">
                    {{ stat.label }}
                  </div>
                </div>
              </v-col>
            </v-row>
          </div>
        </v-col>
      </v-row>
    </v-container>
  </section>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

// Definiujemy emisję zdarzeń (opcjonalnie, jeśli rodzic nadal ma o tym wiedzieć)
const emit = defineEmits<{
  startGame: []
  learnMore: []
}>()

const goToLobby = () => {
  // 1. Emitujemy zdarzenie do rodzica (jeśli potrzebne)
  emit('startGame') 
  
  // 2. Wykonujemy faktyczne przekierowanie na podstronę /lobby
  router.push('/lobby')
}

const heroStats = [
  { value: '200+', label: 'Kart' },
  { value: '5', label: 'Frakcji' },
  { value: '∞', label: 'Strategii' },
]
</script>

<style scoped>
.hero-section {
  position: relative;
  min-height: 90vh;
  display: flex;
  align-items: center;
  overflow: hidden;
}

.hero-bg {
  position: absolute;
  inset: 0;
  background:
      radial-gradient(ellipse at 20% 50%, rgba(255, 111, 0, 0.08) 0%, transparent 60%),
      radial-gradient(ellipse at 80% 20%, rgba(255, 215, 64, 0.06) 0%, transparent 50%),
      radial-gradient(ellipse at 60% 80%, rgba(139, 0, 0, 0.1) 0%, transparent 50%);
}

.hero-overlay {
  position: absolute;
  inset: 0;
  background: url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.015'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E");
}

.hero-content {
  position: relative;
  z-index: 1;
}

.min-height-hero {
  min-height: 80vh;
}

.hero-title {
  font-size: clamp(4rem, 12vw, 9rem);
  font-weight: 900;
  letter-spacing: 0.5rem;
  line-height: 1;
}

.title-glow {
  background: linear-gradient(135deg, #ffd740 0%, #ff6d00 50%, #ffd740 100%);
  background-size: 200% auto;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  animation: shimmer 3s linear infinite;
  filter: drop-shadow(0 0 30px rgba(255, 215, 64, 0.3));
}

.hero-subtitle {
  font-size: clamp(1rem, 2vw, 1.25rem);
  max-width: 560px;
  margin-inline: auto;
  line-height: 1.7;
}

.hero-btn-primary {
  font-weight: 700;
  letter-spacing: 0.05rem;
  box-shadow: 0 4px 24px rgba(255, 111, 0, 0.4) !important;
  transition: all 0.3s ease !important;
}

.hero-btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 32px rgba(255, 111, 0, 0.5) !important;
}

.hero-btn-secondary {
  border-color: rgba(255, 255, 255, 0.3) !important;
}

.stat-item {
  padding: 0 1.5rem;
  border-right: 1px solid rgba(255, 215, 64, 0.2);
}

.stat-item:last-child {
  border-right: none;
}

.stat-value {
  font-size: 1.75rem;
  font-weight: 900;
  color: #ffd740;
  line-height: 1;
}

.stat-label {
  margin-top: 0.25rem;
  letter-spacing: 0.05rem;
}

@keyframes shimmer {
  0% { background-position: 0% center; }
  100% { background-position: 200% center; }
}
</style>
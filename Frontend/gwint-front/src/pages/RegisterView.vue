<template>
  <section class="auth-page">
    <div class="auth-bg" />
    <div class="auth-overlay" />

    <AppHeader />

    <v-main class="auth-main">
      <v-container class="fill-height">
        <v-row align="center" class="fill-height" justify="center">
          <v-col cols="12" lg="5" md="6">
            <div class="auth-shell">
              <div class="auth-badge mb-4 text-center">
                <v-chip
                    color="amber-darken-2"
                    prepend-icon="mdi-cards-outline"
                    size="small"
                    variant="outlined"
                >
                  Nowy gracz
                </v-chip>
              </div>

              <div class="text-center mb-8">
                <h1 class="auth-title mb-3">
                  Dołącz do <span class="title-glow">GWINTA</span>
                </h1>
                <p class="auth-subtitle text-medium-emphasis">
                  Załóż konto, buduj talię i przygotuj się do pierwszych pojedynków.
                </p>
              </div>

              <v-card class="auth-card" elevation="0" rounded="xl">
                <div class="card-glow" />

                <v-card-text class="pa-8">
                  <v-form @submit.prevent="handleRegister">
                    <v-text-field
                        v-model="form.login"
                        class="mb-4 auth-input"
                        :disabled="playerStore.isLoading"
                        label="Login"
                        prepend-inner-icon="mdi-account"
                        required
                        variant="outlined"
                    />

                    <v-text-field
                        v-model="form.password"
                        :append-inner-icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
                        class="mb-4 auth-input"
                        :disabled="playerStore.isLoading"
                        label="Hasło"
                        prepend-inner-icon="mdi-lock"
                        required
                        :type="showPassword ? 'text' : 'password'"
                        variant="outlined"
                        @click:append-inner="showPassword = !showPassword"
                    />

                    <v-text-field
                        v-model="confirmPassword"
                        :append-inner-icon="showConfirmPassword ? 'mdi-eye-off' : 'mdi-eye'"
                        class="mb-2 auth-input"
                        :disabled="playerStore.isLoading"
                        label="Powtórz hasło"
                        prepend-inner-icon="mdi-lock-check"
                        required
                        :type="showConfirmPassword ? 'text' : 'password'"
                        variant="outlined"
                        @click:append-inner="showConfirmPassword = !showConfirmPassword"
                    />

                    <v-alert
                        v-if="localError || playerStore.error"
                        class="mt-4"
                        density="comfortable"
                        type="error"
                        variant="tonal"
                    >
                      {{ localError || playerStore.error }}
                    </v-alert>

                    <v-alert
                        v-if="successMessage"
                        class="mt-4"
                        density="comfortable"
                        type="success"
                        variant="tonal"
                    >
                      {{ successMessage }}
                    </v-alert>

                    <v-btn
                        block
                        class="mt-6 auth-btn-primary"
                        color="amber-darken-2"
                        :loading="playerStore.isLoading"
                        size="x-large"
                        type="submit"
                        variant="elevated"
                    >
                      Utwórz konto
                    </v-btn>
                  </v-form>

                  <div class="auth-divider my-6">
                    <span>lub</span>
                  </div>

                  <div class="d-flex flex-column flex-sm-row ga-3 justify-space-between">
                    <v-btn
                        block
                        class="auth-btn-secondary"
                        :to="{ name: 'login' }"
                        variant="outlined"
                    >
                      Mam już konto
                    </v-btn>

                    <v-btn
                        block
                        class="auth-btn-secondary"
                        :to="{ name: 'home' }"
                        variant="text"
                    >
                      Wróć do strony głównej
                    </v-btn>
                  </div>
                </v-card-text>
              </v-card>
            </div>
          </v-col>
        </v-row>
      </v-container>
    </v-main>

    <AppFooter />
  </section>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import AppFooter from '@/components/layout/AppFooter.vue'
import AppHeader from '@/components/layout/AppHeader.vue'
import { usePlayerStore } from '@/stores/player'

const router = useRouter()
const playerStore = usePlayerStore()

const showPassword = ref(false)
const showConfirmPassword = ref(false)

const localError = ref<string | null>(null)
const successMessage = ref<string | null>(null)

const form = reactive({
  login: '',
  password: '',
})

const confirmPassword = ref('')

async function handleRegister() {
  playerStore.clearError()
  localError.value = null
  successMessage.value = null

  if (!form.login.trim()) {
    localError.value = 'Login jest wymagany.'
    return
  }

  if (!form.password) {
    localError.value = 'Hasło jest wymagane.'
    return
  }

  if (form.password.length < 6) {
    localError.value = 'Hasło powinno mieć minimum 6 znaków.'
    return
  }

  if (form.password !== confirmPassword.value) {
    localError.value = 'Hasła nie są takie same.'
    return
  }

  try {
    await playerStore.register({
      login: form.login.trim(),
      password: form.password,
    })

    successMessage.value = 'Konto zostało utworzone. Za chwilę przejdziesz do logowania.'

    setTimeout(() => {
      router.push({ name: 'login' })
    }, 900)
  } catch {
    // obsłużone w store
  }
}
</script>

<style scoped>
.auth-page {
  position: relative;
  min-height: 100vh;
  overflow: hidden;
}

.auth-main {
  position: relative;
  z-index: 1;
}

.auth-bg {
  position: absolute;
  inset: 0;
  background:
      radial-gradient(ellipse at 20% 20%, rgba(255, 111, 0, 0.09) 0%, transparent 55%),
      radial-gradient(ellipse at 80% 15%, rgba(255, 215, 64, 0.08) 0%, transparent 45%),
      radial-gradient(ellipse at 50% 85%, rgba(139, 0, 0, 0.12) 0%, transparent 55%);
}

.auth-overlay {
  position: absolute;
  inset: 0;
  background:
      linear-gradient(to bottom, rgba(10, 10, 10, 0.45), rgba(10, 10, 10, 0.72)),
      url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.015'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E");
}

.auth-shell {
  padding-top: 96px;
  padding-bottom: 48px;
}

.auth-title {
  font-size: clamp(2rem, 4vw, 3.2rem);
  font-weight: 900;
  letter-spacing: 0.06rem;
  line-height: 1.1;
}

.title-glow {
  background: linear-gradient(135deg, #ffd740 0%, #ff6d00 50%, #ffd740 100%);
  background-size: 200% auto;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  animation: shimmer 3s linear infinite;
  filter: drop-shadow(0 0 18px rgba(255, 215, 64, 0.25));
}

.auth-subtitle {
  max-width: 520px;
  margin-inline: auto;
  line-height: 1.7;
}

.auth-card {
  position: relative;
  overflow: hidden;
  background: rgba(255, 255, 255, 0.04) !important;
  border: 1px solid rgba(255, 215, 64, 0.14);
  backdrop-filter: blur(12px);
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.28);
}

.card-glow {
  position: absolute;
  inset: 0;
  background: radial-gradient(circle at 50% 0%, rgba(255, 215, 64, 0.12), transparent 70%);
  pointer-events: none;
}

:deep(.auth-input .v-field) {
  background: rgba(255, 255, 255, 0.03);
  border-radius: 14px;
}

:deep(.auth-input .v-field--focused) {
  box-shadow: 0 0 0 1px rgba(255, 215, 64, 0.25);
}

.auth-btn-primary {
  font-weight: 800;
  letter-spacing: 0.05rem;
  box-shadow: 0 4px 24px rgba(255, 111, 0, 0.4) !important;
  transition: all 0.3s ease !important;
}

.auth-btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 32px rgba(255, 111, 0, 0.5) !important;
}

.auth-btn-secondary {
  border-color: rgba(255, 255, 255, 0.2) !important;
}

.auth-divider {
  position: relative;
  text-align: center;
  color: rgba(255, 255, 255, 0.48);
  font-size: 0.85rem;
  letter-spacing: 0.08rem;
  text-transform: uppercase;
}

.auth-divider::before,
.auth-divider::after {
  content: '';
  position: absolute;
  top: 50%;
  width: calc(50% - 28px);
  height: 1px;
  background: rgba(255, 215, 64, 0.14);
}

.auth-divider::before {
  left: 0;
}

.auth-divider::after {
  right: 0;
}

@keyframes shimmer {
  0% { background-position: 0% center; }
  100% { background-position: 200% center; }
}
</style>
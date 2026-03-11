<template>
  <v-container class="fill-height d-flex align-center justify-center">
    <v-card elevation="8" max-width="460" rounded="xl" width="100%">
      <v-card-title class="text-h5 pt-6 px-6">
        Rejestracja
      </v-card-title>

      <v-card-text class="px-6">
        <v-form @submit.prevent="handleRegister">
          <v-text-field
            v-model="form.login"
            class="mb-3"
            :disabled="playerStore.isLoading"
            label="Login"
            prepend-inner-icon="mdi-account"
            required
            variant="outlined"
          />

          <v-text-field
            v-model="form.password"
            :append-inner-icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
            class="mb-3"
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
            type="error"
            variant="tonal"
          >
            {{ localError || playerStore.error }}
          </v-alert>

          <v-alert
            v-if="successMessage"
            class="mt-4"
            type="success"
            variant="tonal"
          >
            {{ successMessage }}
          </v-alert>

          <v-btn
            block
            class="mt-6"
            color="primary"
            :loading="playerStore.isLoading"
            size="large"
            type="submit"
          >
            Zarejestruj się
          </v-btn>
        </v-form>
      </v-card-text>

      <v-card-actions class="px-6 pb-6 d-flex justify-space-between">
        <span class="text-body-2">Masz już konto?</span>
        <v-btn
          color="primary"
          :to="{ name: 'login' }"
          variant="text"
        >
          Przejdź do logowania
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
  import { reactive, ref } from 'vue'
  import { useRouter } from 'vue-router'
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

  async function handleRegister () {
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

      successMessage.value = 'Konto zostało utworzone. Możesz się teraz zalogować.'

      setTimeout(() => {
        router.push({ name: 'login' })
      }, 800)
    } catch {
    // obsłużone w store
    }
  }
</script>

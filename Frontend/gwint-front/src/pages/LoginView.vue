<template>
  <v-container class="fill-height d-flex align-center justify-center">
    <v-card elevation="8" max-width="420" rounded="xl" width="100%">
      <v-card-title class="text-h5 pt-6 px-6">
        Logowanie
      </v-card-title>

      <v-card-text class="px-6">
        <v-form @submit.prevent="handleLogin">
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
            :disabled="playerStore.isLoading"
            label="Hasło"
            prepend-inner-icon="mdi-lock"
            required
            :type="showPassword ? 'text' : 'password'"
            variant="outlined"
            @click:append-inner="showPassword = !showPassword"
          />

          <v-alert
            v-if="playerStore.error"
            class="mt-4"
            type="error"
            variant="tonal"
          >
            {{ playerStore.error }}
          </v-alert>

          <v-btn
            block
            class="mt-6"
            color="primary"
            :loading="playerStore.isLoading"
            size="large"
            type="submit"
          >
            Zaloguj się
          </v-btn>
        </v-form>
      </v-card-text>

      <v-card-actions class="px-6 pb-6 d-flex justify-space-between">
        <span class="text-body-2">Nie masz konta?</span>
        <v-btn
          color="primary"
          :to="{ name: 'register' }"
          variant="text"
        >
          Zarejestruj się
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

  const form = reactive({
    login: '',
    password: '',
  })

  async function handleLogin () {
    playerStore.clearError()

    try {
      await playerStore.login({
        login: form.login.trim(),
        password: form.password,
      })

      await router.push({ name: 'home' })
    } catch {
    // obsłużone w store
    }
  }
</script>

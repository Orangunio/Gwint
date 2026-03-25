<template>
  <div>
    <div v-if="roomNotFound">
      <p>❌ Pokój <strong>{{ roomId }}</strong> nie istnieje.</p>
      <button @click="router.push('/')">Wróć do menu</button>
    </div>

    <div v-else-if="roomFull">
      <p>❌ Pokój <strong>{{ roomId }}</strong> jest pełny.</p>
      <button @click="router.push('/')">Wróć do menu</button>
    </div>

    <div v-else>
      <h2>Pokój: {{ roomId }}</h2>

      <table>
        <thead>
          <tr>
            <th>#</th>
            <th>Gracz</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(player, index) in players" :key="player">
            <td>{{ index + 1 }}</td>
            <td>{{ player }}</td>
            <td>✅ Gotowy</td>
          </tr>
          <tr v-if="players.length < 2">
            <td>{{ players.length + 1 }}</td>
            <td><em>Oczekiwanie na gracza...</em></td>
            <td>⏳</td>
          </tr>
        </tbody>
      </table>

      <p>Graczy w pokoju: {{ players.length }} / 2</p>

      <button @click="startGame" :disabled="players.length < 2">
        {{ players.length < 2 ? 'Czekaj na drugiego gracza...' : 'Start gry!' }}
      </button>

      <button @click="leaveRoom">Wyjdź z pokoju</button>

      <p v-if="gameStarted">Gra się zaczęła!</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSignalR } from '@/plugins/signalr'
import { usePlayerStore } from '@/stores/player'

const { invoke, on, off } = useSignalR()
const route = useRoute()
const router = useRouter()
const roomId = route.params.roomId

const playerStore = usePlayerStore()

const players = ref([])
const gameStarted = ref(false)
const roomNotFound = ref(false)
const roomFull = ref(false)

function onPlayersUpdated(logins) {
  players.value = logins
}

function onGameStarted() {
  gameStarted.value = true
}

function onRoomNotFound() {
  roomNotFound.value = true
}

function onRoomFull() {
  roomFull.value = true
}

onMounted(async () => {
  on('PlayersUpdated', onPlayersUpdated)
  on('GameStarted', onGameStarted)
  on('RoomNotFound', onRoomNotFound)
  on('RoomFull', onRoomFull)

  const alreadyJoined = history.state?.joined
  if (!alreadyJoined) {
    await invoke('JoinRoom', roomId, playerStore.displayName)
  } else {
    await invoke('GetPlayers', roomId)
  }
})

onUnmounted(() => {
  off('PlayersUpdated', onPlayersUpdated)
  off('GameStarted', onGameStarted)
  off('RoomNotFound', onRoomNotFound)
  off('RoomFull', onRoomFull)
})

async function leaveRoom() {
  try {
    await invoke('LeaveRoom', roomId)
  } catch (e) {
    console.error('Błąd przy wychodzeniu:', e)
  } finally {
    router.push('/')
  }
}

async function startGame() {
  await invoke('StartGame', roomId)
}
</script>
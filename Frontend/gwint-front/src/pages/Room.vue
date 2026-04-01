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
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSignalRStore } from '@/stores/signalr'
import { usePlayerStore } from '@/stores/player'

const route = useRoute()
const router = useRouter()
const signalRStore = useSignalRStore()
const playerStore = usePlayerStore()

const roomId = route.params.roomId
const players = ref([])
const roomNotFound = ref(false)
const roomFull = ref(false)

function onPlayersUpdated(logins) {
  players.value = logins
}

function onGameStarted() {
  router.push({ name: 'fraction-select', params: { roomId } })
}

function onRoomNotFound() {
  roomNotFound.value = true
}

function onRoomFull() {
  roomFull.value = true
}

onMounted(async () => {
  // Jeśli użytkownik odświeżył stronę – nie ma połączenia, wróć do lobby
  if (!signalRStore.isConnected) {
    await signalRStore.connectToRoom()
  }

  const conn = signalRStore.roomConnection
  conn.on('PlayersUpdated', onPlayersUpdated)
  conn.on('GameStarted', onGameStarted)
  conn.on('RoomNotFound', onRoomNotFound)
  conn.on('RoomFull', onRoomFull)

  // joined: true = przyszliśmy z CreateRoom/JoinRoom i już wywołaliśmy JoinRoom/CreateRoom
  // joined: false/brak = weszliśmy bezpośrednio przez URL, dopiero się łączymy
  const alreadyJoined = history.state?.joined
  if (!alreadyJoined) {
    await conn.invoke('JoinRoom', roomId, playerStore.displayName)
  } else {
    await conn.invoke('GetPlayers', roomId)
  }
})

onUnmounted(() => {
  const conn = signalRStore.roomConnection
  if (!conn) return
  conn.off('PlayersUpdated', onPlayersUpdated)
  conn.off('GameStarted', onGameStarted)
  conn.off('RoomNotFound', onRoomNotFound)
  conn.off('RoomFull', onRoomFull)
})

async function leaveRoom() {
  try {
    await signalRStore.roomConnection?.invoke('LeaveRoom', roomId)
  } catch (e) {
    console.error('Błąd przy wychodzeniu:', e)
  } finally {
    router.push('/')
  }
}

async function startGame() {
  await signalRStore.startRoomGame(roomId)
}
</script>
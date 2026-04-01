<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useSignalRStore } from '@/stores/signalr'
import { usePlayerStore } from '@/stores/player'

const router = useRouter()
const signalRStore = useSignalRStore()
const playerStore = usePlayerStore()
const code = ref('')

async function join() {
  if (!signalRStore.isConnected) {
    await signalRStore.connectToRoom()
  }
  // joinRoom() w store ustawia isHost = false i zapisuje roomId
  await signalRStore.joinRoom(code.value, playerStore.displayName)
  router.push({ name: 'Room', params: { roomId: code.value }, state: { joined: true } })
}
</script>

<template>
  <div>
    <input v-model="code" placeholder="Kod pokoju" />
    <button @click="join">Dołącz</button>
  </div>
</template>
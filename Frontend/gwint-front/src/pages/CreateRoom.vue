<script setup>
import { useRouter } from 'vue-router'
import { useSignalRStore } from '@/stores/signalr'
import { usePlayerStore } from '@/stores/player'

const router = useRouter()
const signalRStore = useSignalRStore()
const playerStore = usePlayerStore()

async function create() {
  if (!signalRStore.isConnected) {
    await signalRStore.connectToRoom()
  }
  // createRoom() w store ustawia isHost = true i zapisuje roomId
  const id = await signalRStore.createRoom(playerStore.displayName)
  router.push({ name: 'Room', params: { roomId: id }, state: { joined: true } })
}
</script>

<template>
  <div>
    <button @click="create">Utwórz pokój</button>
  </div>
</template>
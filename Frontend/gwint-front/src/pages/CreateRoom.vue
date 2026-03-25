<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useSignalR } from '@/plugins/signalr'
import { usePlayerStore } from '@/stores/player'

const { invoke } = useSignalR()
const router = useRouter()
const playerStore = usePlayerStore()

async function create() {
  const id = await invoke('CreateRoom', playerStore.displayName)
  router.push({ name: 'Room', params: { roomId: id }, state: { joined: true } })
}
</script>

<template>
  <div>
    <button @click="create">Utwórz pokój</button>
  </div>
</template>
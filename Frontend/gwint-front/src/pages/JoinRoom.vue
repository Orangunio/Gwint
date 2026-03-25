<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useSignalR } from '@/plugins/signalr'
import { usePlayerStore } from '@/stores/player'

const { invoke } = useSignalR()
const router = useRouter()
const playerStore = usePlayerStore()
const code = ref('')

async function join() {
  await invoke('JoinRoom', code.value, playerStore.displayName)
  router.push({ name: 'Room', params: { roomId: code.value }, state: { joined: true } })
}
</script>

<template>
  <div>
    <input v-model="code" placeholder="Kod pokoju" />
    <button @click="join">Dołącz</button>
  </div>
</template>
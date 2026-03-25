import * as signalR from '@microsoft/signalr'
import { ref } from 'vue'

const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:7117/roomHub')
  .withAutomaticReconnect()
  .build()

const isConnected = ref(false)

async function start() {
  if (connection.state === signalR.HubConnectionState.Disconnected) {
    await connection.start()
    isConnected.value = true
  }
}

async function invoke(method, ...args) {
  await start()
  return connection.invoke(method, ...args)
}

function on(event, handler) {
  connection.on(event, handler)
}

function off(event, handler) {
  connection.off(event, handler)
}

export function useSignalR() {
  return { connection, isConnected, start, invoke, on, off }
}
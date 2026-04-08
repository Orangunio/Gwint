import { defineStore } from 'pinia'
import * as signalR from '@microsoft/signalr'
import { usePlayerStore } from './player'
import router from '@/router'

const HUB_URL = 'http://localhost:5006'

export interface GameCard {
  id: number
  name: string
  strength: number
  finalStrength: number
  place: number
  fraction: number
  ability: number
  isChampion: boolean
  isCommander: boolean
  isSpecial: boolean
}

export interface GameBoard {
  player1FirstCardRow: GameCard[]
  player1SecondCardRow: GameCard[]
  player1ThirdCardRow: GameCard[]
  player2FirstCardRow: GameCard[]
  player2SecondCardRow: GameCard[]
  player2ThirdCardRow: GameCard[]
  rowScores: number[][]
  rogDowodcyActive: boolean[][]
  frostActive: boolean
  fogActive: boolean
  rainActive: boolean
}

export interface GamePlayer {
  id: number
  login: string
  connectionId: string
}

export interface GameState {
  roomId: string
  player1: GamePlayer | null
  player2: GamePlayer | null
  player1CardsOnHand: GameCard[]
  player2CardsOnHand: GameCard[]
  player1CardsOnDisplay: GameCard[]
  player2CardsOnDisplay: GameCard[]
  player1CardsInDeck: GameCard[]
  player2CardsInDeck: GameCard[]
  player1CommanderCard: GameCard | null
  player2CommanderCard: GameCard | null
  board: GameBoard | null
  currentPlayer: GamePlayer | null
  player1Passed: boolean
  player2Passed: boolean
  player1RoundsWon: number
  player2RoundsWon: number
}

export type GameResult = 'win' | 'lose' | 'draw' | null

interface SignalRState {
  roomConnection: signalR.HubConnection | null
  gameConnection: signalR.HubConnection | null
  roomConnectionId: string | null
  gameConnectionId: string | null

  roomId: string | null
  roomPlayers: string[]
  isRoomReady: boolean
  isHost: boolean

  game: GameState | null
  myTurn: boolean
  pendingAction: 'agility' | 'resurrection' | 'decoy' | 'horn' | 'revealCards' | 'revealResurrection' | 'opponentResurrection' | null
  pendingCardId: number | null
  revealedOpponentCards: GameCard[]

  selectedFraction: number | null
  opponentSelectedFraction: number | null
  bothFractionsSelected: boolean

  gameResult: GameResult
  gameResultWinner: string | null

  isConnecting: boolean
  error: string | null

  randomResurrectionCard: GameCard | null
}

export const useSignalRStore = defineStore('signalr', {
  state: (): SignalRState => ({
    roomConnection: null,
    gameConnection: null,
    roomConnectionId: null,
    gameConnectionId: null,

    roomId: null,
    roomPlayers: [],
    isRoomReady: false,
    isHost: false,

    game: null,
    myTurn: false,
    pendingAction: null,
    pendingCardId: null,
    revealedOpponentCards: [],

    selectedFraction: null,
    opponentSelectedFraction: null,
    bothFractionsSelected: false,

    gameResult: null,
    gameResultWinner: null,

    isConnecting: false,
    error: null,

    randomResurrectionCard: null,
  }),

  getters: {
    isConnected: (state) => state.roomConnection?.state === signalR.HubConnectionState.Connected,
    isGameConnected: (state) => state.gameConnection?.state === signalR.HubConnectionState.Connected,

    myPlayer(state): GamePlayer | null {
      if (!state.game || !state.roomConnectionId) return null
      if (state.game.player1?.connectionId === state.roomConnectionId) return state.game.player1
      if (state.game.player2?.connectionId === state.roomConnectionId) return state.game.player2
      return null
    },

    opponentPlayer(state): GamePlayer | null {
      if (!state.game || !state.roomConnectionId) return null
      if (state.game.player1?.connectionId === state.roomConnectionId) return state.game.player2
      return state.game.player1
    },

    amIPlayer1(state): boolean {
      if (!state.game || !state.roomConnectionId) return false
      return state.game.player1?.connectionId === state.roomConnectionId
    },

    amIHost(state): boolean {
      return state.isHost
    },

    myHand(state): GameCard[] {
      if (!state.game || !state.roomConnectionId) return []
      return state.game.player1?.connectionId === state.roomConnectionId
        ? state.game.player1CardsOnHand
        : state.game.player2CardsOnHand
    },

    myCommander(state): GameCard | null {
      if (!state.game || !state.roomConnectionId) return null
      return state.game.player1?.connectionId === state.roomConnectionId
        ? state.game.player1CommanderCard
        : state.game.player2CommanderCard
    },

    myGraveyard(state): GameCard[] {
      if (!state.game || !state.roomConnectionId) return []
      return state.game.player1?.connectionId === state.roomConnectionId
        ? state.game.player1CardsOnDisplay
        : state.game.player2CardsOnDisplay
    },

    opponentGraveyard(state): GameCard[] {
      if (!state.game || !state.roomConnectionId) return []
      return state.game.player1?.connectionId === state.roomConnectionId
        ? state.game.player2CardsOnDisplay
        : state.game.player1CardsOnDisplay
    },
  },

  actions: {

    async connectToRoom() {
      if (this.roomConnection?.state === signalR.HubConnectionState.Connected) return

      this.isConnecting = true
      this.error = null

      const conn = new signalR.HubConnectionBuilder()
        .withUrl(`${HUB_URL}/roomHub`)
        .withAutomaticReconnect()
        .build()

      this.setupRoomListeners(conn)

      try {
        await conn.start()
        this.roomConnection = conn
        this.roomConnectionId = conn.connectionId
      } catch (e) {
        this.error = 'Nie można połączyć z serwerem.'
        throw e
      } finally {
        this.isConnecting = false
      }
    },

    setupRoomListeners(conn: signalR.HubConnection) {
      conn.on('PlayerJoined', () => {
        this.isRoomReady = true
      })

      conn.on('PlayerLeft', (login: string) => {
        this.roomPlayers = this.roomPlayers.filter(p => p !== login)
        this.isRoomReady = false
      })
    },

    async createRoom(playerName: string): Promise<string> {
      if (!this.roomConnection) throw new Error('Brak połączenia')

      const roomId = await this.roomConnection.invoke<string>('CreateRoom', playerName)
      this.roomId = roomId
      this.roomPlayers = [playerName]
      this.isHost = true
      return roomId
    },

    async joinRoom(roomId: string, playerName: string) {
      if (!this.roomConnection) throw new Error('Brak połączenia')

      await this.roomConnection.invoke('JoinRoom', roomId, playerName)
      this.roomId = roomId
      this.isHost = false
    },

    async startRoomGame(roomId: string) {
      if (!this.roomConnection) throw new Error('Brak połączenia')
      await this.roomConnection.invoke('StartGame', roomId)
    },

    async connectToGame() {
      if (this.gameConnection?.state === signalR.HubConnectionState.Connected) return

      const conn = new signalR.HubConnectionBuilder()
        .withUrl(`${HUB_URL}/gameHub`)
        .withAutomaticReconnect()
        .build()

      this.setupGameListeners(conn)

      try {
        await conn.start()
        this.gameConnection = conn
        this.gameConnectionId = conn.connectionId

        if (this.roomId) {
          await conn.invoke('JoinGameRoom', this.roomId, this.roomConnectionId)
        }

      } catch (e) {
        this.error = 'Nie można połączyć z GameHub.'
        throw e
      }
    },

    setupGameListeners(conn: signalR.HubConnection) {
      conn.on('GameStarted', (game: GameState) => {
        console.log('GameStarted, mój connectionId:', this.gameConnectionId)
        this.game = JSON.parse(JSON.stringify(game))
        this.myTurn = false
        this.pendingAction = null
        this.pendingCardId = null
        this.revealedOpponentCards = []
        this.gameResult = null
        this.gameResultWinner = null
      })

      conn.on('GameStateUpdated', (game: GameState) => {
        this.game = JSON.parse(JSON.stringify(game))
      })

      conn.on('TurnStarted', (connectionId: string) => {
        this.myTurn = connectionId === this.roomConnectionId
        this.pendingAction = null
        this.pendingCardId = null
        this.revealedOpponentCards = []
      })

      conn.on('NextTurn', (connectionId: string) => {
        this.myTurn = connectionId === this.roomConnectionId
        this.pendingAction = null
        this.pendingCardId = null
        this.revealedOpponentCards = []
      })

      conn.on('RoundStarted', ({ game, currentPlayerId }) => {
        this.game = game
        this.myTurn = currentPlayerId === this.roomConnectionId
        this.revealedOpponentCards = []
      })

      conn.on('Player1WonGame', (game: GameState) => {
        this.game = JSON.parse(JSON.stringify(game))
        this.myTurn = false
        const isPlayer1 = game.player1?.connectionId === this.roomConnectionId
        this.gameResult = isPlayer1 ? 'win' : 'lose'
        this.gameResultWinner = game.player1?.login ?? 'Gracz 1'
      })

      conn.on('Player2WonGame', (game: GameState) => {
        this.game = JSON.parse(JSON.stringify(game))
        this.myTurn = false
        const isPlayer2 = game.player2?.connectionId === this.roomConnectionId
        this.gameResult = isPlayer2 ? 'win' : 'lose'
        this.gameResultWinner = game.player2?.login ?? 'Gracz 2'
      })

      conn.on('ScoiataelChooseFirstPlayer', () => {
        this.pendingAction = 'agility'
      })

      conn.on('RequestAgilityRow', (cardId: number) => {
        this.pendingAction = 'agility'
        this.pendingCardId = cardId
      })

      conn.on('RequestResurrectionTarget', () => {
        this.pendingAction = 'resurrection'
      })

      conn.on('SelectCardToDecoy', (cardId: number) => {
        this.pendingAction = 'decoy'
        this.pendingCardId = cardId
      })

      conn.on('RequestHornRow', (cardId: number) => {
        this.pendingAction = 'horn'
        this.pendingCardId = cardId
      })

      conn.on('RevealOpponentCards', (cards: GameCard[]) => {
        this.revealedOpponentCards = cards
        this.pendingAction = 'revealCards'
      })

      conn.on('RandomResurrectionResult', (card: GameCard) => {
        this.randomResurrectionCard = card
        this.pendingAction = 'revealResurrection'
      })

      conn.on("RequestOpponentResurrection", () => {
        this.pendingAction = "opponentResurrection"
      })
    },

    async startGame(fraction1: number, fraction2: number) {
      if (!this.gameConnection || !this.roomId) throw new Error('Brak połączenia')
      await this.gameConnection.invoke('StartGame', this.roomId, fraction1, fraction2)
    },

    async chooseFirstPlayer() {
      if (!this.gameConnection || !this.roomId) throw new Error('Brak połączenia')
      await this.gameConnection.invoke('ChooseFirstPlayer', this.roomId)
    },

    async setFirstPlayer(connectionId: string) {
      if (!this.gameConnection || !this.roomId) throw new Error('Brak połączenia')
      await this.gameConnection.invoke('SetFirstPlayer', this.roomId, connectionId)
    },

    async playCard(card: GameCard, selectedRow?: number) {
      if (!this.gameConnection || !this.roomId) throw new Error('Brak połączenia')
      await this.gameConnection.invoke('PlayCard', this.roomId, card, selectedRow ?? null)
    },

    async playerPass() {
      if (!this.gameConnection || !this.roomId) throw new Error('Brak połączenia')
      await this.gameConnection.invoke('PlayerPass', this.roomId)
    },

    async resolveAgility(cardId: number, row: number) {
      if (!this.gameConnection || !this.roomId) throw new Error('Brak połączenia')
      const card = this.myHand.find(c => c.id === cardId) ?? null
      if (!card) return
      await this.gameConnection.invoke('PlayCard', this.roomId, card, row)
      this.pendingAction = null
    },

    async resolveResurrection(cardId: number) {
      if (!this.gameConnection || !this.roomId) throw new Error('Brak połączenia')
      await this.gameConnection.invoke('ResolveResurrection', this.roomId, cardId)
      this.pendingAction = null
    },

    async resolveDecoy(targetCardId: number) {
      if (!this.gameConnection || !this.roomId) throw new Error('Brak połączenia')
      await this.gameConnection.invoke('ResolveDecoy', this.roomId, targetCardId)
      this.pendingAction = null
    },

    async resolveHorn(row: number) {
      if (!this.gameConnection || !this.roomId) return
      await this.gameConnection.invoke('ResolveHorn', this.roomId, row)
      this.pendingAction = null
    },

    async confirmReveal() {
      if (!this.gameConnection || !this.roomId) return
      await this.gameConnection.invoke('ConfirmReveal', this.roomId)
      this.revealedOpponentCards = []
      this.pendingAction = null
    },

    async resolveOpponentResurrection(cardId: number) {
      if (!this.gameConnection || !this.roomId) return
      await this.gameConnection.invoke(
        "ResolveOpponentResurrection",
        this.roomId,
        cardId
      )

      this.pendingAction = null
    },

    clearGameResult() {
      this.gameResult = null
      this.gameResultWinner = null
    },

    async disconnect() {
      await this.roomConnection?.stop()
      await this.gameConnection?.stop()
      this.roomConnection = null
      this.gameConnection = null
      this.roomConnectionId = null
      this.gameConnectionId = null
      this.game = null
      this.roomId = null
      this.roomPlayers = []
      this.isRoomReady = false
      this.isHost = false
      this.myTurn = false
      this.gameResult = null
      this.gameResultWinner = null
      this.revealedOpponentCards = []
      this.pendingAction = null
      this.pendingCardId = null
      this.randomResurrectionCard = null
    },
  },
})
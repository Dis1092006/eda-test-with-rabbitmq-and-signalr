import { defineStore } from 'pinia'
import { ref } from 'vue'

export interface Message {
  user: string
  message: string
  timestamp: Date
}

export type ConnectionStatus = 'connected' | 'disconnected' | 'reconnecting'

export const useMessagesStore = defineStore('messages', () => {
  const allMessages = ref<Message[]>([])
  const filteredMessages = ref<Message[]>([])
  const activeFilter = ref<string | null>(null)
  const status = ref<ConnectionStatus>('disconnected')

  function addMessage(msg: Message) {
    allMessages.value.push(msg)
  }

  function addFilteredMessage(msg: Message) {
    filteredMessages.value.push(msg)
  }

  function setFilter(filter: string) {
    activeFilter.value = filter
    filteredMessages.value = []
  }

  function setStatus(s: ConnectionStatus) {
    status.value = s
  }

  return { allMessages, filteredMessages, activeFilter, status, addMessage, addFilteredMessage, setFilter, setStatus }
})

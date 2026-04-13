import * as signalR from '@microsoft/signalr'
import { useMessagesStore } from '@/stores/messages'

const HUB_URL = 'https://localhost:7180/hub'

let connection: signalR.HubConnection | null = null

function getConnection(): signalR.HubConnection {
  if (connection) return connection

  connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL)
    .withAutomaticReconnect()
    .build()

  const store = useMessagesStore()

  connection.on('ReceiveMessage', (user: string, message: string) => {
    store.addMessage({ user, message, timestamp: new Date() })
  })

  connection.on('ReceiveFilteredMessage', (user: string, message: string) => {
    store.addFilteredMessage({ user, message, timestamp: new Date() })
  })

  connection.onreconnecting(() => store.setStatus('reconnecting'))

  connection.onreconnected(async () => {
    store.setStatus('connected')
    if (store.activeFilter) {
      await connection!.invoke('Subscribe', store.activeFilter)
    }
  })

  connection.onclose(() => store.setStatus('disconnected'))

  return connection
}

export async function startConnection(): Promise<void> {
  const store = useMessagesStore()
  const conn = getConnection()
  try {
    await conn.start()
    store.setStatus('connected')
  } catch (err) {
    store.setStatus('disconnected')
    console.error('SignalR connection failed:', err)
  }
}

export async function subscribe(filter: string): Promise<void> {
  const conn = getConnection()
  await conn.invoke('Subscribe', filter)
}

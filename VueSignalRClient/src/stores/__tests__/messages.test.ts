import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useMessagesStore } from '../messages'

describe('messages store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('adds a message to allMessages', () => {
    const store = useMessagesStore()
    store.addMessage({ user: 'Server', message: 'hello', timestamp: new Date() })
    expect(store.allMessages).toHaveLength(1)
    expect(store.allMessages[0].message).toBe('hello')
  })

  it('adds a filtered message to filteredMessages', () => {
    const store = useMessagesStore()
    store.addFilteredMessage({ user: 'Server', message: 'error occurred', timestamp: new Date() })
    expect(store.filteredMessages).toHaveLength(1)
    expect(store.filteredMessages[0].message).toBe('error occurred')
  })

  it('clears filteredMessages when setFilter is called', () => {
    const store = useMessagesStore()
    store.addFilteredMessage({ user: 'Server', message: 'old', timestamp: new Date() })
    store.setFilter('new-filter')
    expect(store.filteredMessages).toHaveLength(0)
    expect(store.activeFilter).toBe('new-filter')
  })

  it('overwrites activeFilter on successive setFilter calls', () => {
    const store = useMessagesStore()
    store.setFilter('first')
    store.setFilter('second')
    expect(store.activeFilter).toBe('second')
  })

  it('updates connection status', () => {
    const store = useMessagesStore()
    store.setStatus('connected')
    expect(store.status).toBe('connected')
    store.setStatus('reconnecting')
    expect(store.status).toBe('reconnecting')
  })
})

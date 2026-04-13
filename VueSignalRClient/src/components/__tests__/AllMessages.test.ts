import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { setActivePinia, createPinia, getActivePinia } from 'pinia'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { useMessagesStore } from '@/stores/messages'
import AllMessages from '../AllMessages.vue'

const vuetify = createVuetify({ components, directives })

function mountComponent() {
  return mount(AllMessages, {
    global: { plugins: [getActivePinia()!, vuetify] }
  })
}

describe('AllMessages.vue', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('renders "No messages yet" when store is empty', () => {
    const wrapper = mountComponent()
    expect(wrapper.text()).toContain('No messages yet')
  })

  it('renders messages from the store', () => {
    const store = useMessagesStore()
    store.addMessage({ user: 'Server', message: 'hello world', timestamp: new Date() })
    const wrapper = mountComponent()
    expect(wrapper.text()).toContain('hello world')
    expect(wrapper.text()).toContain('Server')
  })

  it('shows Disconnected chip when status is disconnected', () => {
    const store = useMessagesStore()
    store.setStatus('disconnected')
    const wrapper = mountComponent()
    expect(wrapper.text()).toContain('Disconnected')
  })

  it('shows Connected chip when status is connected', () => {
    const store = useMessagesStore()
    store.setStatus('connected')
    const wrapper = mountComponent()
    expect(wrapper.text()).toContain('Connected')
  })
})

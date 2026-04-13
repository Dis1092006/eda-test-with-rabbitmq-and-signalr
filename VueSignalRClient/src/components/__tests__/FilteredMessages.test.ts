import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { setActivePinia, createPinia, getActivePinia } from 'pinia'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { useMessagesStore } from '@/stores/messages'
import FilteredMessages from '../FilteredMessages.vue'

vi.mock('@/services/signalr', () => ({
  subscribe: vi.fn().mockResolvedValue(undefined)
}))

const vuetify = createVuetify({ components, directives })

function mountComponent() {
  return mount(FilteredMessages, {
    global: { plugins: [getActivePinia()!, vuetify] }
  })
}

describe('FilteredMessages.vue', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('shows "No filter" chip when no active filter', () => {
    const wrapper = mountComponent()
    expect(wrapper.text()).toContain('No filter')
  })

  it('shows active filter chip when store has activeFilter', () => {
    const store = useMessagesStore()
    store.setFilter('error')
    const wrapper = mountComponent()
    expect(wrapper.text()).toContain('Filter: error')
  })

  it('shows placeholder text when no filtered messages and no filter', () => {
    const wrapper = mountComponent()
    expect(wrapper.text()).toContain('Set a filter to start receiving messages')
  })

  it('shows "Waiting" text when filter is set but no messages yet', () => {
    const store = useMessagesStore()
    store.setFilter('error')
    const wrapper = mountComponent()
    expect(wrapper.text()).toContain('Waiting for matching messages')
  })

  it('renders filtered messages from store', () => {
    const store = useMessagesStore()
    store.setFilter('error')
    store.addFilteredMessage({ user: 'Server', message: 'error in db', timestamp: new Date() })
    const wrapper = mountComponent()
    expect(wrapper.text()).toContain('error in db')
  })
})

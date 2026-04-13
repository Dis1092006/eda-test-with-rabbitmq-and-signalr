<template>
  <v-card height="100%" class="d-flex flex-column">
    <v-card-title class="pa-4">Filtered Messages</v-card-title>
    <v-divider />
    <v-card-text class="flex-grow-1 d-flex flex-column">
      <div class="d-flex ga-2 mb-3">
        <v-text-field
          v-model="filterInput"
          label="Substring filter"
          density="compact"
          hide-details
          @keyup.enter="onSubscribe"
        />
        <v-btn color="primary" :disabled="!filterInput" @click="onSubscribe">
          Subscribe
        </v-btn>
      </div>
      <div class="mb-3">
        <v-chip v-if="store.activeFilter" color="primary" variant="tonal">
          Filter: {{ store.activeFilter }}
        </v-chip>
        <v-chip v-else color="secondary" variant="outlined">No filter</v-chip>
      </div>
      <v-list lines="two" style="flex: 1; overflow-y: auto">
        <v-list-item
          v-for="(msg, i) in store.filteredMessages"
          :key="i"
          :title="msg.user"
          :subtitle="msg.message"
        >
          <template #append>
            <span class="text-caption text-medium-emphasis">
              {{ formatTime(msg.timestamp) }}
            </span>
          </template>
        </v-list-item>
        <v-list-item v-if="store.filteredMessages.length === 0">
          <v-list-item-title class="text-medium-emphasis">
            {{ store.activeFilter ? 'Waiting for matching messages...' : 'Set a filter to start receiving messages' }}
          </v-list-item-title>
        </v-list-item>
      </v-list>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useMessagesStore } from '@/stores/messages'
import { subscribe } from '@/services/signalr'

const store = useMessagesStore()
const filterInput = ref('')

async function onSubscribe() {
  if (!filterInput.value) return
  store.setFilter(filterInput.value)
  try {
    await subscribe(filterInput.value)
  } catch (err) {
    console.error('Subscribe failed:', err)
  }
  filterInput.value = ''
}

function formatTime(date: Date): string {
  return date.toLocaleTimeString()
}
</script>

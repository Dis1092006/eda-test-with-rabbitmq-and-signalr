<template>
  <v-card height="100%" class="d-flex flex-column">
    <v-card-title class="d-flex align-center justify-space-between pa-4">
      All Messages
      <v-chip :color="statusColor" size="small" variant="tonal">
        {{ statusLabel }}
      </v-chip>
    </v-card-title>
    <v-divider />
    <v-card-text class="pa-0 flex-grow-1 overflow-hidden">
      <v-list lines="two" style="max-height: 100%; overflow-y: auto">
        <v-list-item
          v-for="(msg, i) in store.allMessages"
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
        <v-list-item v-if="store.allMessages.length === 0">
          <v-list-item-title class="text-medium-emphasis">No messages yet</v-list-item-title>
        </v-list-item>
      </v-list>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useMessagesStore } from '@/stores/messages'

const store = useMessagesStore()

const statusColor = computed(() => {
  if (store.status === 'connected') return 'success'
  if (store.status === 'reconnecting') return 'warning'
  return 'error'
})

const statusLabel = computed(() => {
  if (store.status === 'connected') return 'Connected'
  if (store.status === 'reconnecting') return 'Reconnecting...'
  return 'Disconnected'
})

function formatTime(date: Date): string {
  return date.toLocaleTimeString()
}
</script>

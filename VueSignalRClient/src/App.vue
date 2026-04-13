<template>
  <v-app>
    <v-app-bar color="primary" flat>
      <v-app-bar-title>SignalR Live Dashboard</v-app-bar-title>
    </v-app-bar>
    <v-main style="height: 100vh">
      <v-container fluid class="pa-4" style="height: calc(100% - 64px)">
        <v-row style="height: 100%">
          <v-col cols="6" style="height: 100%">
            <AllMessages />
          </v-col>
          <v-col cols="6" style="height: 100%">
            <FilteredMessages />
          </v-col>
        </v-row>
      </v-container>
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { startConnection } from '@/services/signalr'
import AllMessages from '@/components/AllMessages.vue'
import FilteredMessages from '@/components/FilteredMessages.vue'

onMounted(() => {
  startConnection()
})
</script>

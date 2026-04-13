# Vue.js SignalR Client — Design Spec

**Date:** 2026-04-13  
**Status:** Approved

## Overview

Add a Vue.js + TypeScript frontend (`VueSignalRClient`) to the existing EDA demo repository. The client connects to the SignalR hub at `https://localhost:7180/hub` and provides two features:

1. **Real-time message feed** — displays all messages broadcast by the hub (`ReceiveMessage` event).
2. **Server-side filtered feed** — user enters a substring, client sends it to the hub via `Subscribe(filter)`, and the hub replies only with messages containing that substring (`ReceiveFilteredMessage` event).

## Architecture

### Backend changes (SignalRHub)

**New: `FilterService`** — singleton registered in DI.  
Stores `ConcurrentDictionary<string connectionId, string filter>`.

**Modified: `MessageHub.cs`**  
Add two methods:
- `Subscribe(string filter)` — saves filter for current connection (`Context.ConnectionId`)
- `OnDisconnectedAsync(Exception?)` — removes connection from dictionary on disconnect

**Modified: `MessageController.cs`** (POST `/message`)  
Broadcasting logic:
- `ReceiveMessage(user, message)` → `Clients.All` (unchanged)
- `ReceiveFilteredMessage(user, message)` → only connections where `message.Contains(filter, StringComparison.OrdinalIgnoreCase)`; sent via `Clients.Client(connectionId)` per matching entry

**Modified: `Program.cs`**  
Add CORS policy allowing `http://localhost:5173` with credentials support (required for SignalR WebSocket).

### Frontend project (VueSignalRClient)

**Stack:** Vite + Vue 3 + TypeScript + Vuetify 3 + Pinia + @microsoft/signalr

**Directory layout:**
```
VueSignalRClient/
├── src/
│   ├── components/
│   │   ├── AllMessages.vue          # scrollable feed of all messages
│   │   └── FilteredMessages.vue     # filter input + scrollable filtered feed
│   ├── services/
│   │   └── signalr.ts               # connection init, auto-reconnect, event wiring
│   ├── stores/
│   │   └── messages.ts              # Pinia store: allMessages[], filteredMessages[], activeFilter
│   ├── App.vue                      # root: Vuetify two-column layout
│   └── main.ts
├── package.json
└── vite.config.ts
```

The project is **not added to the .sln** — it is a standalone npm project.

## UI Layout

Single page, two-column Vuetify layout:

**Left column — All Messages**
- `v-card` titled "All Messages"
- Connection status chip (Connected / Reconnecting... / Disconnected)
- Scrollable `v-list`: each item shows sender name, message text, timestamp

**Right column — Filtered Messages**
- `v-text-field` for substring input + "Subscribe" button
- Active filter chip (displays current filter or "No filter")
- Scrollable `v-list`: messages received via `ReceiveFilteredMessage`
- List clears when a new filter is submitted

## Data Flow

```
SignalR Hub
    │
    ├─ ReceiveMessage(user, message)
    │       └─▶ store.allMessages.push({ user, message, timestamp })
    │
    └─ ReceiveFilteredMessage(user, message)
            └─▶ store.filteredMessages.push({ user, message, timestamp })

User submits filter
    └─▶ hub.invoke("Subscribe", filter)
        └─▶ store.activeFilter = filter
            store.filteredMessages = []   // clear stale results
```

## Error Handling & Reconnection

- SignalR connection configured with `withAutomaticReconnect()`
- On disconnect: status chip turns red ("Disconnected")
- On reconnecting: status chip turns yellow ("Reconnecting...")
- On reconnected: active filter (if any) is re-sent via `Subscribe(activeFilter)`

## CORS

`SignalRHub/Program.cs` adds a named CORS policy:
```csharp
builder.Services.AddCors(options =>
    options.AddPolicy("VueDev", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));
```
Applied before `app.MapHub<MessageHub>("/hub")`.

## Out of Scope

- Sending messages from the Vue client (read-only + subscribe only)
- Multiple simultaneous filters per client
- Production build / Docker containerization of the Vue app
- Authentication / authorization

## README Update

Add a section to the existing `README.md` describing:
- Prerequisites: Node.js 20+
- How to run: `cd VueSignalRClient && npm install && npm run dev`
- URL: `http://localhost:5173`

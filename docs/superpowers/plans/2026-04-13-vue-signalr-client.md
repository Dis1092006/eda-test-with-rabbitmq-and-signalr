# Vue SignalR Client Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a Vue 3 + TypeScript frontend and server-side filtered broadcasting to the existing EDA demo.

**Architecture:** A new `FilterService` singleton in `SignalRHub` stores per-connection substring filters; `MessageController` broadcasts `ReceiveFilteredMessage` to matching connections when a message arrives. The `VueSignalRClient` npm project connects to the hub, displays all messages in one panel, and sends a `Subscribe(filter)` call to receive server-filtered messages in a second panel.

**Tech Stack:** Vue 3, TypeScript, Vite, Vuetify 3, Pinia, @microsoft/signalr (frontend) · ASP.NET Core 10, xUnit (backend)

---

## File Map

### New files
| Path | Responsibility |
|------|---------------|
| `SignalRHub/Services/FilterService.cs` | Thread-safe per-connection filter store |
| `SignalRHub.Tests/SignalRHub.Tests.csproj` | xUnit test project |
| `SignalRHub.Tests/FilterServiceTests.cs` | Unit tests for FilterService |
| `VueSignalRClient/` | Entire Vue project (Vite scaffold + custom files) |
| `VueSignalRClient/src/services/signalr.ts` | SignalR connection factory and event wiring |
| `VueSignalRClient/src/stores/messages.ts` | Pinia store: allMessages, filteredMessages, status |
| `VueSignalRClient/src/components/AllMessages.vue` | Left panel: real-time message feed |
| `VueSignalRClient/src/components/FilteredMessages.vue` | Right panel: filter input + filtered feed |
| `VueSignalRClient/src/stores/__tests__/messages.test.ts` | Vitest store tests |
| `VueSignalRClient/src/components/__tests__/AllMessages.test.ts` | Vitest component tests |
| `VueSignalRClient/src/components/__tests__/FilteredMessages.test.ts` | Vitest component tests |

### Modified files
| Path | Change |
|------|--------|
| `SignalRHub/MessageHub.cs` | Add `Subscribe` method + `OnDisconnectedAsync` |
| `SignalRHub/Controllers/MessageController.cs` | Inject `FilterService`, send `ReceiveFilteredMessage` |
| `SignalRHub/Program.cs` | Register `FilterService`, add CORS policy |
| `VueSignalRClient/src/main.ts` | Wire Vuetify + Pinia + start SignalR |
| `VueSignalRClient/src/App.vue` | Two-column Vuetify layout |
| `README.md` | Add VueSignalRClient section |

---

## Task 1: Create FilterService with unit tests (TDD)

**Files:**
- Create: `SignalRHub/Services/FilterService.cs`
- Create: `SignalRHub.Tests/SignalRHub.Tests.csproj`
- Create: `SignalRHub.Tests/FilterServiceTests.cs`

- [ ] **Step 1: Create the xUnit test project**

Run from repo root:
```bash
dotnet new xunit -n SignalRHub.Tests -o SignalRHub.Tests
dotnet sln EventDrivenArchitectureTest.sln add SignalRHub.Tests/SignalRHub.Tests.csproj
dotnet add SignalRHub.Tests/SignalRHub.Tests.csproj reference SignalRHub/SignalRHub.csproj
```

- [ ] **Step 2: Write failing tests for FilterService**

Create `SignalRHub.Tests/FilterServiceTests.cs`:
```csharp
using SignalRHub.Services;

namespace SignalRHub.Tests;

public class FilterServiceTests
{
    private readonly FilterService _sut = new();

    [Fact]
    public void GetMatchingConnections_ReturnsConnection_WhenMessageContainsFilter()
    {
        _sut.SetFilter("conn1", "hello");

        var result = _sut.GetMatchingConnections("Hello World");

        Assert.Contains("conn1", result);
    }

    [Fact]
    public void GetMatchingConnections_DoesNotReturnConnection_WhenMessageDoesNotContainFilter()
    {
        _sut.SetFilter("conn1", "xyz");

        var result = _sut.GetMatchingConnections("Hello World");

        Assert.DoesNotContain("conn1", result);
    }

    [Fact]
    public void GetMatchingConnections_IsCaseInsensitive()
    {
        _sut.SetFilter("conn1", "HELLO");

        var result = _sut.GetMatchingConnections("hello world");

        Assert.Contains("conn1", result);
    }

    [Fact]
    public void RemoveFilter_RemovesConnection()
    {
        _sut.SetFilter("conn1", "hello");
        _sut.RemoveFilter("conn1");

        var result = _sut.GetMatchingConnections("Hello World");

        Assert.DoesNotContain("conn1", result);
    }

    [Fact]
    public void SetFilter_OverwritesPreviousFilter()
    {
        _sut.SetFilter("conn1", "hello");
        _sut.SetFilter("conn1", "world");

        var matchesOld = _sut.GetMatchingConnections("hello test");
        var matchesNew = _sut.GetMatchingConnections("world test");

        Assert.DoesNotContain("conn1", matchesOld);
        Assert.Contains("conn1", matchesNew);
    }

    [Fact]
    public void GetMatchingConnections_ReturnsMultipleConnections_WhenMultipleMatch()
    {
        _sut.SetFilter("conn1", "error");
        _sut.SetFilter("conn2", "error");
        _sut.SetFilter("conn3", "info");

        var result = _sut.GetMatchingConnections("error occurred").ToList();

        Assert.Contains("conn1", result);
        Assert.Contains("conn2", result);
        Assert.DoesNotContain("conn3", result);
    }
}
```

- [ ] **Step 3: Run tests — expect build failure (FilterService doesn't exist yet)**

```bash
dotnet test SignalRHub.Tests/SignalRHub.Tests.csproj
```

Expected: build error — `The type or namespace name 'FilterService' could not be found`

- [ ] **Step 4: Implement FilterService**

Create `SignalRHub/Services/FilterService.cs`:
```csharp
using System.Collections.Concurrent;

namespace SignalRHub.Services;

public class FilterService
{
    private readonly ConcurrentDictionary<string, string> _filters = new();

    public void SetFilter(string connectionId, string filter) =>
        _filters[connectionId] = filter;

    public void RemoveFilter(string connectionId) =>
        _filters.TryRemove(connectionId, out _);

    public IEnumerable<string> GetMatchingConnections(string message) =>
        _filters
            .Where(kv => message.Contains(kv.Value, StringComparison.OrdinalIgnoreCase))
            .Select(kv => kv.Key);
}
```

- [ ] **Step 5: Run tests — expect all 6 passing**

```bash
dotnet test SignalRHub.Tests/SignalRHub.Tests.csproj --verbosity normal
```

Expected output:
```
Test run for SignalRHub.Tests.dll
Passed!  - Failed: 0, Passed: 6, Skipped: 0, Total: 6
```

- [ ] **Step 6: Commit**

```bash
git add SignalRHub/Services/FilterService.cs SignalRHub.Tests/
git commit -m "feat: add FilterService with unit tests"
```

---

## Task 2: Modify MessageHub to support Subscribe

**Files:**
- Modify: `SignalRHub/MessageHub.cs`

- [ ] **Step 1: Replace MessageHub.cs**

```csharp
using Microsoft.AspNetCore.SignalR;
using SignalRHub.Services;

namespace SignalRHub;

public class MessageHub : Hub
{
    private readonly FilterService _filterService;

    public MessageHub(FilterService filterService)
    {
        _filterService = filterService;
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public Task Subscribe(string filter)
    {
        _filterService.SetFilter(Context.ConnectionId, filter);
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _filterService.RemoveFilter(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
```

- [ ] **Step 2: Build to verify no errors**

```bash
dotnet build SignalRHub/SignalRHub.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add SignalRHub/MessageHub.cs
git commit -m "feat: add Subscribe method and cleanup on disconnect to MessageHub"
```

---

## Task 3: Modify MessageController to broadcast filtered messages

**Files:**
- Modify: `SignalRHub/Controllers/MessageController.cs`

- [ ] **Step 1: Replace MessageController.cs**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRHub.Services;

namespace SignalRHub.Controllers;

[ApiController]
[Route("[controller]")]
public class MessageController : ControllerBase
{
    private readonly IHubContext<MessageHub> _hubContext;
    private readonly FilterService _filterService;

    public MessageController(IHubContext<MessageHub> hubContext, FilterService filterService)
    {
        _hubContext = hubContext;
        _filterService = filterService;
    }

    [HttpPost]
    [Route("/message")]
    public async Task<IActionResult> Post([FromBody] string message)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Server", message);

        var matchingConnections = _filterService.GetMatchingConnections(message).ToList();
        if (matchingConnections.Count > 0)
        {
            await _hubContext.Clients.Clients(matchingConnections)
                .SendAsync("ReceiveFilteredMessage", "Server", message);
        }

        return Ok();
    }
}
```

- [ ] **Step 2: Build to verify no errors**

```bash
dotnet build SignalRHub/SignalRHub.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add SignalRHub/Controllers/MessageController.cs
git commit -m "feat: broadcast ReceiveFilteredMessage to subscribed connections"
```

---

## Task 4: Add CORS and DI registration to Program.cs

**Files:**
- Modify: `SignalRHub/Program.cs`

- [ ] **Step 1: Replace Program.cs**

```csharp
using Scalar.AspNetCore;
using SignalRHub;
using SignalRHub.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSingleton<FilterService>();

builder.Services.AddCors(options =>
    options.AddPolicy("VueDev", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors("VueDev");
app.UseAuthorization();

app.MapControllers();
app.MapHub<MessageHub>("/hub");

app.Run();
```

- [ ] **Step 2: Build and run briefly to verify startup**

```bash
dotnet build SignalRHub/SignalRHub.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add SignalRHub/Program.cs
git commit -m "feat: register FilterService singleton and add CORS policy for Vue dev server"
```

---

## Task 5: Scaffold Vue project and install dependencies

**Files:**
- Create: `VueSignalRClient/` (entire Vite scaffold)

- [ ] **Step 1: Scaffold the project**

Run from repo root:
```bash
npm create vite@latest VueSignalRClient -- --template vue-ts
```

- [ ] **Step 2: Install runtime dependencies**

```bash
cd VueSignalRClient
npm install
npm install vuetify @mdi/font pinia @microsoft/signalr
```

- [ ] **Step 3: Install dev dependencies**

```bash
npm install -D vitest @vue/test-utils jsdom
```

- [ ] **Step 4: Delete the scaffold placeholder component**

```bash
rm src/components/HelloWorld.vue
```

- [ ] **Step 5: Add vitest config to vite.config.ts**

Replace `VueSignalRClient/vite.config.ts` with:
```typescript
import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  test: {
    globals: true,
    environment: 'jsdom',
  }
})
```

- [ ] **Step 6: Add vitest types to tsconfig.app.json**

Open `VueSignalRClient/tsconfig.app.json` and add `"vitest/globals"` to the `compilerOptions.types` array. If there is no `types` array, add:
```json
"compilerOptions": {
  ...existing options...,
  "types": ["vitest/globals"]
}
```

- [ ] **Step 7: Add test script to package.json**

In `VueSignalRClient/package.json`, add to `"scripts"`:
```json
"test": "vitest run"
```

- [ ] **Step 8: Commit**

```bash
cd ..
git add VueSignalRClient/
git commit -m "chore: scaffold VueSignalRClient with Vite, Vuetify, Pinia, SignalR"
```

---

## Task 6: Create Pinia messages store with Vitest tests (TDD)

**Files:**
- Create: `VueSignalRClient/src/stores/messages.ts`
- Create: `VueSignalRClient/src/stores/__tests__/messages.test.ts`

- [ ] **Step 1: Write failing store tests**

Create `VueSignalRClient/src/stores/__tests__/messages.test.ts`:
```typescript
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
```

- [ ] **Step 2: Run tests — expect failure (store doesn't exist)**

```bash
cd VueSignalRClient
npm test
```

Expected: error — `Cannot find module '../messages'`

- [ ] **Step 3: Implement the messages store**

Create `VueSignalRClient/src/stores/messages.ts`:
```typescript
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
```

- [ ] **Step 4: Run tests — expect all 5 passing**

```bash
npm test
```

Expected:
```
✓ src/stores/__tests__/messages.test.ts (5)
Test Files  1 passed (1)
Tests  5 passed (5)
```

- [ ] **Step 5: Commit**

```bash
cd ..
git add VueSignalRClient/src/stores/
git commit -m "feat: add Pinia messages store with vitest tests"
```

---

## Task 7: Create SignalR service

**Files:**
- Create: `VueSignalRClient/src/services/signalr.ts`

- [ ] **Step 1: Create signalr.ts**

Create `VueSignalRClient/src/services/signalr.ts`:
```typescript
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
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd VueSignalRClient
npx tsc --noEmit
```

Expected: no errors.

- [ ] **Step 3: Commit**

```bash
cd ..
git add VueSignalRClient/src/services/signalr.ts
git commit -m "feat: add SignalR connection service with auto-reconnect"
```

---

## Task 8: Create AllMessages.vue with tests (TDD)

**Files:**
- Create: `VueSignalRClient/src/components/__tests__/AllMessages.test.ts`
- Create: `VueSignalRClient/src/components/AllMessages.vue`

- [ ] **Step 1: Write failing component tests**

Create `VueSignalRClient/src/components/__tests__/AllMessages.test.ts`:
```typescript
import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { useMessagesStore } from '@/stores/messages'
import AllMessages from '../AllMessages.vue'

const vuetify = createVuetify({ components, directives })

function mountComponent() {
  return mount(AllMessages, {
    global: { plugins: [createPinia(), vuetify] }
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
```

- [ ] **Step 2: Run tests — expect failure (component doesn't exist)**

```bash
cd VueSignalRClient
npm test
```

Expected: error — `Cannot find module '../AllMessages.vue'`

- [ ] **Step 3: Implement AllMessages.vue**

Create `VueSignalRClient/src/components/AllMessages.vue`:
```vue
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
```

- [ ] **Step 4: Run tests — expect all passing**

```bash
npm test
```

Expected:
```
✓ src/stores/__tests__/messages.test.ts (5)
✓ src/components/__tests__/AllMessages.test.ts (4)
Test Files  2 passed (2)
Tests  9 passed (9)
```

- [ ] **Step 5: Commit**

```bash
cd ..
git add VueSignalRClient/src/components/AllMessages.vue VueSignalRClient/src/components/__tests__/AllMessages.test.ts
git commit -m "feat: add AllMessages component with vitest tests"
```

---

## Task 9: Create FilteredMessages.vue with tests (TDD)

**Files:**
- Create: `VueSignalRClient/src/components/__tests__/FilteredMessages.test.ts`
- Create: `VueSignalRClient/src/components/FilteredMessages.vue`

- [ ] **Step 1: Write failing component tests**

Create `VueSignalRClient/src/components/__tests__/FilteredMessages.test.ts`:
```typescript
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
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
    global: { plugins: [createPinia(), vuetify] }
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

  it('shows placeholder text when no filtered messages', () => {
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
```

- [ ] **Step 2: Run tests — expect failure (component doesn't exist)**

```bash
cd VueSignalRClient
npm test
```

Expected: error — `Cannot find module '../FilteredMessages.vue'`

- [ ] **Step 3: Implement FilteredMessages.vue**

Create `VueSignalRClient/src/components/FilteredMessages.vue`:
```vue
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
  await subscribe(filterInput.value)
  filterInput.value = ''
}

function formatTime(date: Date): string {
  return date.toLocaleTimeString()
}
</script>
```

- [ ] **Step 4: Run all tests — expect all passing**

```bash
npm test
```

Expected:
```
✓ src/stores/__tests__/messages.test.ts (5)
✓ src/components/__tests__/AllMessages.test.ts (4)
✓ src/components/__tests__/FilteredMessages.test.ts (5)
Test Files  3 passed (3)
Tests  14 passed (14)
```

- [ ] **Step 5: Commit**

```bash
cd ..
git add VueSignalRClient/src/components/FilteredMessages.vue VueSignalRClient/src/components/__tests__/FilteredMessages.test.ts
git commit -m "feat: add FilteredMessages component with vitest tests"
```

---

## Task 10: Wire up App.vue and main.ts

**Files:**
- Modify: `VueSignalRClient/src/App.vue`
- Modify: `VueSignalRClient/src/main.ts`

- [ ] **Step 1: Replace App.vue**

```vue
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
```

- [ ] **Step 2: Replace main.ts**

```typescript
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import 'vuetify/styles'
import '@mdi/font/css/materialdesignicons.css'
import App from './App.vue'

const vuetify = createVuetify({ components, directives, theme: { defaultTheme: 'light' } })

createApp(App)
  .use(createPinia())
  .use(vuetify)
  .mount('#app')
```

- [ ] **Step 3: Run all tests to confirm nothing is broken**

```bash
cd VueSignalRClient
npm test
```

Expected: all 14 tests pass.

- [ ] **Step 4: Do a production build to catch type errors**

```bash
npm run build
```

Expected: `✓ built in Xs` (no errors).

- [ ] **Step 5: Commit**

```bash
cd ..
git add VueSignalRClient/src/App.vue VueSignalRClient/src/main.ts
git commit -m "feat: wire up App.vue with Vuetify layout and SignalR startup"
```

---

## Task 11: Update README.md

**Files:**
- Modify: `README.md`

- [ ] **Step 1: Add VueSignalRClient section to README**

In `README.md`, add the following after the `### SignalRClient` section and before `---`:

```markdown
### VueSignalRClient
A Vue 3 + TypeScript browser frontend that connects to the SignalR Hub.
- Displays all incoming messages in a real-time feed (left panel).
- Accepts a substring filter, sends it to the hub via `Subscribe(filter)`, and displays only server-filtered messages in a second panel (right panel).
- Auto-reconnects on connection loss and re-registers the active filter after reconnect.

**Dependencies:** Vue 3, Vite, Vuetify 3, Pinia, @microsoft/signalr

**Prerequisites:** Node.js 20+
```

Then update the **Architecture Overview** diagram to include the Vue client:

```
┌──────────────────┐    RabbitMQ Queue    ┌──────────────────┐    HTTP POST    ┌──────────────────┐   SignalR (WS)   ┌──────────────────┐
│  RabbitMQProducer│ ──────────────────▶  │ RabbitMQConsumer │ ──────────────▶ │   SignalRHub     │ ───────────────▶ │  SignalRClient   │
│  (console app)   │                      │  (background svc)│                 │ (ASP.NET Web API)│                  │  (console app)   │
└──────────────────┘                      └──────────────────┘                 └──────────────────┘                  └──────────────────┘
                                                                                        │                SignalR (WS)   ┌──────────────────┐
                                                                                        └─────────────────────────────▶ │ VueSignalRClient │
                                                                                                                        │  (browser app)   │
                                                                                                                        └──────────────────┘
```

Then add a new top-level section **Running VueSignalRClient** before the **Project Structure** section:

```markdown
## Running VueSignalRClient

### Prerequisites
- Node.js 20 or later
- The .NET developer certificate must be trusted in your browser. If you see a certificate error when the app tries to connect, navigate to `https://localhost:7180/scalar/v1` once and accept the certificate warning, then reload the Vue app.

### Step 1 — Install dependencies

```powershell
cd VueSignalRClient
npm install
```

### Step 2 — Start the dev server

```powershell
npm run dev
```

The app will be available at `http://localhost:5173`.

### Step 3 — Start SignalRHub (if not already running)

```powershell
cd SignalRHub
dotnet run
```

### How to use

1. Open `http://localhost:5173` in your browser.
2. The left panel will show all messages as they arrive.
3. In the right panel, type a substring in the filter field and click **Subscribe**. Only messages containing that substring (case-insensitive) will appear in the right panel.
4. You can change the filter at any time by entering a new value and clicking **Subscribe** again.

### Running tests

```powershell
cd VueSignalRClient
npm test
```
```

- [ ] **Step 2: Commit**

```bash
git add README.md
git commit -m "docs: add VueSignalRClient section to README"
```

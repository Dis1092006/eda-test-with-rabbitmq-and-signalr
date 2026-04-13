# EDA Test with RabbitMQ and SignalR

A proof-of-concept project demonstrating **Event-Driven Architecture (EDA)** using **RabbitMQ** as a message broker and **ASP.NET Core SignalR** for real-time push notifications to connected clients.

All projects target **.NET 10**.

---

## Architecture Overview

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

**Message flow:**
1. The user types a message in **RabbitMQProducer**.
2. The message is published to a RabbitMQ queue (`some-queue`, exchange `amq.direct`).
3. **RabbitMQConsumer** picks it up and forwards it via `HTTP POST` to the SignalR Hub.
4. **SignalRHub** broadcasts it to all connected SignalR clients.
5. **SignalRClient** receives and prints the message in real time.

---

## Projects

### RabbitMQProducer
A console application that publishes messages to RabbitMQ.
- Connects to RabbitMQ on `localhost:5672`.
- Declares queue `some-queue` bound to exchange `amq.direct` with routing key `some-routing-key`.
- Reads user input in a loop and publishes each line as a message.
- Type `exit` to quit.

**NuGet dependencies:** `RabbitMQ.Client 7.2.1`

### RabbitMQConsumer
A console application hosting a `BackgroundService` that consumes messages from RabbitMQ.
- Subscribes to the same `some-queue` queue.
- On each received message, sends an HTTP POST request to `https://localhost:7180/message` (SignalRHub).

**NuGet dependencies:** `RabbitMQ.Client 7.2.1`, `Microsoft.Extensions.Hosting 10.0.0`

### SignalRHub
An ASP.NET Core Web API + SignalR server — the central message hub.
- Listens on `https://localhost:7180`.
- **REST endpoint** `POST /message`: accepts a JSON string and broadcasts it to all connected SignalR clients via the `ReceiveMessage` event.
- **SignalR Hub** at `/hub` with method `SendMessage(user, message)`.
- Interactive API docs available at `/scalar/v1` (in Development mode) via [Scalar](https://scalar.com).

**NuGet dependencies:** `Microsoft.AspNetCore.OpenApi 10.0.0`, `Scalar.AspNetCore 2.13.21`

### SignalRClient
A console application that connects to the SignalR Hub and prints incoming messages.
- Connects to `https://localhost:7180/hub`.
- Listens for the `ReceiveMessage` event and prints `{user}: {message}` to the console.

**NuGet dependencies:** `Microsoft.AspNetCore.SignalR.Client 10.0.0`

### VueSignalRClient
A Vue 3 + TypeScript browser frontend that connects to the SignalR Hub.
- Displays all incoming messages in a real-time feed (left panel).
- Accepts a substring filter, sends it to the hub via `Subscribe(filter)`, and displays only server-filtered messages in a second panel (right panel).
- Auto-reconnects on connection loss and re-registers the active filter after reconnect.

**Dependencies:** Vue 3, Vite, Vuetify 4, Pinia, @microsoft/signalr

**Prerequisites:** Node.js 20+

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- **RabbitMQ** running on `localhost:5672`

The easiest way to run RabbitMQ locally is via Docker:

```bash
docker run -d --hostname my-rabbit --name rabbitmq \
  -p 5672:5672 -p 15672:15672 \
  rabbitmq:3-management
```

RabbitMQ Management UI will be available at `http://localhost:15672` (login: `guest` / `guest`).

---

## Full End-to-End Testing

Run each project in a **separate terminal**, starting in the order below.

### Step 1 — Start SignalRHub

```powershell
cd SignalRHub
dotnet run
```

Wait until you see:
```
Now listening on: https://localhost:7180
```

### Step 2 — Start SignalRClient

```powershell
cd SignalRClient
dotnet run
```

Output:
```
SignalR client starting ...
```

The client is now connected and waiting for messages.

### Step 3 — Start RabbitMQConsumer

```powershell
cd RabbitMQConsumer
dotnet run
```

Output:
```
Consumer starting ...
```

### Step 4 — Start RabbitMQProducer

```powershell
cd RabbitMQProducer
dotnet run
```

Type any message and press **Enter**:
```
Enter message payload (print "exit" to finish):
Hello World
Message sent
```

### Expected result

| Terminal          | Output                        |
|-------------------|-------------------------------|
| RabbitMQConsumer  | `Message received: Hello World` |
| SignalRClient     | `Server: Hello World`         |

This confirms the full pipeline:
**User input → RabbitMQ → Consumer → HTTP → SignalR Hub → WebSocket → SignalR Client**

---

## Testing the SignalR Part in Isolation (without RabbitMQ)

You only need **SignalRHub** and **SignalRClient** for this scenario.

### Step 1 — Start SignalRHub

```powershell
cd SignalRHub
dotnet run
```

### Step 2 — Start SignalRClient

```powershell
cd SignalRClient
dotnet run
```

### Step 3 — Send a message

Choose one of the following options:

#### Option A: curl

PowerShell 5.1 strips double quotes when passing arguments to external executables.
Use the stop-parsing symbol `--%` to bypass this:

```powershell
curl.exe --% -k -X POST https://localhost:7180/message -H "Content-Type: application/json" -d "\"Hello from curl!\""
```

> **Notes:**
> - `--% ` (stop-parsing symbol) tells PowerShell to pass everything after it verbatim to the process, without any quote processing.
> - The `-k` flag skips SSL certificate validation (the dev certificate is self-signed).
> - The message body must be a valid **JSON string**, i.e. surrounded by double quotes: `"Hello from curl!"`.

A successful call returns HTTP **200 OK** with an empty body.

#### Option B: Scalar API Reference

1. Open your browser and navigate to:
   ```
   https://localhost:7180/scalar/v1
   ```
2. If the browser shows a certificate warning, click **Advanced → Proceed to localhost**.
3. Find the **POST /message** endpoint in the list.
4. Click on it, then click **Test Request**.
5. In the **Body** section set the content type to `application/json` and enter a JSON string (quotes are required):
   ```json
   "Hello from Scalar!"
   ```
6. Click **Send**.
7. Verify the response status is **200**.

### Step 4 — Check the result

Switch to the **SignalRClient** terminal. You should see:

```
Server: Hello from curl!
```
or
```
Server: Hello from Scalar!
```

You can send as many requests as you like — every message will appear in the client console instantly, confirming that the **HTTP → SignalR Hub → WebSocket → Client** pipeline works correctly.

---

## Running VueSignalRClient

### Prerequisites
- Node.js 20 or later
- The .NET developer certificate must be trusted in your browser. If you see a connection error, navigate to `https://localhost:7180/scalar/v1` once, accept the certificate warning, then reload the Vue app.

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
2. The left panel shows all messages as they arrive.
3. In the right panel, type a substring in the filter field and click **Subscribe**. Only messages containing that substring (case-insensitive) will appear in the right panel.
4. You can change the filter at any time by entering a new value and clicking **Subscribe** again.

### Running tests

```powershell
cd VueSignalRClient
npm test
```

---

## Project Structure

```
eda-test-with-rabbitmq-and-signalr/
├── EventDrivenArchitectureTest.sln
├── RabbitMQProducer/           # Publishes messages to RabbitMQ
├── RabbitMQConsumer/           # Consumes from RabbitMQ, forwards to SignalRHub
├── SignalRHub/                 # ASP.NET Core Web API + SignalR server
│   ├── Controllers/
│   │   └── MessageController.cs
│   └── Services/
│       └── FilterService.cs
├── SignalRClient/              # Console SignalR client
├── SignalRHub.Tests/           # xUnit tests for SignalRHub
└── VueSignalRClient/           # Vue 3 browser SignalR client
```

---

## License

This project is provided for experimental and educational purposes.


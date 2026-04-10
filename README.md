# EDA Test with RabbitMQ and SignalR

A proof-of-concept project demonstrating **Event-Driven Architecture (EDA)** using **RabbitMQ** as a message broker and **ASP.NET Core SignalR** for real-time push notifications to connected clients.

All projects target **.NET 6**.

---

## Architecture Overview

```
┌──────────────────┐    RabbitMQ Queue    ┌──────────────────┐    HTTP POST    ┌──────────────────┐   SignalR (WS)   ┌──────────────────┐
│  RabbitMQProducer│ ──────────────────▶  │ RabbitMQConsumer │ ──────────────▶ │   SignalRHub     │ ───────────────▶ │  SignalRClient   │
│  (console app)   │                      │  (background svc)│                 │ (ASP.NET Web API)│                  │  (console app)   │
└──────────────────┘                      └──────────────────┘                 └──────────────────┘                  └──────────────────┘
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

**NuGet dependencies:** `RabbitMQ.Client 6.5.0`

### RabbitMQConsumer
A console application hosting a `BackgroundService` that consumes messages from RabbitMQ.
- Subscribes to the same `some-queue` queue.
- On each received message, sends an HTTP POST request to `https://localhost:7180/message` (SignalRHub).

**NuGet dependencies:** `RabbitMQ.Client 6.5.0`, `Microsoft.Extensions.Hosting 7.0.1`

### SignalRHub
An ASP.NET Core Web API + SignalR server — the central message hub.
- Listens on `https://localhost:7180`.
- **REST endpoint** `POST /message`: accepts a JSON string and broadcasts it to all connected SignalR clients via the `ReceiveMessage` event.
- **SignalR Hub** at `/hub` with method `SendMessage(user, message)`.
- Swagger UI available at `/swagger` (in Development mode).

**NuGet dependencies:** `Swashbuckle.AspNetCore 6.5.0`

### SignalRClient
A console application that connects to the SignalR Hub and prints incoming messages.
- Connects to `https://localhost:7180/hub`.
- Listens for the `ReceiveMessage` event and prints `{user}: {message}` to the console.

**NuGet dependencies:** `Microsoft.AspNetCore.SignalR.Client 6.0.16`

---

## Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
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

#### Option B: Swagger UI

1. Open your browser and navigate to:
   ```
   https://localhost:7180/swagger
   ```
2. If the browser shows a certificate warning, click **Advanced → Proceed to localhost**.
3. Expand the **POST /message** endpoint.
4. Click **Try it out**.
5. In the **Request body** field enter a JSON string (quotes are required):
   ```json
   "Hello from Swagger!"
   ```
6. Click **Execute**.
7. Verify the response code is **200**.

### Step 4 — Check the result

Switch to the **SignalRClient** terminal. You should see:

```
Server: Hello from curl!
```
or
```
Server: Hello from Swagger!
```

You can send as many requests as you like — every message will appear in the client console instantly, confirming that the **HTTP → SignalR Hub → WebSocket → Client** pipeline works correctly.

---

## Project Structure

```
eda-test-with-rabbitmq-and-signalr/
├── EventDrivenArchitectureTest.sln
├── RabbitMQProducer/           # Publishes messages to RabbitMQ
├── RabbitMQConsumer/           # Consumes from RabbitMQ, forwards to SignalRHub
├── SignalRHub/                 # ASP.NET Core Web API + SignalR server
│   └── Controllers/
│       └── MessageController.cs
└── SignalRClient/              # Console SignalR client
```

---

## License

This project is provided for experimental and educational purposes.


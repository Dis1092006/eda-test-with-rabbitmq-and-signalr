using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("SignalR client starting ...");

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7180/hub")
    .Build();
await connection.StartAsync();

connection.On<string, string>("ReceiveMessage", (user, message) =>
{
    Console.WriteLine($"{user}: {message}");
});

Console.Read();
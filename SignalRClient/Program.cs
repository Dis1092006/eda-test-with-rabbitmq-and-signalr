using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("SignalR client starting ...");

const string hubUrl = "https://localhost:7180/hub";

var connection = new HubConnectionBuilder()
    .WithUrl(hubUrl)
    .Build();
await connection.StartAsync();

connection.On<string, string>("ReceiveMessage", (user, message) =>
{
    Console.WriteLine($"{user}: {message}");
});

Console.Read();
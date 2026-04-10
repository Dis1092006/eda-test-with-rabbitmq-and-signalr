using System.Text;
using RabbitMQ.Client;

Console.WriteLine("Producer starting ...");

const string exchangeName = "amq.direct";
const string queueName = "some-queue";
const string routingKey = "some-routing-key";

var factory = new ConnectionFactory { HostName = "localhost" };
await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
await channel.QueueBindAsync(queueName, exchangeName, routingKey);

while (true)
{
    Console.WriteLine("Enter message payload (print \"exit\" to finish):");
    var payload = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(payload))
        continue;

    if (payload == "exit")
        break;

    var body = Encoding.UTF8.GetBytes(payload);
    await channel.BasicPublishAsync(exchangeName, routingKey, body);

    Console.WriteLine("Message sent");
}

Console.WriteLine("Finish");
using System.Text;
using RabbitMQ.Client;

Console.WriteLine("Start");

const string exchangeName = "amq.direct";
const string queueName = "some-queue";
const string routingKey = "some-routing-key";

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queueName, true, false, false);
channel.QueueBind(queueName, exchangeName, routingKey);

while (true)
{
    Console.WriteLine("Enter message payload (print \"exit\" to finish):");
    var payload = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(payload))
        continue;
    
    if (payload == "exit")
        break;

    var body = Encoding.UTF8.GetBytes(payload);

    channel.BasicPublish(exchangeName, routingKey, null, body);

    Console.WriteLine("Message sent");
}

Console.WriteLine("Finish");

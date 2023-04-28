using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQConsumer;

public class ConsumerService : BackgroundService
{
    private readonly IModel _channel;

    private const string ExchangeName = "amq.direct";
    private const string QueueName = "some-queue";
    private const string RoutingKey = "some-routing-key";

    private const string MessageProcessorEndpoint = "https://localhost:7180/message";

    public ConsumerService()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
        _channel.QueueDeclare(QueueName, true, false, false);
        _channel.QueueBind(QueueName, ExchangeName, RoutingKey);
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"Message received: {message}");
            
            var client = new HttpClient();
            _ = client.PostAsJsonAsync<string>(MessageProcessorEndpoint, message).Result;
        };
        
        _channel.BasicConsume(QueueName, true, consumer);
        
        return Task.CompletedTask;
    }
}
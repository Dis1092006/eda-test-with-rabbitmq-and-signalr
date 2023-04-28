using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQConsumer;

Console.WriteLine("Consumer starting ...");

var host = new HostBuilder()
    .ConfigureServices(services =>
        services.AddHostedService<ConsumerService>())
    .UseConsoleLifetime()
    .Build();

host.Run();
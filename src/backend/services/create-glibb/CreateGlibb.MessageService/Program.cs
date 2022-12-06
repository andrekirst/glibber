using Library.Redis;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CreateGlibb.MessageService;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var hostBuilder = Host.CreateDefaultBuilder(args);

        hostBuilder.ConfigureServices(services =>
        {
            services.AddCacheClient();

            services.AddHostedService<SubscribeHostedService>();

            services.AddMassTransit(configurator =>
            {
                configurator.UsingRabbitMq((context, factoryConfigurator) =>
                {
                    factoryConfigurator.Host("localhost", "/", hostConfigurator =>
                    {
                        hostConfigurator.Username("guest");
                        hostConfigurator.Password("guest");
                    });

                    factoryConfigurator.ConfigureEndpoints(context);
                });
            });
        });

        var host = hostBuilder.Build();

        Console.CancelKeyPress += (_, _) => cancellationTokenSource.Cancel();
        await host.RunAsync(cancellationToken);
    }
}
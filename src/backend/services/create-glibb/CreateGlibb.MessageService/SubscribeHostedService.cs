using System.Text.Json;
using Ardalis.GuardClauses;
using Contracts;
using Library.Redis;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CreateGlibb.MessageService;

public class SubscribeHostedService : IHostedService, IDisposable, IAsyncDisposable
{
    private readonly ICacheClient _cacheClient;
    private readonly ILogger<SubscribeHostedService> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    private Timer? _timer;
    private const long BatchCount = 1000;
    private readonly TimeSpan _dueTime = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _period = TimeSpan.FromMinutes(1);

    public SubscribeHostedService(
        ICacheClient cacheClient,
        ILogger<SubscribeHostedService> logger,
        IPublishEndpoint publishEndpoint)
    {
        _cacheClient = cacheClient;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(TimerCallback, null, _dueTime, _period);
        
        var subscriber = _cacheClient.Connection.GetSubscriber();

        async void Handler(RedisChannel _, RedisValue value)
        {
            Guard.Against.Null(value, nameof(value));

            var @event = JsonSerializer.Deserialize(value!, GlibbCreationStartedJsonContext.Default.GlibbCreationStarted);

            Guard.Against.Null(@event, nameof(@event));

            await _publishEndpoint.Publish(@event, cancellationToken);
        }

        await subscriber.SubscribeAsync(EnvironmentValues.RedisSubscribtionName, Handler);

        _logger.LogInformation($"{nameof(SubscribeHostedService)} started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(SubscribeHostedService)} stopped");
        await _cacheClient.Connection.GetSubscriber().UnsubscribeAsync(EnvironmentValues.RedisSubscribtionName);
    }

    private async void TimerCallback(object? state)
    {
        var values = await _cacheClient.Database.ListRightPopAsync(EnvironmentValues.RedisListName, BatchCount);
        var events = values
            .Select(value => JsonSerializer.Deserialize(value!, GlibbCreationStartedJsonContext.Default.GlibbCreationStarted))
            .Where(m => m != null)
            .Select(m => m!)
            .ToList();

        await _publishEndpoint.PublishBatch(events);
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        _timer?.DisposeAsync();
        return ValueTask.CompletedTask;
    }
}

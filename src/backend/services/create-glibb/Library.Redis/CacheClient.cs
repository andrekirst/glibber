using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Library.Redis;

public class CacheClient : ICacheClient
{
    private readonly ILogger<CacheClient> _logger;
    private readonly Lazy<IConnectionMultiplexer>? _connection;

    public CacheClient(
        IConfiguration configuration,
        ILogger<CacheClient> logger)
    {
        _logger = logger;

        var host = configuration["Redis:Host"] ?? "localhost";
        var port = configuration.GetValue<int?>("Redis:Port") ?? 6379;
        var allowAdmin = configuration.GetValue<bool?>("Redis:AllowAdmin") ?? false;
        var clientName = configuration["Redis:ClientName"] ?? Guid.NewGuid().ToString();
        _logger.LogInformation("Use ClientName \"{clientName}\"", clientName);

        var configurationOptions = new ConfigurationOptions
        {
            EndPoints = { { host, port } },
            AllowAdmin = allowAdmin,
            ClientName = clientName,
            ReconnectRetryPolicy = new LinearRetry(5000),
            AbortOnConnectFail = false
        };
        _connection = new Lazy<IConnectionMultiplexer>(() =>
        {
            var redis = ConnectionMultiplexer.Connect(configurationOptions);
            redis.ErrorMessage += Redis_ErrorMessage;
            redis.InternalError += Redis_InternalError;
            redis.ConnectionFailed += Redis_ConnectionFailed;
            return redis;
        });
    }

    public IConnectionMultiplexer Connection => _connection?.Value ?? throw new ArgumentNullException();

    public IDatabase Database => Connection.GetDatabase();

    public async Task<T?> JsonGet<T>(RedisKey key, CommandFlags flags = CommandFlags.None)
    {
        var rv = await Database.StringGetAsync(key, flags);
        return rv.HasValue ? JsonSerializer.Deserialize<T>(rv!) : default;
    }

    public Task<bool> JsonSet(RedisKey key, object value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        return Database.StringSetAsync(key, JsonSerializer.Serialize(value), expiry, when, flags);
    }

    private void Redis_ConnectionFailed(object? sender, ConnectionFailedEventArgs e)
    {
        if (e.Exception != null)
        {
            _logger.LogError(e.Exception, e.Exception.Message);
        }
    }

    private void Redis_InternalError(object? sender, InternalErrorEventArgs e)
    {
        _logger.LogError(e.Exception, e.Exception.Message, e.Origin);
    }

    private void Redis_ErrorMessage(object? sender, RedisErrorEventArgs e)
    {
        _logger.LogError(e.Message);
    }
}
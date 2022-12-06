using StackExchange.Redis;

namespace Library.Redis;

public interface ICacheClient
{
    IConnectionMultiplexer Connection { get; }
    IDatabase Database { get; }
    Task<T?> JsonGet<T>(RedisKey key, CommandFlags flags = CommandFlags.None);
    Task<bool> JsonSet(RedisKey key, object value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None);
}
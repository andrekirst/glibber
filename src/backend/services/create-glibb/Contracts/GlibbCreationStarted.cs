using System.Text.Json.Serialization;

namespace Contracts;

public class GlibbCreationStarted
{
    public Guid GlibbId { get; set; }
    public string MessageText { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[JsonSerializable(typeof(GlibbCreationStarted))]
public partial class GlibbCreationStartedJsonContext : JsonSerializerContext
{
}

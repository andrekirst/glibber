using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Contracts;

public class Message
{
    [MaxLength(Lengths.Glibb.Message.MaximumLength)]
    [Required]
    public string Text { get; set; } = default!;
}

[JsonSerializable(typeof(Message))]
public partial class MessageJsonContext : JsonSerializerContext
{
}
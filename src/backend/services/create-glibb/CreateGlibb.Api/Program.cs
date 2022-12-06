using System.Text.Json;
using Contracts;
using Library.Redis;
using Microsoft.AspNetCore.Mvc;

namespace CreateGlibb.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddProblemDetails();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddCacheClient();
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.JsonSerializerOptions.AddContext<MessageJsonContext>();
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // TODO
        //app.UseAuthorization();

        app.MapPost("/glibb", async (
                [FromBody] Message message,
                [FromServices] ICacheClient cacheClient) =>
             {
                 var glibbId = Guid.NewGuid();
                 var @event = new GlibbCreationStarted
                 {
                     GlibbId = glibbId,
                     MessageText = message.Text
                 };

                 var json = JsonSerializer.Serialize(@event, GlibbCreationStartedJsonContext.Default.GlibbCreationStarted);
                 var subscriberCount = await cacheClient.Database.PublishAsync(EnvironmentValues.RedisSubscribtionName, json);
                 if (subscriberCount == 0)
                 {
                     await cacheClient.Database.ListLeftPushAsync(EnvironmentValues.RedisListName, json);
                 }

                 return TypedResults.Created("glibb", glibbId);
             })
            .WithOpenApi()
            .WithDescription("Initiating the creation of an Glibb with an message")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem();

        await app.RunAsync();
    }
}

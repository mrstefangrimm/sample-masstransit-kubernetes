using MassTransit;
using Microsoft.AspNetCore.Builder;
using Sample.Masstransit.WebApi.Core;
using Sample.Masstransit.WebApi.Core.Events;
using Sample.Masstransit.WebApi.Core.Extensions;
using Sample.Masstransit.Worker.Workers;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddSerilog("Worker MassTransit");
    Log.Information("Starting Worker");
    Log.Information("DOTNET_ENVIRONMENT: " + Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));

    builder.Configuration
           .AddJsonFile($"/app/appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
           .AddJsonFile("/config/appsettings.json", optional: true, reloadOnChange: true)
           .Build();

    foreach (var kvp in builder.Configuration.AsEnumerable())
    {
        Log.Debug($"{kvp.Key} = {kvp.Value}");
    }

    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog(Log.Logger)
        .ConfigureServices((context, collection) =>
        {
            AppSettings appSettings = new AppSettings();
            context.Configuration.Bind(appSettings);

            collection.AddOpenTelemetry(appSettings);
            collection.AddHttpContextAccessor();

            collection.AddMassTransit(x =>
            {
                x.AddDelayedMessageScheduler();
                x.AddConsumer<TimerVideoConsumer>(typeof(TimerVideoConsumerDefinition));
                x.AddConsumer<QueueClientInsertedConsumer>(typeof(QueueClientConsumerDefinition));
                x.AddConsumer<QueueClientUpdatedConsumer>(typeof(QueueClientUpdatedConsumerDefinition));
                x.AddConsumer<QueueSendEmailConsumer>(typeof(QueueSendEmailConsumerDefinition));
                x.AddRequestClient<ConvertVideoEvent>();

                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(builder.Configuration.GetConnectionString("RabbitMq"));
                    cfg.UseDelayedMessageScheduler();
                    //cfg.ConnectReceiveObserver(new ReceiveObserverExtensions());
                    cfg.ServiceInstance(instance =>
                    {
                        instance.ConfigureJobServiceEndpoints();
                        instance.ConfigureEndpoints(ctx, new KebabCaseEndpointNameFormatter("dev", false));
                    });
                });
            });
        }).Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}
using Sample.Masstransit.WebApi.Core;
using Sample.Masstransit.WebApi.Core.Extensions;
using Serilog;

try
{
    Console.WriteLine("ASPNETCORE_ENVIRONMENT: " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

    var builder = WebApplication.CreateBuilder(args);
    builder.AddSerilog("API MassTransit");
    Log.Information("Starting API");

    IConfiguration? configuration = null;

    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "kubernetes")
    {
        configuration = new ConfigurationBuilder()
          .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
          .Build();
    }
    else
    {    
      configuration = new ConfigurationBuilder()
        .SetBasePath(Environment.CurrentDirectory)
        .AddJsonFile("/app/config/appsettings.json", optional: true, reloadOnChange: true)
        .Build();
    }

    var appSettings = new AppSettings();
    builder.Configuration.Bind(appSettings);
    builder.Configuration.Bind(configuration);

    builder.Services.AddRouting(options => options.LowercaseUrls = true);

    builder.Services.AddControllers();
    builder.Services.AddOpenTelemetry(appSettings);

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", null);
    });

    builder.Services.AddMassTransitExtension(builder.Configuration);

    var app = builder.Build();

    //if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample.Masstransit.WebApi v1"));
    }

    app.MapControllers();

    await app.RunAsync();
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

//static IHostBuilder CreateHostBuilder(string[] args) =>
//    Host.CreateDefaultBuilder(args)
//        .ConfigureAppConfiguration((context, config) =>
//        {
//            config.AddJsonFile("/app/config/appsettings.json", optional: true, reloadOnChange: false);
//        })
//        .ConfigureWebHostDefaults(webBuilder =>
//        {
//            webBuilder.UseStartup<Startup>();
//        });
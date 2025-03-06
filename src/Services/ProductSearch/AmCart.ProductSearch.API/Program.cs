using AmCart.ProductSearch.API.Configuration;
using AmCart.ProductSearch.API.Repositories;
using AmCart.ProductSearch.API.Repositories.Interfaces;
using AmCart.ProductSearch.API.Services;
using AmCart.ProductSearch.API.Services.Interfaces;
using AmCart.ProductSearch.API.AutoMapper;
using AmCart.ProductSearch.API.Shared.Enums;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Serilog;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using System;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Load configuration from appsettings.json and environment variables
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // ✅ Load environment variables

// 🔹 Configure Logging (Serilog)
ConfigureLogging();

// 🔹 Register AutoMapper for Product Mapping
builder.Services.AddAutoMapper(typeof(ProductMappingProfile));

// 🔹 Add services to the container (Controllers)
builder.Services.AddControllers();

// 🔹 Load configuration from appsettings.json
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.Configure<ElasticSearchSettings>(builder.Configuration.GetSection("ElasticsearchSettings"));

// ✅ Register SearchSettings from appsettings.json
builder.Services.Configure<SearchSettings>(builder.Configuration.GetSection("SearchSettings"));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptionsMonitor<SearchSettings>>().CurrentValue
);

// 🔹 Register MongoDB Client
//builder.Services.AddSingleton<IMongoClient>(sp =>
//{
//    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
//    return new MongoClient(settings.ConnectionString);
//});

// 🔹 Register MongoDB Client as a Singleton
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var connectionString = Environment.GetEnvironmentVariable("COSMOSDB_CONNECTION_STRING")
                           ?? settings.ConnectionString; // ✅ Use env variable if available

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("❌ MongoDB/CosmosDB connection string is missing.");
    }

    return new MongoClient(connectionString);
});

// 🔹 Register Elasticsearch Client
builder.Services.AddSingleton<ElasticsearchClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<ElasticSearchSettings>>().Value;
    var connectionSettings = new ElasticsearchClientSettings(new Uri(settings.Uri))
        .DefaultIndex(settings.DefaultIndex) // Set default index
        .Authentication(new BasicAuthentication(settings.Username, settings.Password))  // Use basic authentication with username and password
        .EnableDebugMode()
        .DisablePing()
        .ServerCertificateValidationCallback((o, certificate, chain, errors) => true) // Ignore SSL certificate validation (use cautiously in production)
        .RequestTimeout(TimeSpan.FromMinutes(2))
        .ThrowExceptions();// Ensure exceptions are thrown on errors, preventing silent failures

    return new ElasticsearchClient(connectionSettings);
});

//// 🔹 Register Product Search Repository dynamically
//builder.Services.AddScoped<IProductSearchRepository>(sp =>
//{
//    var settings = sp.GetRequiredService<IOptions<SearchSettings>>().Value;
//    var mongoClient = sp.GetRequiredService<IMongoClient>();
//    var elasticClient = sp.GetRequiredService<ElasticsearchClient>();

//    return settings.ProviderType switch
//    {
//        SearchProviderType.Elasticsearch => new ProductSearchRepository(elasticClient),
//        SearchProviderType.CosmosDb or SearchProviderType.MongoDb => new ProductSearchRepository(mongoClient, settings.ProviderType),
//        _ => throw new InvalidOperationException($"Invalid search provider: {settings.ProviderType}")
//    };
//});

// 🔹 Register Product Search Repository dynamically
builder.Services.AddScoped<IProductSearchRepository>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<SearchSettings>>().Value;
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var mapper = sp.GetRequiredService<IMapper>(); // ✅ Get IMapper instance

    return settings.ProviderType switch
    {
        SearchProviderType.Elasticsearch =>
            new ElasticProductSearchRepository(
                sp.GetRequiredService<ElasticsearchClient>(),
                loggerFactory.CreateLogger<ElasticProductSearchRepository>()
            ),

        SearchProviderType.CosmosDb or SearchProviderType.MongoDb =>
            new CosmosDbProductSearchRepository(
                sp.GetRequiredService<IMongoClient>(),  // ✅ Inject MongoClient
                sp.GetRequiredService<IOptions<MongoDbSettings>>(),  // ✅ Inject MongoDbSettings
                loggerFactory.CreateLogger<CosmosDbProductSearchRepository>() , // ✅ Inject Logger
                  mapper  // ✅ Inject AutoMapper
            ),

        _ => throw new InvalidOperationException($"Invalid search provider: {settings.ProviderType}")
    };
});



// 🔹 Register Services
builder.Services.AddScoped<IProductDataSyncService, ProductDataSyncService>();

// 🔹 Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Product Search API",
        Version = "v1",
        Description = "API for searching products in AmCart",
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// 🔹 Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// 🔹 Ensure Database Connections
var logger = app.Services.GetRequiredService<ILogger<Program>>();
await EnsureDatabaseConnectionsAsync(app.Services, logger);

// 🔹 Perform Product Data Synchronization only for Elasticsearch
using (var scope = app.Services.CreateScope())
{
    var settings = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<SearchSettings>>().CurrentValue;
    if (settings.ProviderType == SearchProviderType.Elasticsearch)
    {
        var syncService = scope.ServiceProvider.GetRequiredService<IProductDataSyncService>();
        await syncService.SyncProductDataAsync();
    }
}

// 🔹 Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Search API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// 🔹 Health Check Endpoint
app.MapGet("/", () => Results.Ok("Product Search API is running 🚀"));

app.Run();

/// <summary>
/// Configures Serilog for logging.
/// </summary>
void ConfigureLogging()
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.Debug()
        .WriteTo.File(
            path: @"AmCart.ProductSearch.API-.log",
            rollingInterval: RollingInterval.Day,
            fileSizeLimitBytes: 10_000_000,
            rollOnFileSizeLimit: true,
            retainedFileCountLimit: 7,
            encoding: System.Text.Encoding.UTF8
        )
        .MinimumLevel.Debug()
        .CreateLogger();

    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();
}

/// <summary>
/// Ensures that MongoDB or Elasticsearch connections are established at application startup based on the configured SearchProviderType.
/// </summary>
/// <param name="services">The service provider for dependency injection.</param>
/// <param name="logger">The logger instance for logging connection status.</param>
async Task EnsureDatabaseConnectionsAsync(IServiceProvider services, Microsoft.Extensions.Logging.ILogger logger)
{
    using var scope = services.CreateScope();
    var provider = scope.ServiceProvider;
    var searchSettings = provider.GetRequiredService<IOptionsMonitor<SearchSettings>>().CurrentValue;

    try
    {
        if (searchSettings.ProviderType == SearchProviderType.MongoDb || searchSettings.ProviderType == SearchProviderType.CosmosDb)
        {
            // ✅ Ensure MongoDB/CosmosDB connection
            var mongoClient = provider.GetRequiredService<IMongoClient>();
            await mongoClient.ListDatabaseNamesAsync();
            logger.LogInformation("✅ {ProviderType} connection established successfully.", searchSettings.ProviderType);
        }
        else if (searchSettings.ProviderType == SearchProviderType.Elasticsearch)
        {
            // ✅ Ensure Elasticsearch connection with retry logic
            var elasticClient = provider.GetRequiredService<ElasticsearchClient>();

            for (int attempt = 1; attempt <= 5; attempt++)
            {
                try
                {
                    var pingResponse = await elasticClient.PingAsync();
                    if (pingResponse.IsSuccess())
                    {
                        logger.LogInformation("✅ Elasticsearch connected on attempt {Attempt}.", attempt);

                        // ✅ Ensure index and mappings for ProductSearch
                        await ElasticsearchMappings.CreateProductSearchIndexAsync(elasticClient);
                        return;
                    }

                    logger.LogWarning("⚠️ Elasticsearch connection failed (Attempt {Attempt}/5): {ErrorMessage}",
                        attempt, pingResponse.ElasticsearchServerError?.Error?.Reason ?? "Unknown error");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,"⚠️ Elasticsearch attempt {Attempt}/5 failed: {ErrorMessage}", attempt, ex.Message);
                }

                await Task.Delay(2000);
            }

            logger.LogError("❌ Failed to connect to Elasticsearch after multiple attempts.");
        }
        else
        {
            logger.LogError("❌ Invalid search provider: {ProviderType}.", searchSettings.ProviderType);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error during service initialization.");
    }
}


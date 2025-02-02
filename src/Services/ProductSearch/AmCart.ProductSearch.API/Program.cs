using AmCart.ProductSearch.API.Configuration;
using AmCart.ProductSearch.API.Repositories;
using AmCart.ProductSearch.API.Repositories.Interfaces;
using AmCart.ProductSearch.API.Services.Interfaces;
using AmCart.ProductSearch.API.Services;
using Microsoft.Extensions.Options;
using Serilog;
using MongoDB.Driver;
using AmCart.ProductSearch.API.AutoMapper;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using System;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Configure Logging (Serilog)
ConfigureLogging();

// 🔹 Register AutoMapper for Product Mapping
builder.Services.AddAutoMapper(typeof(ProductMappingProfile));

// 🔹 Add services to the container (Controllers)
builder.Services.AddControllers();

// 🔹 Load configuration from appsettings.json
// Register MongoDB and Elasticsearch settings from configuration
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.Configure<ElasticSearchSettings>(builder.Configuration.GetSection("ElasticsearchSettings"));

// 🔹 Register MongoDB Client as a Singleton
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);  // Returns an instance of MongoClient using the connection string from settings
});

// 🔹 Register Elasticsearch Client as a Singleton
builder.Services.AddSingleton<ElasticsearchClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<ElasticSearchSettings>>().Value;

    // Set up connection settings for Elasticsearch, including authentication and timeout
    var connectionSettings = new ElasticsearchClientSettings(new Uri(settings.Uri))
        .DefaultIndex(settings.DefaultIndex)  // Set default index
        .Authentication(new BasicAuthentication(settings.Username, settings.Password))  // Use basic authentication with username and password
        .EnableDebugMode()  // Optional: Enable verbose logging for debugging
        .DisablePing()  // Optional: Disable ping (disable version check)
        .ServerCertificateValidationCallback((o, certificate, chain, errors) => true)  // Ignore SSL certificate validation (use cautiously in production)
        .RequestTimeout(TimeSpan.FromMinutes(2))  // Set request timeout to 2 minutes
        .ThrowExceptions();  // Ensure exceptions are thrown on errors, preventing silent failures

    return new ElasticsearchClient(connectionSettings);
});

// 🔹 Register Repositories and Services
builder.Services.AddScoped<IProductSearchRepository, ProductSearchRepository>();
builder.Services.AddScoped<IProductDataSyncService, ProductDataSyncService>();

// 🔹 Add Swagger to API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔹 Configure CORS policy (Allow all origins, methods, and headers)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());  // Allow all origins and HTTP methods for cross-origin requests
});

var app = builder.Build();

// 🔹 Ensure Database Connections (MongoDB and Elasticsearch) are Established at Startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
await EnsureDatabaseConnectionsAsync(app.Services, logger);

// 🔹 Perform Product Data Synchronization during Application Startup
using (var scope = app.Services.CreateScope())
{
    var syncService = scope.ServiceProvider.GetRequiredService<IProductDataSyncService>();
    await syncService.SyncProductDataAsync();  // Sync product data on application startup
}

// 🔹 Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();  // Enable Swagger for API documentation in development environment
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Search API v1");
        options.RoutePrefix = string.Empty;  // Makes Swagger UI available at root URL
    });
}

// 🔹 Apply CORS Middleware (Use defined policy)
app.UseCors("AllowAll");

app.UseHttpsRedirection();  // Redirect HTTP to HTTPS
app.UseAuthorization();  // Ensure authorization middleware is applied
app.MapControllers();  // Map controllers to the request pipeline

// 🔹 Health Check Endpoint (Basic route to check if API is live)
app.MapGet("/", () => Results.Ok("Product Search API is running 🚀"));

app.Run();  // Run the application

/// <summary>
/// Configures Serilog for logging.
/// </summary>
void ConfigureLogging()
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()  // Log to console
        .WriteTo.Debug()    // Log to debug output
        .WriteTo.File(
            path: @"AmCart.ProductSearch.API-.log",  // Log file path
            rollingInterval: RollingInterval.Day,  // Create a new log file every day
            fileSizeLimitBytes: 10_000_000,  // Max log file size (10 MB)
            rollOnFileSizeLimit: true,  // Roll over the log file if size is exceeded
            retainedFileCountLimit: 7,  // Retain 7 days' worth of logs
            encoding: System.Text.Encoding.UTF8  // Use UTF-8 encoding for log files
        )
        .MinimumLevel.Debug()  // Minimum log level (Debug and above)
        .CreateLogger();  // Create the logger instance

    builder.Logging.ClearProviders();  // Clear default logging providers to use Serilog
    builder.Logging.AddSerilog();  // Add Serilog to ASP.NET Core logging
}

/// <summary>
/// Ensures that MongoDB and Elasticsearch connections are established at application startup.
/// </summary>
/// <param name="services">The service provider for dependency injection.</param>
/// <param name="logger">The logger instance for logging connection status.</param>
async Task EnsureDatabaseConnectionsAsync(IServiceProvider services, Microsoft.Extensions.Logging.ILogger logger)
{
    using var scope = services.CreateScope();
    var provider = scope.ServiceProvider;

    try
    {
        // ✅ Ensure MongoDB connection
        var mongoClient = provider.GetRequiredService<IMongoClient>();
        await mongoClient.ListDatabaseNamesAsync();  // Ensure MongoDB connection is successful by listing database names asynchronously
        logger.LogInformation("✅ MongoDB connection established successfully.");

        // ✅ Ensure Elasticsearch connection with retry logic (up to 5 attempts)
        var elasticClient = provider.GetRequiredService<ElasticsearchClient>();

        for (int attempt = 1; attempt <= 5; attempt++)
        {
            try
            {
                var pingResponse = await elasticClient.PingAsync();  // Ping Elasticsearch to check connection
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
                logger.LogWarning("⚠️ Elasticsearch attempt {Attempt}/5 failed: {ErrorMessage}", attempt, ex.Message);
            }

            await Task.Delay(2000);  // Retry after 2 seconds
        }

        logger.LogError("❌ Failed to connect to Elasticsearch after multiple attempts.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error during service initialization.");
    }
}





//void EnsureDatabaseConnections(IServiceProvider services, Microsoft.Extensions.Logging.ILogger logger)
//{
//    using var scope = services.CreateScope();
//    var provider = scope.ServiceProvider;

//    try
//    {

//        //// 🔹 MongoDB Connection Check
//        var mongoClient = provider.GetRequiredService<IMongoClient>();
//        mongoClient.ListDatabaseNames();
//        logger.LogInformation("✅ MongoDB connection established successfully.");


//        // 🔹 Elasticsearch Connection Check
//        var elasticClient = provider.GetRequiredService<IElasticClient>();
//        for (int attempt = 1; attempt <= 5; attempt++)
//        {
//            var pingResponse = elasticClient.Ping();
//            if (pingResponse.IsValid)
//            {
//                logger.LogInformation("✅ Elasticsearch connected on attempt {Attempt}.", attempt);
//                return;
//            }

//            logger.LogWarning("⚠️ Elasticsearch connection failed (Attempt {Attempt}/5): {ErrorMessage}",
//                              attempt, pingResponse.OriginalException?.Message ?? "Unknown error");

//            Thread.Sleep(2000); // Retry delay
//        }

//        logger.LogError("❌ Failed to connect to Elasticsearch after multiple attempts.");
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, "❌ Error during service initialization.");
//    }
//}

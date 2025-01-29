//using AmCart.ProductSearch.API.Configuration;
//using AmCart.ProductSearch.API.Repositories.Interfaces;
//using AmCart.ProductSearch.API.Repositories;
//using Microsoft.AspNetCore.HttpLogging;
//using Microsoft.Extensions.Options;
//using Nest;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.OpenApi.Models;

////var builder = WebApplication.CreateBuilder(args);

////// Add services to the container
////builder.Services.AddControllers();
////builder.Services.AddEndpointsApiExplorer();
////builder.Services.AddSwaggerGen(options =>
////{
////    options.SwaggerDoc("v1", new OpenApiInfo
////    {
////        Title = "Product Search API",
////        Version = "v1",
////        Description = "API for searching products using Elasticsearch"
////    });
////});

////var app = builder.Build();

////// Enable Swagger only in Development or based on a setting
////if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwaggerInProduction"))
////{
////    app.UseSwagger();
////    app.UseSwaggerUI(options =>
////    {
////        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Search API v1");
////        options.RoutePrefix = string.Empty; // Swagger is available at the root URL
////    });
////}

////// Enable essential middlewares
////app.UseHttpsRedirection();
////app.UseAuthorization();
////app.MapControllers();
////app.Run();

//var builder = WebApplication.CreateBuilder(args);

//// Load configuration
//builder.Services.Configure<ElasticSearchSettings>(builder.Configuration.GetSection("Elasticsearch"));

//// Add services to the container
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();

//// Configure Swagger with custom API information
//builder.Services.AddSwaggerGen(options =>
//{
//    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
//    {
//        Title = "Product Search API",
//        Version = "v1",
//        Description = "API for searching products using Elasticsearch",
//        Contact = new Microsoft.OpenApi.Models.OpenApiContact
//        {
//            Name = "Support",
//            Email = "support@example.com",
//            Url = new Uri("https://example.com")
//        }
//    });

//    // Include XML comments (requires XML documentation enabled in project settings)
//    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
//    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
//    if (File.Exists(xmlPath))
//    {
//        options.IncludeXmlComments(xmlPath);
//    }
//});

//// Register the ProductSearchRepository
//builder.Services.AddScoped<IProductSearchRepository, ProductSearchRepository>();

//// Register IElasticClient as Singleton
//builder.Services.AddSingleton<IElasticClient>(sp =>
//{
//    var elasticSettings = sp.GetRequiredService<IOptions<ElasticSearchSettings>>().Value;

//    var connectionSettings = new ConnectionSettings(new Uri(elasticSettings.Uri))
//                             .DefaultIndex(elasticSettings.DefaultIndex);

//    // Initialize IElasticClient
//    var elasticClient = new ElasticClient(connectionSettings);

//    // Optionally: Test the connection on startup
//    if (!elasticClient.Ping().IsValid)
//    {
//        throw new InvalidOperationException("Unable to connect to Elasticsearch.");
//    }

//    return elasticClient;
//});

//// Add logging
//builder.Services.AddHttpLogging(logging =>
//{
//    logging.LoggingFields = HttpLoggingFields.All;
//});

//// Configure CORS (Optional)
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//        policy.AllowAnyOrigin()
//              .AllowAnyMethod()
//              .AllowAnyHeader());
//});

//var app = builder.Build();

//// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwaggerInProduction"))
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(options =>
//    {
//        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Search API v1");
//        options.RoutePrefix = string.Empty; // Makes Swagger available at root URL
//    });
//}

//// Enable HTTP Logging
//app.UseHttpLogging();

//// Enable CORS
//app.UseCors("AllowAll");

//app.UseHttpsRedirection();
//app.UseAuthorization();
//app.MapControllers();

//app.Run();


//public void EnsureElasticsearchConnection(IElasticClient elasticClient, ILogger logger, int maxRetries = 5, int delayMilliseconds = 2000)
//{
//    for (int attempt = 1; attempt <= maxRetries; attempt++)
//    {
//        var pingResponse = elasticClient.Ping();

//        if (pingResponse.IsValid)
//        {
//            logger.LogInformation("Successfully connected to Elasticsearch on attempt {Attempt}.", attempt);
//            return; // Exit method if connection is successful
//        }

//        logger.LogWarning("Elasticsearch connection failed on attempt {Attempt}/{MaxRetries}. Error: {ErrorMessage}",
//                          attempt, maxRetries, pingResponse.OriginalException?.Message ?? "Unknown error");

//        if (attempt < maxRetries)
//        {
//            Thread.Sleep(delayMilliseconds); // Wait before retrying
//        }
//    }

//    logger.LogError("Unable to connect to Elasticsearch after {MaxRetries} attempts.", maxRetries);
//}

using AmCart.ProductSearch.API.Configuration;
using AmCart.ProductSearch.API.Repositories;
using AmCart.ProductSearch.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Serilog;
using System;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Add services to the container
builder.Services.AddControllers();

// 🔹 Load configuration from appsettings.json
builder.Services.Configure<ElasticSearchSettings>(builder.Configuration.GetSection("ElasticsearchSettings"));

// 🔹 Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()  // Logs to console
    .WriteTo.Debug()    // Logs to debug output
                        // 🔹 Rolling file configuration
    .WriteTo.File(
        path: @"C:\Logs\AmCart.ProductSearch.API-.log",              // Path for log files
        rollingInterval: RollingInterval.Day,  // Create a new log file every day
        fileSizeLimitBytes: 10_000_000,       // Max size of log files (10 MB)
        rollOnFileSizeLimit: true,            // Start a new file when size is exceeded
        retainedFileCountLimit: 7,            // Keep 7 days worth of logs
        encoding: System.Text.Encoding.UTF8  // Use UTF-8 encoding
    )
    .MinimumLevel.Debug()
    .CreateLogger();

// Integrate Serilog into ASP.NET Core's logging system
builder.Logging.AddSerilog();

// 🔹 Add Elasticsearch Client using ElasticSearchSettings
builder.Services.AddSingleton<IElasticClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<ElasticSearchSettings>>().Value;
    var connectionSettings = new ConnectionSettings(new Uri(settings.Uri))
                             .DefaultIndex(settings.DefaultIndex); // Use default index from settings
    return new ElasticClient(connectionSettings);
});

// 🔹 Register the ProductSearchRepository
builder.Services.AddScoped<IProductSearchRepository, ProductSearchRepository>();

// 🔹 Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔹 Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()   // Allow all origins (use more restrictive policies in production)
            .AllowAnyMethod()   // Allow any HTTP method
            .AllowAnyHeader()); // Allow any HTTP header
});

var app = builder.Build();

// 🔹 Ensure Elasticsearch is connected on startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
EnsureDatabaseConnections(app.Services, logger);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Search API v1");
        options.RoutePrefix = string.Empty; // Makes Swagger available at root URL
    });
}

// Apply the CORS middleware
app.UseCors("AllowAll");  // Use the CORS policy defined above

// Configure middleware
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok("Product Search API is running 🚀"));

app.Run();

void EnsureDatabaseConnections(IServiceProvider services, Microsoft.Extensions.Logging.ILogger logger)
{
    using var scope = services.CreateScope();
    var provider = scope.ServiceProvider;

    try
    {
        // 🔹 Elasticsearch Connection Check
        var elasticClient = provider.GetRequiredService<IElasticClient>();
        for (int attempt = 1; attempt <= 5; attempt++)
        {
            var pingResponse = elasticClient.Ping();
            if (pingResponse.IsValid)
            {
                logger.LogInformation("✅ Elasticsearch connected on attempt {Attempt}.", attempt);
                return;
            }

            logger.LogWarning("⚠️ Elasticsearch connection failed (Attempt {Attempt}/5): {ErrorMessage}",
                              attempt, pingResponse.OriginalException?.Message ?? "Unknown error");

            Thread.Sleep(2000); // Retry delay
        }

        logger.LogError("❌ Failed to connect to Elasticsearch after multiple attempts.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error during service initialization.");
    }
}

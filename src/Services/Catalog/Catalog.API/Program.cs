﻿//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Catalog.API
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            CreateHostBuilder(args).Build().Run();
//        }

//        public static IHostBuilder CreateHostBuilder(string[] args) =>
//            Host.CreateDefaultBuilder(args)
//                .ConfigureWebHostDefaults(webBuilder =>
//                {
//                    webBuilder.UseStartup<Startup>();
//                });
//    }
//}


//using System;
//using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using Catalog.API.Authorization;
//using Catalog.API.Data;
//using Catalog.API.Filters;
//using Catalog.API.Repositories;
//using Catalog.API.Repositories.Interfaces;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.OpenApi.Models;
//using MongoDB.Driver;

//var builder = WebApplication.CreateBuilder(args);
//var configuration = builder.Configuration;

//// Clear Microsoft claim name mappings to preserve original JWT claims
//JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

//// Add HTTP context accessor service
//builder.Services.AddHttpContextAccessor();

//// Configure authentication using IdentityServer4
//builder.Services.AddAuthentication(IdentityServer4.AccessTokenValidation.IdentityServerAuthenticationDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.Authority = configuration["IdentityProviderSettings:IdentityServiceUrl"];
//        options.Audience = "catalogapi";
//        options.RequireHttpsMetadata = false;
//    });

//// Configure authorization policies
////builder.Services.AddAuthorization(options =>
////{
////    // Role-based policies
////    options.AddPolicy("CanRead", policy => policy.RequireRole("Administrator", "User"));
////    options.AddPolicy("HasFullAccess", policy => policy.RequireRole("Administrator"));

////    // Scope-based policies
////    options.AddPolicy("CanReadScope", policy => policy.RequireClaim("scope", "catalogapi.read", "catalogapi.fullaccess"));
////    options.AddPolicy("HasFullAccessScope", policy => policy.RequireClaim("scope", "catalogapi.fullaccess"));

////    // Combined policies (OR logic between scopes and roles)
////    options.AddPolicy("CanRead", policy => policy.Requirements.Add(new CombinedRequirement(
////        options.GetPolicy("CanReadScope").Requirements.ToList(),
////        options.GetPolicy("CanReadRole").Requirements.ToList())));

////    options.AddPolicy("HasFullAccess", policy => policy.Requirements.Add(new CombinedRequirement(
////        options.GetPolicy("HasFullAccessScope").Requirements.ToList(),
////        options.GetPolicy("HasFullAccessRole").Requirements.ToList())));
////});
//// Configure authorization policies
//builder.Services.AddAuthorization(options =>
//{
//    // Role-based policies
//    options.AddPolicy("CanReadRole", policy => policy.RequireRole("Administrator", "User"));
//    options.AddPolicy("HasFullAccessRole", policy => policy.RequireRole("Administrator"));

//    // Scope-based policies
//    options.AddPolicy("CanReadScope", policy => policy.RequireClaim("scope", "catalogapi.read", "catalogapi.fullaccess"));
//    options.AddPolicy("HasFullAccessScope", policy => policy.RequireClaim("scope", "catalogapi.fullaccess"));

//    // Combined policies (OR logic between scopes and roles)
//    options.AddPolicy("CanRead", policy => policy.Requirements.Add(new CombinedRequirement(
//        options.GetPolicy("CanReadScope").Requirements.ToList(),
//        options.GetPolicy("CanReadRole").Requirements.ToList())));

//    options.AddPolicy("HasFullAccess", policy => policy.Requirements.Add(new CombinedRequirement(
//        options.GetPolicy("HasFullAccessScope").Requirements.ToList(),
//        options.GetPolicy("HasFullAccessRole").Requirements.ToList())));
//});

//// Register custom authorization handler
//builder.Services.AddScoped<IAuthorizationHandler, CombinedRequirementHandler>();

//// Add MVC controllers
//builder.Services.AddControllers();

//// Configure Swagger for API documentation
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog.API", Version = "v1" });
//    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
//    {
//        Type = SecuritySchemeType.OAuth2,
//        Flows = new OpenApiOAuthFlows()
//        {
//            Implicit = new OpenApiOAuthFlow()
//            {
//                AuthorizationUrl = new Uri($"{configuration.GetValue<string>("IdentityProviderSettings:IdentityServiceUrl")}/connect/authorize"),
//                TokenUrl = new Uri($"{configuration.GetValue<string>("IdentityProviderSettings:IdentityServiceUrl")}/connect/token"),
//                Scopes = new Dictionary<string, string>
//                {
//                    { "catalogapi.fullaccess", "Catalog API" }
//                }
//            }
//        }
//    });
//    c.OperationFilter<AuthorizeCheckOperationFilter>();
//});

//// Register application services for dependency injection
//builder.Services.AddScoped<ICatalogContext, CatalogContext>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();

//var app = builder.Build();

//// Configure middleware pipeline
//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//    app.UseSwagger();
//    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog.API v1"));
//}

//app.UseRouting();
//app.UseAuthorization();
//app.MapControllers().RequireAuthorization();

//// Seed product data into MongoDB
//SeedDatabase(app);

//app.Run();

///// <summary>
///// Seeds initial product data into the database if empty.
///// </summary>
///// <param name="app">The application instance.</param>
//void SeedDatabase(WebApplication app)
//{
//    using var scope = app.Services.CreateScope();
//    var services = scope.ServiceProvider;
//    var logger = services.GetRequiredService<ILogger<Program>>();
//    var context = services.GetRequiredService<ICatalogContext>();
//    try
//    {
//        // Ensure the database and collection exist before seeding data
//        var productCollection = context.Products;
//        // Check if the collection is empty, and seed data if so
//        if (productCollection != null && !productCollection.AsQueryable().Any())
//        {
//            CatalogContextSeed.SeedData(productCollection);
//        }
//        else
//        {
//            logger.LogWarning("Product collection is not available or already seeded.");
//        }
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, "An error occurred while seeding the database.");
//    }
//}



using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Catalog.API.Authorization;
using Catalog.API.Data;
using Catalog.API.Entities;
using Catalog.API.Filters;
using Catalog.API.Repositories;
using Catalog.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add logging immediately at the start of the application
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole(); // Or any other logging provider
});
var logger = loggerFactory.CreateLogger("Startup");

logger.LogInformation("Application starting...");

// Clear Microsoft claim name mappings to preserve original JWT claims
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Add HTTP context accessor service
builder.Services.AddHttpContextAccessor();

// Configure authentication using IdentityServer4
builder.Services.AddAuthentication(IdentityServer4.AccessTokenValidation.IdentityServerAuthenticationDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        try
        {
            options.Authority = configuration["IdentityProviderSettings:IdentityServiceUrl"];
            options.Audience = "catalogapi";
            options.RequireHttpsMetadata = false;
            logger.LogInformation("Authentication configured.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error configuring authentication.");
        }
    });

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    try
    {
        // Role-based policies
        options.AddPolicy("CanReadRole", policy => policy.RequireRole("Administrator", "User"));
        options.AddPolicy("HasFullAccessRole", policy => policy.RequireRole("Administrator"));

        // Scope-based policies
        options.AddPolicy("CanReadScope", policy => policy.RequireClaim("scope", "catalogapi.read", "catalogapi.fullaccess"));
        options.AddPolicy("HasFullAccessScope", policy => policy.RequireClaim("scope", "catalogapi.fullaccess"));

        // Combined policies (OR logic between scopes and roles)
        options.AddPolicy("CanRead", policy => policy.Requirements.Add(new CombinedRequirement(
            options.GetPolicy("CanReadScope").Requirements.ToList(),
            options.GetPolicy("CanReadRole").Requirements.ToList())));

        options.AddPolicy("HasFullAccess", policy => policy.Requirements.Add(new CombinedRequirement(
            options.GetPolicy("HasFullAccessScope").Requirements.ToList(),
            options.GetPolicy("HasFullAccessRole").Requirements.ToList())));
        logger.LogInformation("Authorization policies configured.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error configuring authorization.");
    }
});

// Register custom authorization handler
builder.Services.AddScoped<IAuthorizationHandler, CombinedRequirementHandler>();

// Add MVC controllers
builder.Services.AddControllers();

// Configure Swagger for API documentation
builder.Services.AddSwaggerGen(c =>
{
    try
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog.API", Version = "v1" });
        c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows()
            {
                Implicit = new OpenApiOAuthFlow()
                {
                    AuthorizationUrl = new Uri($"{configuration.GetValue<string>("IdentityProviderSettings:IdentityServiceUrl")}/connect/authorize"),
                    TokenUrl = new Uri($"{configuration.GetValue<string>("IdentityProviderSettings:IdentityServiceUrl")}/connect/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "catalogapi.fullaccess", "Catalog API" }
                    }
                }
            }
        });
        c.OperationFilter<AuthorizeCheckOperationFilter>();
        logger.LogInformation("Swagger configured.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error configuring Swagger.");
    }
});

// Register application services for dependency injection
builder.Services.AddScoped<ICatalogContext, CatalogContext>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog.API v1"));
    logger.LogInformation("Developer environment middleware configured.");
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers().RequireAuthorization();

// Seed product data into MongoDB
SeedDatabase(app, logger); // Pass the logger here.

app.Run();

/// <summary>
/// Seeds initial product data into the database if empty and ensures necessary indexes exist.  (MongoDB 4.0 Compatible)
/// </summary>
/// <param name="app">The application instance.</param>
/// <param name="logger">The logger instance for logging operations.</param>

void SeedDatabase(WebApplication app, ILogger logger)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ICatalogContext>();
        var productCollection = context.Products;

        var seedLogger = services.GetRequiredService<ILogger<CatalogContextSeed>>();

        // ✅ Get the list of existing indexes
        var existingIndexes = productCollection.Indexes.List().ToList();

        // ✅ Ensure single-field index on 'category'
        bool categoryIndexExists = existingIndexes.Any(index => index["key"].AsBsonDocument.Contains("category"));
        if (!categoryIndexExists)
        {
            var categoryIndexDefinition = Builders<Product>.IndexKeys.Ascending(p => p.Category);
            var categoryIndexModel = new CreateIndexModel<Product>(categoryIndexDefinition);
            productCollection.Indexes.CreateOne(categoryIndexModel);
            logger.LogInformation("✅ Index on 'category' field created.");
        }
        else
        {
            logger.LogInformation("🔹 Index on 'category' already exists.");
        }

        // ✅ Ensure single-field index on 'name'
        bool nameIndexExists = existingIndexes.Any(index => index["key"].AsBsonDocument.Contains("name"));
        if (!nameIndexExists)
        {
            var nameIndexDefinition = Builders<Product>.IndexKeys.Ascending(p => p.Name);
            var nameIndexModel = new CreateIndexModel<Product>(nameIndexDefinition);
            productCollection.Indexes.CreateOne(nameIndexModel);
            logger.LogInformation("✅ Index on 'name' field created.");
        }
        else
        {
            logger.LogInformation("🔹 Index on 'name' already exists.");
        }

        // ✅ Ensure single-field index on 'description'
        bool descriptionIndexExists = existingIndexes.Any(index => index["key"].AsBsonDocument.Contains("description"));
        if (!descriptionIndexExists)
        {
            var descriptionIndexDefinition = Builders<Product>.IndexKeys.Ascending(p => p.Description);
            var descriptionIndexModel = new CreateIndexModel<Product>(descriptionIndexDefinition);
            productCollection.Indexes.CreateOne(descriptionIndexModel);
            logger.LogInformation("✅ Index on 'description' field created.");
        }
        else
        {
            logger.LogInformation("🔹 Index on 'description' already exists.");
        }

        // ✅ Check if the collection is empty before seeding data
        if (!productCollection.AsQueryable().Any())
        {
            CatalogContextSeed.SeedData(productCollection, seedLogger);
            logger.LogInformation("✅ Database seeded successfully.");
        }
        else
        {
            logger.LogWarning("⚠️ Product collection is already seeded.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ An error occurred while seeding the database.");
    }
}







//void SeedDatabase(WebApplication app, ILogger logger)
//{
//    using var scope = app.Services.CreateScope();
//    var services = scope.ServiceProvider;

//    try
//    {
//        var context = services.GetRequiredService<ICatalogContext>();
//        var productCollection = context.Products;

//        var seedLogger = services.GetRequiredService<ILogger<CatalogContextSeed>>();

//        // ✅ Get the list of existing indexes
//        var existingIndexes = productCollection.Indexes.List().ToList();

//        // ✅ Ensure Single-Field Index on 'category'
//        bool categoryIndexExists = existingIndexes.Any(index => index["key"].AsBsonDocument.Contains("category"));
//        if (!categoryIndexExists)
//        {
//            var categoryIndexDefinition = Builders<Product>.IndexKeys.Ascending(p => p.Category);
//            productCollection.Indexes.CreateOne(new CreateIndexModel<Product>(categoryIndexDefinition));
//            logger.LogInformation("✅ Single-field Index on 'category' created.");
//        }
//        else
//        {
//            logger.LogInformation("🔹 Index on 'category' already exists.");
//        }

//        // ✅ Ensure Single-Field Index on 'name'
//        bool nameIndexExists = existingIndexes.Any(index => index["key"].AsBsonDocument.Contains("name"));
//        if (!nameIndexExists)
//        {
//            var nameIndexDefinition = Builders<Product>.IndexKeys.Ascending(p => p.Name);
//            productCollection.Indexes.CreateOne(new CreateIndexModel<Product>(nameIndexDefinition));
//            logger.LogInformation("✅ Single-field Index on 'name' created.");
//        }
//        else
//        {
//            logger.LogInformation("🔹 Index on 'name' already exists.");
//        }

//        // ✅ Ensure Single-Field Index on 'description'
//        bool descriptionIndexExists = existingIndexes.Any(index => index["key"].AsBsonDocument.Contains("description"));
//        if (!descriptionIndexExists)
//        {
//            var descriptionIndexDefinition = Builders<Product>.IndexKeys.Ascending(p => p.Description);
//            productCollection.Indexes.CreateOne(new CreateIndexModel<Product>(descriptionIndexDefinition));
//            logger.LogInformation("✅ Single-field Index on 'description' created.");
//        }
//        else
//        {
//            logger.LogInformation("🔹 Index on 'description' already exists.");
//        }

//        // ✅ Ensure Full-Text Search Index (MongoDB Atlas / CosmosDB Search API)
//        bool textIndexExists = existingIndexes.Any(index =>
//            index["key"].AsBsonDocument.Elements.Any(e => e.Value.AsInt32 == 2)); // Text indexes have a value of "2"

//        if (!textIndexExists)
//        {
//            var textIndexDefinition = Builders<Product>.IndexKeys.Text(p => p.Name).Text(p => p.Description);
//            productCollection.Indexes.CreateOne(new CreateIndexModel<Product>(textIndexDefinition));
//            logger.LogInformation("✅ Full-Text Search Index on 'name' and 'description' created.");
//        }
//        else
//        {
//            logger.LogInformation("🔹 Full-Text Index on 'name' and 'description' already exists.");
//        }

//        // ✅ Check if the collection is empty before seeding data
//        if (productCollection != null && !productCollection.AsQueryable().Any())
//        {
//            CatalogContextSeed.SeedData(productCollection, seedLogger);
//            logger.LogInformation("✅ Database seeded successfully.");
//        }
//        else
//        {
//            logger.LogWarning("⚠️ Product collection is not available or already seeded.");
//        }
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, "❌ An error occurred while seeding the database.");
//    }
//}



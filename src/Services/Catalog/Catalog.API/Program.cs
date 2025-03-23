//using Microsoft.AspNetCore.Hosting;
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
using IdentityModel;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
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

// Retrieve Identity Service URL from environment variable first, fallback to appsettings.json
//var identityServiceUrl = Environment.GetEnvironmentVariable("IDENTITY_SERVER_URL")
//                         ?? configuration["IdentityProviderSettings:IdentityServiceUrl"];



// Configure authentication using IdentityServer4 and JWT Bearer tokens

// Retrieve Identity Service URL from environment variable or fallback to config file
var identityServiceUrl = Environment.GetEnvironmentVariable("IDENTITY_SERVER_URL")
                         ?? configuration["IdentityProviderSettings:IdentityServiceUrl"]
                         ?? "amcart.centralindia.cloudapp.azure.com"; // ✅ Ensure it has a valid port

logger.LogInformation($"Resolved IdentityServer URL: {identityServiceUrl}");

// External URL for Swagger UI (used by browser)
var identityServicePublicUrl = Environment.GetEnvironmentVariable("IDENTITY_SERVER_PUBLIC_URL")
                               ?? configuration["IdentityProviderSettings:IdentityServicePublicUrl"]
                               ?? identityServiceUrl;

logger.LogInformation("IDENTITY_SERVER_URL..= " + identityServiceUrl);

logger.LogInformation("IDENTITY_SERVER_Public_URL..= " + identityServicePublicUrl);

// Log a warning if the Identity Service URL is not set (but don't throw an exception)
if (string.IsNullOrEmpty(identityServiceUrl))
{
    logger.LogWarning("Identity Service URL is not configured! Using default fallback or authentication may fail.");
}

// 🛠️ Validate the Authority URL to prevent misconfiguration
if (!Uri.TryCreate(identityServiceUrl, UriKind.Absolute, out _))
{
    throw new InvalidOperationException($"Invalid IdentityServer URL: {identityServiceUrl}");
}


// Disable automatic claim mapping
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Configure authentication using IdentityServer4 and JWT Bearer tokens
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = identityServiceUrl.TrimEnd().TrimEnd('/'); // ✅ Remove trailing spaces & slashes
        options.Audience = "catalogapi"; // ✅ Matches API resource in IdentityServer
        options.RequireHttpsMetadata = false; // ❗ Only disable in development
        IdentityModelEventSource.ShowPII = true;
        //options.MapInboundClaims = false;


        options.TokenValidationParameters = new TokenValidationParameters
        {
            //RoleClaimType = JwtClaimTypes.Role, // ✅ Map "role" claim
            RoleClaimType = "role", // ✅ Explicitly map "role" as a role claim
            //opt.TokenValidationParameters.RoleClaimType = "role";
            //opt.TokenValidationParameters.NameClaimType = "name";
            NameClaimType = "name",  //JwtClaimTypes.PreferredUserName, // ✅ Map "preferred_username" claim
            ValidateIssuer = true, // ✅ Ensure token is issued by a trusted source
            ValidateAudience = true, // ✅ Ensure token is meant for this API
            ValidateLifetime = true // ✅ Ensure token has not expired


        };

    });


//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        try
//        {
//            options.Authority = identityServiceUrl; // ✅ IdentityServer URL
//            options.Audience = "catalogapi"; // ✅ Matches API resource in IdentityServer
//            options.RequireHttpsMetadata = false; // ❗ Only disable for development

//            // ✅ Ensure correct claim mappings
//            options.TokenValidationParameters = new TokenValidationParameters
//            {
//                RoleClaimType = JwtClaimTypes.Role, // IdentityServer4 uses "role"
//                NameClaimType = JwtClaimTypes.PreferredUserName, // "preferred_username"
//                ValidateIssuer = true,
//                ValidateAudience = true,
//                ValidateLifetime = true
//            };

//            logger.LogInformation($"Authentication configured with Authority: {identityServiceUrl ?? "NOT SET"}");
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "Error configuring authentication.");
//        }
//    });



//builder.Services.AddAuthentication(IdentityServer4.AccessTokenValidation.IdentityServerAuthenticationDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        try
//        {
//            // Set the Authority URL for token validation (from environment or config)
//            options.Authority = identityServiceUrl;

//            // Set the API audience (the API resource this service represents)
//            options.Audience = "catalogapi";

//            // Allow HTTP URLs (disable HTTPS metadata requirement, useful in local/dev environments)
//            options.RequireHttpsMetadata = false;

//            // Ensure correct role claim mapping
//            //options.TokenValidationParameters = new TokenValidationParameters
//            //{
//            //    RoleClaimType = "role",
//            //    NameClaimType = "name"
//            //};
//            options.TokenValidationParameters = new TokenValidationParameters
//            {
//                RoleClaimType = "role", // ✅ Ensure role claims are recognized
//                NameClaimType = JwtRegisteredClaimNames.Sub,

//            };



//            // Log the final authority URL used for authentication
//            logger.LogInformation($"Authentication configured with Authority: {identityServiceUrl ?? "NOT SET"}");
//        }
//        catch (Exception ex)
//        {
//            // Log any errors encountered while configuring authentication
//            logger.LogError(ex, "Error configuring authentication.");
//        }
//    });


// Configure authorization policies
//builder.Services.AddAuthorization(options =>
//{
//    try
//    {
//        // Role-based policies
//        options.AddPolicy("CanReadRole", policy => policy.RequireRole("Administrator", "User"));
//        options.AddPolicy("HasFullAccessRole", policy => policy.RequireRole("Administrator"));

//        // Scope-based policies
//        options.AddPolicy("CanReadScope", policy => policy.RequireClaim("scope", "catalogapi.read", "catalogapi.fullaccess"));
//        options.AddPolicy("HasFullAccessScope", policy => policy.RequireClaim("scope", "catalogapi.fullaccess"));

//        // Combined policies (OR logic between scopes and roles)
//        options.AddPolicy("CanRead", policy => policy.Requirements.Add(new CombinedRequirement(
//            options.GetPolicy("CanReadScope").Requirements.ToList(),
//            options.GetPolicy("CanReadRole").Requirements.ToList())));

//        options.AddPolicy("HasFullAccess", policy => policy.Requirements.Add(new CombinedRequirement(
//            options.GetPolicy("HasFullAccessScope").Requirements.ToList(),
//            options.GetPolicy("HasFullAccessRole").Requirements.ToList())));
//        logger.LogInformation("Authorization policies configured.");
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, "Error configuring authorization.");
//    }
//});

// Configure authorization policies
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
        options.AddPolicy("CanRead", policy => {
            var scopeRequirements = options.GetPolicy("CanReadScope").Requirements.OfType<ClaimsAuthorizationRequirement>().SelectMany(c => c.AllowedValues).Select(v => new ScopeRequirement(v)).ToList<IAuthorizationRequirement>();
            var roleRequirements = options.GetPolicy("CanReadRole").Requirements.OfType<RolesAuthorizationRequirement>().SelectMany(r => r.AllowedRoles).Select(r => new RoleRequirement { Role = r }).ToList<IAuthorizationRequirement>();

            policy.Requirements.Add(new CombinedRequirement(scopeRequirements, roleRequirements));
        });

        options.AddPolicy("HasFullAccess", policy => {
            var scopeRequirements = options.GetPolicy("HasFullAccessScope").Requirements.OfType<ClaimsAuthorizationRequirement>().SelectMany(c => c.AllowedValues).Select(v => new ScopeRequirement(v)).ToList<IAuthorizationRequirement>();
            var roleRequirements = options.GetPolicy("HasFullAccessRole").Requirements.OfType<RolesAuthorizationRequirement>().SelectMany(r => r.AllowedRoles).Select(r => new RoleRequirement { Role = r }).ToList<IAuthorizationRequirement>();

            policy.Requirements.Add(new CombinedRequirement(scopeRequirements, roleRequirements));
        });

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

        //string identityServerUrl = Environment.GetEnvironmentVariable("IDENTITY_SERVER_URL")
        //                          ?? configuration.GetValue<string>("IdentityProviderSettings:IdentityServiceUrl");

        c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows()
            {
                Implicit = new OpenApiOAuthFlow()
                {
                    //AuthorizationUrl = new Uri($"{identityServerUrl}/connect/authorize"),
                    //TokenUrl = new Uri($"{identityServerUrl}/connect/token"),
                    AuthorizationUrl = new Uri($"{identityServicePublicUrl}/connect/authorize"),
                    TokenUrl = new Uri($"{identityServicePublicUrl}/connect/token"),
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

//// Configure Swagger for API documentation
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog.API", Version = "v1" });

//    string identityServerUrl = Environment.GetEnvironmentVariable("IDENTITY_SERVER_URL")
//                              ?? configuration.GetValue<string>("IdentityProviderSettings:IdentityServiceUrl");

//    //c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
//    //{
//    //    Type = SecuritySchemeType.OAuth2,
//    //    Flows = new OpenApiOAuthFlows()
//    //    {
//    //        AuthorizationCode = new OpenApiOAuthFlow()
//    //        {
//    //            AuthorizationUrl = new Uri($"{identityServerUrl}/connect/authorize"),
//    //            TokenUrl = new Uri($"{identityServerUrl}/connect/token"),
//    //            Scopes = new Dictionary<string, string>
//    //            {
//    //                { "catalogapi.fullaccess", "Full access to Catalog API" }
//    //            }
//    //        }
//    //    }
//    //});

//    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
//    {
//        Type = SecuritySchemeType.OAuth2,
//        Flows = new OpenApiOAuthFlows()
//        {
//            Implicit = new OpenApiOAuthFlow()
//            {
//                AuthorizationUrl = new Uri("http://localhost:5000/connect/authorize"), // HTTP
//                TokenUrl = new Uri("http://localhost:5000/connect/token"), // HTTP
//                Scopes = new Dictionary<string, string>
//            {
//                { "catalogapi.fullaccess", "Catalog API Full Access" }
//            }
//            }
//        }
//    });

//    c.OperationFilter<AuthorizeCheckOperationFilter>();
//});






// Register application services for dependency injection
builder.Services.AddScoped<ICatalogContext, CatalogContext>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAngularApp",
//        policy => policy.WithOrigins("http://localhost:4200")
//                        .AllowAnyMethod()
//                        .AllowAnyHeader());
//});

var app = builder.Build();

// 🔹 Ensure static files are enabled for Swagger UI
app.UseStaticFiles();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog.API v1"));

    // 🔹 Configure Swagger UI with custom JavaScript
    app.UseSwaggerUI(c =>
    {
        //c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API v1");

        //// ✅ Inject custom JavaScript (Ensure the file exists in wwwroot/swagger/)
        //c.InjectJavascript("/swagger/custom-swagger.js");

        //// ✅ OAuth2 Configuration for IdentityServer4
        //c.OAuthClientId("catalogswaggerui"); // Your Swagger client ID
        //c.OAuthUsePkce(); // ✅ Enable PKCE to fix "code_challenge is missing" issue

        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API v1");

        // ✅ Inject custom JavaScript (Optional, ensure the file exists in wwwroot/swagger/)
        c.InjectJavascript("/swagger/custom-swagger.js");

        // ✅ OAuth2 Configuration for Implicit Flow
        c.OAuthClientId("catalogswaggerui");  // 🔹 Use your Swagger Client ID
        //c.OAuthUsePkce(false);  // 🔹 PKCE is not required for Implicit Flow
        //c.OAuthUseBasicAuthenticationWithAccessCodeGrant(false); // 🔹 Prevents asking for client secret
        c.OAuthScopeSeparator(" "); // 🔹 Space-separated scopes
    });

    logger.LogInformation("Developer environment middleware configured.");
}



// Enable CORS before routing
//app.UseCors("AllowAngularApp"); // 🔥 Add this here
var angularClientUrl = Environment.GetEnvironmentVariable("ANGULAR_CLIENT_URL") ?? "http://localhost:4200";

app.UseCors(builder =>
     //builder.WithOrigins(angularClientUrl)  // ✅ Allow frontend from env variable or default
     builder.SetIsOriginAllowed(_ => true)
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials());  // ✅ Required for secured endpoints

app.UseRouting();
app.UseAuthorization();
app.MapControllers().RequireAuthorization();

// Seed product data into MongoDB
SeedDatabase(app, logger); // Pass the logger here.

IdentityModelEventSource.ShowPII = true;
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



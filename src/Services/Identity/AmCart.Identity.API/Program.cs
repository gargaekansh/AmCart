
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AmCart.Identity.API.Data;
using AmCart.Identity.API.Models;
using Microsoft.Extensions.DependencyInjection;
using AmCart.Identity.API.Configuration;
using AmCart.Identity.API.Services;
using AmCart.Identity.API.Services.Interfaces;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using IdentityModel;
using System.Security.Claims;
using IdentityServer4.EntityFramework.DbContexts;

var builder = WebApplication.CreateBuilder(args);

// In this system we are using Nginx Ingress, therefore we need to resend the headers into this service
#region ReverseProxy - header forwarding
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Everything what was configured implicitly, we need to reset.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
#endregion

// 🔹 Ensure correct Role and User ID claims mapping
//builder.Services.Configure<IdentityOptions>(options =>
//{
//    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role; // Map role claim
//    options.ClaimsIdentity.UserIdClaimType = JwtClaimTypes.Subject; // Map user ID claim
//});





/*********************** Uncomment THis for JwtClaimTypes.Role; ***********************/
//////builder.Services.Configure<IdentityOptions>(options =>
//////{
//////    //options.ClaimsIdentity.RoleClaimType = "role"; // Match the claim in JWT
//////    options.ClaimsIdentity.RoleClaimType = JwtClaimTypes.Role;
//////    options.ClaimsIdentity.UserIdClaimType = JwtClaimTypes.Subject;
//////});
///



// 🔹 Enable MVC and Razor Pages
builder.Services.AddControllersWithViews(); // ✅ Enable MVC Controllers & Views
builder.Services.AddRazorPages(); // ✅ Enable Razor Pages

//builder.Services.AddControllers(); // Add this line!

// Load configuration from appsettings.json and environment variables
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables() // Load environment variables
    .Build();


//// In this system we are using Nginx Ingress, therefore we need to resend the headers into this service
//#region ReverseProxy - header forwarding
//builder.Services.Configure<ForwardedHeadersOptions>(options =>
//{
//    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

//    // Everything what was configured implicitly, we need to reset.
//    options.KnownNetworks.Clear();
//    options.KnownProxies.Clear();
//});
//#endregion


// 🔹 Configure Services
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<ICustomTokenService, CustomTokenService>();
builder.Services.AddTransient<IProfileService, ProfileService>();
builder.Services.AddTransient<ITokenService, DefaultTokenService>();
builder.Services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
builder.Services.AddTransient<IExtensionGrantValidator, TokenExchangeExtensionGrantValidator>();

// 🔹 Configure Database
// Configure database connection



var connectionString = Environment.GetEnvironmentVariable("IDENTITY_DB_CONNECTION")
                     ?? builder.Configuration.GetConnectionString("IdentityDb"); // Fallback to appsettings.json

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDb")));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

//builder.Services.AddDbContext<PersistedGrantDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDb")));


var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
//var connectionString = builder.Configuration.GetConnectionString("IdentityDb");




// 🔹 Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register PersistedGrantDbContext for IdentityServer4
builder.Services.AddDbContext<AmCart.Identity.API.Data.PersistedGrantDbContext>(options =>
    options.UseSqlServer(connectionString));


//// 🔹 Configure IdentityServer
//builder.Services.AddIdentityServer(options =>
//{
//    options.Events.RaiseErrorEvents = true;
//    options.Events.RaiseInformationEvents = true;
//    options.Events.RaiseFailureEvents = true;
//    options.Events.RaiseSuccessEvents = true;
//    options.EmitStaticAudienceClaim = true;
//    //options.IssuerUri = "AmCart";
//    options.EmitStaticAudienceClaim = true;

//    //options.SupportedGrantTypes = options.SupportedGrantTypes.Append("urn:ietf:params:oauth:grant-type:token-exchange").ToList();  // Append is usually safer

//    //// Now you can access SupportedGrantTypes
//    //options.SupportedGrantTypes.Add("urn:ietf:params:oauth:grant-type:token-exchange");

//})
//     .AddProfileService<ProfileService>()   // Register your custom profile service
//.AddDeveloperSigningCredential()  // Use a real certificate in production
//.AddAspNetIdentity<ApplicationUser>()
//.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
//.AddInMemoryIdentityResources(Config.IdentityResources)
//.AddInMemoryApiResources(Config.ApiResources)
//.AddInMemoryApiScopes(Config.ApiScopes)
//.AddInMemoryClients(Config.Clients(builder.Configuration)




//);


// Retrieve Identity Service URL from environment variable or fallback to config file
var identityServiceUrl = Environment.GetEnvironmentVariable("IDENTITY_SERVER_URL") ??
                          configuration["IdentityIssuer"]
                         ?? "amcart.centralindia.cloudapp.azure.com"; 


// 🔹 Configure IdentityServer
builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
    options.EmitStaticAudienceClaim = true;
    options.IssuerUri = identityServiceUrl;

})

.AddProfileService<ProfileService>()   // Register custom profile service
.AddDeveloperSigningCredential()  // 🔒 Use a real certificate in production
.AddAspNetIdentity<ApplicationUser>()
.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
.AddInMemoryIdentityResources(Config.IdentityResources)
.AddInMemoryApiResources(Config.ApiResources)
.AddInMemoryApiScopes(Config.ApiScopes)

//.AddInMemoryClients(Config.Clients(builder.Configuration))
.AddInMemoryClients(Config.Clients(configuration))
.AddExtensionGrantValidator<TokenExchangeExtensionGrantValidator>()


                //// 🔹 Add Operational & Configuration Store (for Persisted Grants)
                //// this adds the config data from DB (clients, resources)
                //.AddConfigurationStore(options =>
                //{
                //    options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString,
                //    sqlServerOptionsAction: sqlOptions =>
                //    {
                //        sqlOptions.MigrationsAssembly(migrationsAssembly);
                //        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                //    });
                //})
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString,
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(migrationsAssembly);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    });

                    // 🔄 Enable token cleanup
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 3600; // Cleanup every 1 hour
                    //options.DeviceFlowCodes = false; // ADD THIS LINE TO DISABLE IT

                }
);







//builder.Services.AddAuthentication();

builder.Services.AddAuthorization(); // AddAuthorization is still required
builder.Services.AddScoped<ApplicationDbContextInitialiser>();

// 🔹 Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity Microservice", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// 🔹 Add Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(
        //connectionString: builder.Configuration.GetConnectionString("IdentityDb")!,
        connectionString: connectionString!,
        healthQuery: "SELECT 1", // Ensures a simple check
        name: "sql",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "sql", "sqlserver" });

var app = builder.Build();

// 🔹 Apply Migrations & Seed Database in Development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
        initialiser.MigrateDatabaseAndSeed();
    }

    // 🔹 Enable Swagger UI
    app.UseSwagger(o =>
    {
        o.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
        {
            swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = @"/" } };
        });
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
        //options.RoutePrefix = string.Empty; // Swagger UI at root
        options.RoutePrefix = "swagger"; //string.Empty;  // Makes Swagger UI available at root URL
    });
}


// 🔹 Middleware Setup


app.UseForwardedHeaders();
app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax });

var angularClientUrl = Environment.GetEnvironmentVariable("ANGULAR_CLIENT_URL") ?? "http://localhost:4200";

app.UseCors(builder =>
     //builder.WithOrigins(angularClientUrl)  // ✅ Allow frontend from env variable or default
     builder.SetIsOriginAllowed(_ => true)
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials());  // ✅ Required for secured endpoints

//app.UseCors(builder => builder.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials());

//// ✅ Apply CORS before Routing
//app.UseCors(builder =>
//    builder.WithOrigins("http://localhost:4200")
//           .AllowAnyHeader()
//           .AllowAnyMethod()
//           .AllowCredentials());

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();

// ✅ Use top-level route registrations (Fixes ASP0014 warning)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // 👈 Conventional routing

app.MapRazorPages();  // ✅ Razor Pages
app.MapControllers(); // ✅ Attribute-based routing
app.MapHealthChecks("/hc", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();


//// 🔹 Middleware Setup
////app.UseRouting();
////app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax });
//app.UseForwardedHeaders();
//app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax });
//app.UseStaticFiles(); // Enable the static files from wwwroot directory
//app.UseRouting();
//app.UseAuthentication();
//app.UseStaticFiles();

//app.UseIdentityServer();
//app.UseAuthorization();

//// 🔹 Map Controllers, Razor Pages & Health Checks
//app.MapControllers();
//app.MapRazorPages(); // ✅ Map Razor Pages for IdentityServer UI
//app.MapHealthChecks("/hc", new HealthCheckOptions
//{
//    Predicate = _ => true,
//    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
//});



//app.Run();






//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using AmCart.Identity.API.Data;
//using AmCart.Identity.API.Models;
//using Microsoft.Extensions.DependencyInjection;
//using AmCart.Identity.API.Configuration;
//using AmCart.Identity.API.Services;
//using AmCart.Identity.API.Services.Interfaces;
//using Microsoft.AspNetCore.Diagnostics.HealthChecks;
//using HealthChecks.UI.Client;
//using Microsoft.OpenApi.Models;
//using System.Reflection;
//using Microsoft.Extensions.Diagnostics.HealthChecks;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
//builder.Services.AddScoped<ITokenService, TokenService>(); // Register TokenService for DI

//// Configure Database Connection
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDb")));

//// Configure Identity
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//    .AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddDefaultTokenProviders();

//// Configure IdentityServer4 with ASP.NET Identity
//builder.Services.AddIdentityServer(options =>
//{
//    options.Events.RaiseErrorEvents = true;
//    options.Events.RaiseInformationEvents = true;
//    options.Events.RaiseFailureEvents = true;
//    options.Events.RaiseSuccessEvents = true;

//    options.EmitStaticAudienceClaim = true;
//    options.IssuerUri = "AmCart";
//})
//    .AddDeveloperSigningCredential() // Use a real certificate in production
//    .AddAspNetIdentity<ApplicationUser>()
//    .AddInMemoryIdentityResources(Config.IdentityResources)
//    .AddInMemoryApiResources(Config.ApiResources)
//    .AddInMemoryApiScopes(Config.ApiScopes)
//    .AddInMemoryClients(Config.Clients(builder.Configuration));

//builder.Services.AddAuthentication();
//builder.Services.AddScoped<ApplicationDbContextInitialiser>();

//// 🔹 Add Swagger to API Documentation
//builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen(c =>
//{
//    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity Microservice", Version = "v1" });
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
//    });
//    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });

//    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
//    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
//    c.IncludeXmlComments(xmlPath);
//});

////// Add health checks
////builder.Services.AddHealthChecks()
////    .AddSqlServer(
////        builder.Configuration.GetConnectionString("IdentityDb")
////    );



//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    // Initialise and seed database
//    using (var scope = app.Services.CreateScope())
//    {
//        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
//        initialiser.MigrateDatabaseAndSeed();
//    }




//    //app.UseSwagger();
//    //app.UseSwaggerUI(options =>
//    //{
//    //    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
//    //    options.RoutePrefix = string.Empty;  // Swagger UI at root
//    //});

//    app.UseSwagger(o =>
//    {
//        o.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
//        {
//            swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = @"/" } };
//        });
//    });
//    app.UseSwaggerUI(options =>
//    {
//        // o.SwaggerEndpoint("../swagger/v1/swagger.json", "v1");

//        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
//        options.RoutePrefix = string.Empty;  // Swagger UI at root
//    });
//}

//app.UseRouting();
//app.UseIdentityServer();


////// Health checks endpoint
////app.MapHealthChecks("/hc", new HealthCheckOptions()
////{
////    Predicate = _ => true,
////    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
////});

//app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax });


////app.UseAuthorization();
////app.UseIdentityServer();



//app.Run();



//void ConfigureServices(IServiceCollection services)
//{




//    //app.UseCors(builder => builder.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials());

//    app.UseStaticFiles();

//    app.UseForwardedHeaders();
//    ////app.UseIdentityServer();

//    // Fix a problem with chrome. Chrome enabled a new feature "Cookies without SameSite must be secure", 
//    // the coockies shold be expided from https, but in eShop, the internal comunicacion in aks and docker compose is http.
//    // To avoid this problem, the policy of cookies shold be in Lax mode.
//    app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax });
//    //app.UseRouting();
//    app.UseAuthentication();
//    app.UseAuthorization();


//    //...

//    services.AddHealthChecks();


//    app.UseHealthChecks("/health");


//    app.MapControllers();

//    app.UseEndpoints(endpoints =>
//    {
//        endpoints.MapDefaultControllerRoute(); endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
//        {
//            Predicate = _ => true,
//            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
//        });
//    });
//    //...
//}





//-----------------------------------------------------------------------


















//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using AmCart.Identity.API.Data;
//using AmCart.Identity.API.Models;
//using Microsoft.Extensions.DependencyInjection;
//using AmCart.Identity.API.Configuration;
//using AmCart.Identity.API.Services;
//using AmCart.Identity.API.Services.Interfaces;
//using Microsoft.AspNetCore.Diagnostics.HealthChecks;
//using HealthChecks.UI.Client;
//using Microsoft.OpenApi.Models;
//using System.Reflection;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
//builder.Services.AddScoped<ITokenService, TokenService>(); // Register TokenService for DI




//// Configure Database Connection
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDb")));

//// Configure Identity
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//    .AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddDefaultTokenProviders();

//// Configure IdentityServer4 with ASP.NET Identity
//builder.Services.AddIdentityServer(options =>
//{
//    options.Events.RaiseErrorEvents = true;
//    options.Events.RaiseInformationEvents = true;
//    options.Events.RaiseFailureEvents = true;
//    options.Events.RaiseSuccessEvents = true;

//    // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
//    options.EmitStaticAudienceClaim = true;
//    options.IssuerUri = "AmCart";
//})
//    .AddDeveloperSigningCredential() // Use a real certificate in production
//    .AddAspNetIdentity<ApplicationUser>()
//    .AddInMemoryIdentityResources(Config.IdentityResources)
//    .AddInMemoryApiResources(Config.ApiResources)
//    .AddInMemoryApiScopes(Config.ApiScopes)
//    .AddInMemoryClients(Config.Clients(builder.Configuration));

//builder.Services.AddAuthentication();
//builder.Services.AddScoped<ApplicationDbContextInitialiser>();

//// 🔹 Add Swagger to API Documentation
//builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity Microservice", Version = "v1" });
//    // To Enable authorization using Swagger (JWT)
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
//    });
//    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });

//    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
//    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
//    c.IncludeXmlComments(xmlPath);
//});

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    // Initialise and seed database
//    using (var scope = app.Services.CreateScope())
//    {
//        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
//        initialiser.MigrateDatabaseAndSeed();
//    }

//    app.UseSwagger();  // Enable Swagger for API documentation in development environment
//    app.UseSwaggerUI(options =>
//    {
//        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
//        options.RoutePrefix = string.Empty;  // Makes Swagger UI available at root URL
//    });
//}

////app.UseHealthChecks("/health");
//app.UseRouting();

//// Map health check endpoint
////app.MapHealthChecks("/health");

//// Middleware to handle cookies (for Chrome SameSite issues)
//app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax });

//app.UseIdentityServer();
//app.UseAuthentication();
//app.UseAuthorization();

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapDefaultControllerRoute();
//    endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
//    {
//        Predicate = _ => true,
//        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
//    });
//});

//app.MapControllers();

//app.Run();




























////var builder = WebApplication.CreateBuilder(args);

////// Add services to the container.

////builder.Services.AddControllers();
////// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
////builder.Services.AddOpenApi();

////var app = builder.Build();

////// Configure the HTTP request pipeline.
////if (app.Environment.IsDevelopment())
////{
////    app.MapOpenApi();
////}

////app.UseHttpsRedirection();

////app.UseAuthorization();

////app.MapControllers();

////app.Run();


////-------------------------------------------------------------------------//


//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using AmCart.Identity.API.Data;
//using AmCart.Identity.API.Models;
//using Microsoft.Extensions.DependencyInjection;
//using AmCart.Identity.API.Configuration;
//using AmCart.Identity.API.Services;
//using AmCart.Identity.API.Services.Interfaces;
//using Microsoft.AspNetCore.Diagnostics.HealthChecks;
//using HealthChecks.UI.Client;
//using Microsoft.OpenApi.Models;
//using System.Reflection;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
//builder.Services.AddScoped<ITokenService,TokenService>(); // Register TokenService for DI

//// Configure Database Connection
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDb")));

//// Configure Identity
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//    .AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddDefaultTokenProviders();

//// Configure IdentityServer4 with ASP.NET Identity
//builder.Services.AddIdentityServer(options =>
//{
//    options.Events.RaiseErrorEvents = true;
//    options.Events.RaiseInformationEvents = true;
//    options.Events.RaiseFailureEvents = true;
//    options.Events.RaiseSuccessEvents = true;

//    // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
//    options.EmitStaticAudienceClaim = true;
//    options.IssuerUri = "AmCart";
//})
//    .AddDeveloperSigningCredential() // Use a real certificate in production
//    .AddAspNetIdentity<ApplicationUser>()
//    .AddInMemoryIdentityResources(Config.IdentityResources)
//    .AddInMemoryApiResources(Config.ApiResources)
//    .AddInMemoryApiScopes(Config.ApiScopes)
//    .AddInMemoryClients(Config.Clients(builder.Configuration));


//builder.Services.AddAuthentication();
//builder.Services.AddScoped<ApplicationDbContextInitialiser>();

//// 🔹 Add Swagger to API Documentation
//builder.Services.AddEndpointsApiExplorer();


//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity Microservice", Version = "v1" });
//    // To Enable authorization using Swagger (JWT)    
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
//    });
//    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });

//    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
//    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
//    c.IncludeXmlComments(xmlPath);
//});



//var app = builder.Build();
//if (app.Environment.IsDevelopment())
//{
//    // app.UseMigrationsEndPoint();

//    // Initialise and seed database
//    using (var scope = app.Services.CreateScope())
//    {
//        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
//        initialiser.MigrateDatabaseAndSeed();
//    }

//    app.UseSwagger();  // Enable Swagger for API documentation in development environment
//    app.UseSwaggerUI(options =>
//    {
//        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
//        options.RoutePrefix = string.Empty;  // Makes Swagger UI available at root URL
//    });

//}

////app.UseMiddleware<LogContextMiddleware>();
//// app.MapGet("/", () => "Hello World!");

//app.UseHealthChecks("/health");
//app.UseSwagger();
//app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Microservice"));

//app.UseRouting();
//app.UseIdentityServer();
//app.UseAuthentication();
//app.UseAuthorization();

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapDefaultControllerRoute(); endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
//    {
//        Predicate = _ => true,
//        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
//    });
//});




//builder.Services.AddControllers();



//// Fix a problem with chrome. Chrome enabled a new feature "Cookies without SameSite must be secure", 
//// the coockies shold be expided from https, but in eShop, the internal comunicacion in aks and docker compose is http.
//// To avoid this problem, the policy of cookies shold be in Lax mode.
//app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax });
//app.UseRouting();

//app.UseStaticFiles();

//app.UseForwardedHeaders();

//app.UseIdentityServer();
//app.UseAuthorization();

//app.MapControllers();
//app.Run();


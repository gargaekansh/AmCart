﻿
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<ITokenService, TokenService>(); // Register TokenService for DI

// Configure Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDb")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure IdentityServer4 with ASP.NET Identity
builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
    options.EmitStaticAudienceClaim = true;
    options.IssuerUri = "AmCart";
})
    .AddDeveloperSigningCredential() // Use a real certificate in production
    .AddAspNetIdentity<ApplicationUser>()
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddInMemoryApiResources(Config.ApiResources)
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryClients(Config.Clients(builder.Configuration));

builder.Services.AddAuthentication();
builder.Services.AddScoped<ApplicationDbContextInitialiser>();

// 🔹 Add Swagger to API Documentation
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity Microservice", Version = "v1" });
    // To Enable authorization using Swagger (JWT)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Initialise and seed database
    using (var scope = app.Services.CreateScope())
    {
        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
        initialiser.MigrateDatabaseAndSeed();
    }

    app.UseSwagger();  // Enable Swagger for API documentation in development environment
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
        options.RoutePrefix = string.Empty;  // Makes Swagger UI available at root URL
    });
}

app.UseHealthChecks("/health");
app.UseRouting();

// Middleware to handle cookies (for Chrome SameSite issues)
app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax });

app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
    endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
});

app.MapControllers();

app.Run();




























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


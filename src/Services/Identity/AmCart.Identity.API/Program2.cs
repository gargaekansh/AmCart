
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
//using Microsoft.AspNetCore.HttpOverrides;
//using IdentityServer4.Services;
//using IdentityServer4.Validation;
//using IdentityModel;
//using System.Security.Claims;
//using IdentityServer4.EntityFramework.DbContexts;

//var builder = WebApplication.CreateBuilder(args);

//// 🔹 Ensure correct Role and User ID claims mapping
//builder.Services.Configure<IdentityOptions>(options =>
//{
//    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role; // Map role claim
//    options.ClaimsIdentity.UserIdClaimType = JwtClaimTypes.Subject; // Map user ID claim
//});

//// 🔹 Enable MVC and Razor Pages
//builder.Services.AddControllersWithViews(); // ✅ Enable MVC Controllers & Views
//builder.Services.AddRazorPages(); // ✅ Enable Razor Pages

////builder.Services.AddControllers(); // Add this line!

//// Load configuration from appsettings.json and environment variables
//var configuration = new ConfigurationBuilder()
//    .SetBasePath(Directory.GetCurrentDirectory())
//    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
//    .AddEnvironmentVariables() // Load environment variables
//    .Build();



//// 🔹 Configure Services
//builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
//////builder.Services.AddScoped<ITokenService, TokenService>();
//builder.Services.AddTransient<IProfileService, ProfileService>();
//builder.Services.AddTransient<ITokenService, DefaultTokenService>();
//builder.Services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
//builder.Services.AddTransient<IExtensionGrantValidator, TokenExchangeExtensionGrantValidator>();

//// 🔹 Configure Database
//// Configure database connection



//var connectionString = Environment.GetEnvironmentVariable("IDENTITY_DB_CONNECTION")
//                     ?? builder.Configuration.GetConnectionString("IdentityDb"); // Fallback to appsettings.json

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));


//var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
////var connectionString = builder.Configuration.GetConnectionString("IdentityDb");




//// 🔹 Configure Identity
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//    .AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddDefaultTokenProviders();

//// Register PersistedGrantDbContext for IdentityServer4
//builder.Services.AddDbContext<AmCart.Identity.API.Data.PersistedGrantDbContext>(options =>
//    options.UseSqlServer(connectionString));






//// 🔹 Configure IdentityServer
//builder.Services.AddIdentityServer(options =>
//{
//    options.Events.RaiseErrorEvents = true;
//    options.Events.RaiseInformationEvents = true;
//    options.Events.RaiseFailureEvents = true;
//    options.Events.RaiseSuccessEvents = true;
//    options.EmitStaticAudienceClaim = true;

//})
//.AddProfileService<ProfileService>()   // Register custom profile service
//.AddDeveloperSigningCredential()  // 🔒 Use a real certificate in production
//.AddAspNetIdentity<ApplicationUser>()
//.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
//.AddInMemoryIdentityResources(Config.IdentityResources)
//.AddInMemoryApiResources(Config.ApiResources)
//.AddInMemoryApiScopes(Config.ApiScopes)

////.AddInMemoryClients(Config.Clients(builder.Configuration))
//.AddInMemoryClients(Config.Clients(configuration))
//.AddExtensionGrantValidator<TokenExchangeExtensionGrantValidator>()


//                // this adds the operational data from DB (codes, tokens, consents)
//                .AddOperationalStore(options =>
//                {
//                    options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString,
//                    sqlServerOptionsAction: sqlOptions =>
//                    {
//                        sqlOptions.MigrationsAssembly(migrationsAssembly);
//                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
//                    });

//                    // 🔄 Enable token cleanup
//                    options.EnableTokenCleanup = true;
//                    options.TokenCleanupInterval = 3600; // Cleanup every 1 hour
//                    //options.DeviceFlowCodes = false; // ADD THIS LINE TO DISABLE IT

//                }
//);







////builder.Services.AddAuthentication();

//builder.Services.AddAuthorization(); // AddAuthorization is still required
//builder.Services.AddScoped<ApplicationDbContextInitialiser>();

//// 🔹 Configure Swagger
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity Microservice", Version = "v1" });
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
//    });
//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
//    });

//    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
//    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
//    c.IncludeXmlComments(xmlPath);
//});

//// 🔹 Add Health Checks
//builder.Services.AddHealthChecks()
//    .AddSqlServer(
//        //connectionString: builder.Configuration.GetConnectionString("IdentityDb")!,
//        connectionString: connectionString!,
//        healthQuery: "SELECT 1", // Ensures a simple check
//        name: "sql",
//        failureStatus: HealthStatus.Unhealthy,
//        tags: new[] { "db", "sql", "sqlserver" });

//var app = builder.Build();

//// 🔹 Apply Migrations & Seed Database in Development
//if (app.Environment.IsDevelopment())
//{
//    using (var scope = app.Services.CreateScope())
//    {
//        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
//        initialiser.MigrateDatabaseAndSeed();
//    }

//    // 🔹 Enable Swagger UI
//    app.UseSwagger(o =>
//    {
//        o.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
//        {
//            swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = @"/" } };
//        });
//    });

//    app.UseSwaggerUI(options =>
//    {
//        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
//        //options.RoutePrefix = string.Empty; // Swagger UI at root
//        options.RoutePrefix = "swagger"; //string.Empty;  // Makes Swagger UI available at root URL
//    });
//}

//// 🔹 Middleware Setup
//app.UseRouting();
//app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax });

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









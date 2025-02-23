//using Catalog.API.Data;
//using Catalog.API.Repositories;
//using Catalog.API.Repositories.Interfaces;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.OpenApi.Models;
//using MongoDB.Driver;
//using System;
//using System.Linq;
//using System.IdentityModel.Tokens.Jwt;
//using IdentityServer4.AccessTokenValidation;
//using Catalog.API.Filters;
//using System.Collections.Generic;
//using Microsoft.AspNetCore.Authorization;
//using System.Threading.Tasks;
//using Catalog.API.Authorization;

//namespace Catalog.API
//{
//    public class Startup
//    {
//        public Startup(IConfiguration configuration)
//        {
//            Configuration = configuration;

//            // clear Microsoft changed claim names from dictionary and preserve original ones
//            // e.g. Microsoft stack renames the 'sub' claim name to http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
//            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
//        }

//        public IConfiguration Configuration { get; }

//        // This method gets called by the runtime. Use this method to add services to the container.
//        public void ConfigureServices(IServiceCollection services)
//        {
//            services.AddHttpContextAccessor();

//            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
//            .AddJwtBearer(options =>
//            {
//                options.Authority = Configuration["IdentityProviderSettings:IdentityServiceUrl"];
//                options.Audience = "catalogapi";
//                options.RequireHttpsMetadata = false; // added because of a healthcheck
//            });

//            services.AddAuthorization(options =>
//            {
//                //options.AddPolicy("CanRead", policy => policy.RequireClaim("scope", "catalogapi.read", "catalogapi.fullaccess"));
//                //options.AddPolicy("HasFullAccess", policy => policy.RequireClaim("scope", "catalogapi.fullaccess"));

//                options.AddPolicy("CanRead", policy => policy.RequireRole("Administrator", "User"));
//                options.AddPolicy("HasFullAccess", policy => policy.RequireRole("Administrator"));


//                // Policies based on scopes
//                options.AddPolicy("CanReadScope", policy => policy.RequireClaim("scope", "catalogapi.read", "catalogapi.fullaccess"));
//                options.AddPolicy("HasFullAccessScope", policy => policy.RequireClaim("scope", "catalogapi.fullaccess"));

//                // Policies based on roles
//                options.AddPolicy("CanReadRole", policy => policy.RequireRole("Administrator", "User"));
//                options.AddPolicy("HasFullAccessRole", policy => policy.RequireRole("Administrator"));

//                // Combined Policies (OR logic)
//                options.AddPolicy("CanRead", policy => policy.Requirements.Add(new CombinedRequirement(
//                    options.GetPolicy("CanReadScope").Requirements.ToList(),
//                    options.GetPolicy("CanReadRole").Requirements.ToList())));

//                options.AddPolicy("HasFullAccess", policy => policy.Requirements.Add(new CombinedRequirement(
//                    options.GetPolicy("HasFullAccessScope").Requirements.ToList(),
//                    options.GetPolicy("HasFullAccessRole").Requirements.ToList())));



//            });


//            // Register the handler
//            services.AddScoped<IAuthorizationHandler, CombinedRequirementHandler>();

//            services.AddControllers();
//            ////services.AddSwaggerGen(c =>
//            ////{
//            ////    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog.API", Version = "v1" });
//            ////});
//            ///

//            services.AddSwaggerGen(c =>
//            {
//                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog.API", Version = "v1" });

//                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
//                {
//                    Type = SecuritySchemeType.OAuth2,
//                    Flows = new OpenApiOAuthFlows()
//                    {
//                        Implicit = new OpenApiOAuthFlow()
//                        {
//                            AuthorizationUrl = new Uri($"{Configuration.GetValue<string>("IdentityProviderSettings:IdentityServiceUrl")}/connect/authorize"),
//                            TokenUrl = new Uri($"{Configuration.GetValue<string>("IdentityProviderSettings:IdentityServiceUrl")}/connect/token"),
//                            Scopes = new Dictionary<string, string>()
//                            {
//                                { "catalogapi.fullaccess", "Catalog API" }
//                            }
//                        }
//                    }
//                });

//                c.OperationFilter<AuthorizeCheckOperationFilter>();
//            });

//            services.AddScoped<ICatalogContext, CatalogContext>();
//            services.AddScoped<IProductRepository, ProductRepository>();


//        }

//        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//        {
//            if (env.IsDevelopment())
//            {
//                app.UseDeveloperExceptionPage();
//                app.UseSwagger();
//                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog.API v1"));
//            }

//            app.UseRouting();

//            app.UseAuthorization();

//            app.UseEndpoints(endpoints =>
//            {
//                //endpoints.MapDefaultHealthChecks();
//                endpoints.MapControllers().RequireAuthorization();
//            });

//            // Seed Product Data
//            SeedDatabase(app);
//        }

//        private static void SeedDatabase(IApplicationBuilder app)
//        {
//            // Using DI to get CatalogContext and logger
//            using (var scope = app.ApplicationServices.CreateScope())
//            {
//                var services = scope.ServiceProvider;
//                var logger = services.GetRequiredService<ILogger<Startup>>();
//                var context = services.GetRequiredService<ICatalogContext>();
//                try
//                {
//                    // Ensure the database and collection exist before seeding data
//                    var productCollection = context.Products;

//                    if (productCollection != null)
//                    {
//                        // Check if the collection is empty, and seed data if so
//                        if (!productCollection.AsQueryable().Any())
//                        {
//                            CatalogContextSeed.SeedData(productCollection);
//                        }
//                    }
//                    else
//                    {
//                        logger.LogWarning("Product collection is not available.");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    logger.LogError(ex, "An error occurred while seeding the database.");
//                }
//            }
//        }





//    }
//}

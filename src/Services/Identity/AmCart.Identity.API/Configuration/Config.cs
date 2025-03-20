using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Security.Claims;

namespace AmCart.Identity.API.Configuration
{
    public static class Config
    {
        // User basic identity info (e.g. username, password)
        // They give access to user identity data
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(), // givenName, familyName claims will be returned
                new IdentityResources.Address(),
                new IdentityResource() { Name = "roles", DisplayName = "Roles", Description = "User roles", UserClaims = new[] { "role" } }
        //        new IdentityResource
        //{
        //    Name = "roles",
        //    DisplayName = "Roles",
        //    Description = "User roles",
        //        UserClaims = { "role", ClaimTypes.Role, JwtClaimTypes.Role } // ✅ Include all role claim types
        //}
            };

        // This will set 'aud' value in access token which is used for authentication on API
        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
                new ApiResource("catalogapi", "Catalog API")
                {
                    Scopes = { "catalogapi.read", "catalogapi.fullaccess" },
                    UserClaims = new List<string> { "role" },
                    //   UserClaims = { JwtClaimTypes.Role, JwtClaimTypes.PreferredUserName } // ✅ Ensure name claim is requested
                       ///UserClaims = { "role" } // Important!
                },
                new ApiResource("discountapi", "Discount API")
                {
                    Scopes = { "discount.fullaccess" }
                },
                new ApiResource("basketapi", "Basket API")
                {
                    Scopes = { "basketapi.fullaccess"},
                    UserClaims = new List<string> { "role" }
                },
                new ApiResource("shoppinggateway", "Shopping Gateway")
                {
                    Scopes = { "shoppinggateway.fullaccess" }
                },
                new ApiResource("orderapi", "Order API")
                {
                    Scopes = { "orderapi.fullaccess" }
                },
                new ApiResource("shoppingaggregator", "Shopping Aggregator")
                {
                    Scopes = { "shoppingaggregator.fullaccess" }
                },
                new ApiResource("ordersagaorchestrator", "Order Saga Orchestrator")
                {
                    Scopes = { "ordersagaorchestrator.fullaccess" }
                },
                new ApiResource("paymentapi", "Payment API")
                {
                    Scopes = { "paymentapi.fullaccess" }
                },
                new ApiResource("deliveryapi", "Delivery API")
                {
                    Scopes = { "deliveryapi.fullaccess" }
                },
            };

        // Defines API's that are going to be used from the Client
        // They give access to API's
        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("catalogapi.fullaccess", "Catalog API Full Access"),
                new ApiScope("catalogapi.read", "Catalog API Read Operations"),
                new ApiScope("basketapi.read", "Basket API Read Operations"),
                new ApiScope("basketapi.fullaccess", "Basket API Full Access"),
                new ApiScope("discount.fullaccess", "Discount API Full Access"),
                new ApiScope("shoppinggateway.fullaccess", "Shopping Gateway Full Access"),
                new ApiScope("orderapi.fullaccess", "Order API Operations"),
                new ApiScope("shoppingaggregator.fullaccess", "Shopping Aggregator Full Access"),
                new ApiScope("paymentapi.fullaccess", "Payment API Full Access"),
                new ApiScope("deliveryapi.fullaccess", "Delivery API Full Access"),
                new ApiScope("ordersagaorchestrator.fullaccess", "Order Saga Orchesteator Full Access"),
                new ApiScope(IdentityServerConstants.LocalApi.ScopeName),
            };

        public static IEnumerable<Client> Clients(IConfiguration configuration)
        {

            var test = configuration["WebClientUrls:Razor"];
            // Get JWT settings from configuration
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

            // Read values from environment variables (flat structure)
            var razorClientUri = Environment.GetEnvironmentVariable("WebClientUrls__Razor") ??
                                 configuration.GetValue<string>("WebClientUrls:Razor") ??
                                 "https://localhost:4999"; // Default value

            var angularClientUri = Environment.GetEnvironmentVariable("WebClientUrls__Angular") ??
                                   configuration.GetValue<string>("WebClientUrls:Angular") ??
                                   "http://localhost:4200"; // Default value


            var nodeClientUri = Environment.GetEnvironmentVariable("WebClientUrls__Node") ??
                             configuration.GetValue<string>("WebClientUrls:Node") ??
                             "http://localhost:3000"; // Default value


            var blazorClientUri = Environment.GetEnvironmentVariable("WebClientUrls__Blazor") ??
                                  configuration.GetValue<string>("WebClientUrls:Blazor") ??
                                  "https://localhost:5002"; // Default value

            //var catalogApiUrl = Environment.GetEnvironmentVariable("ApiUrls__Catalog") ??
            //                    configuration.GetValue<string>("ApiUrls:Catalog") ??
            //                    "http://localhost:8000"; // Default value

            //var catalogApiInternalUrl = Environment.GetEnvironmentVariable("ApiUrls__CatalogInternal")
            //                ?? configuration["ApiUrls:CatalogInternal"]
            //                ?? "http://amcart.catalog.api:8080";

            var catalogApiPublicUrl = Environment.GetEnvironmentVariable("ApiUrls__CatalogPublic")
                                      ?? configuration["ApiUrls:CatalogPublic"]
                                      ?? "http://localhost:8000";

            //var catalogApiRedirectUris =  $"{catalogApiPublicUrl}/swagger/oauth2-redirect.html" ;
            //Console.WriteLine("RedirectUris = " + catalogApiRedirectUris);
            //var catalogApiPostLogoutRedirectUris = $"{catalogApiPublicUrl}/swagger/";
            //Console.WriteLine("PostLogoutRedirectUris = " + catalogApiPostLogoutRedirectUris);

            var basketApiUrl = Environment.GetEnvironmentVariable("ApiUrls__Basket") ??
                               configuration.GetValue<string>("ApiUrls:Basket") ??
                               "http://localhost:5001"; // Default value

            var orderApiUrl = Environment.GetEnvironmentVariable("ApiUrls__Order") ??
                              configuration.GetValue<string>("ApiUrls:Order") ??
                              "http://localhost:5004"; // Default value

            var paymentApiUrl = Environment.GetEnvironmentVariable("ApiUrls__Payment") ??
                                configuration.GetValue<string>("ApiUrls:Payment") ??
                                "http://localhost:5009"; // Default value

            var deliveryApiUrl = Environment.GetEnvironmentVariable("ApiUrls__Delivery") ??
                                 configuration.GetValue<string>("ApiUrls:Delivery") ??
                                 "http://localhost:5010"; // Default value

            var shoppingAggregatorApiUrl = Environment.GetEnvironmentVariable("ApiUrls__ShoppingAggregator") ??
                                           configuration.GetValue<string>("ApiUrls:ShoppingAggregator") ??
                                           "http://localhost:5005"; // Default value

            // Retrieve client secrets from configuration
            var shoppingM2MSecret = configuration.GetValue<string>("ClientSecrets:ShoppingMachineClientSecret")?.Sha256() ?? string.Empty;
            var razorClientSecret = configuration.GetValue<string>("ClientSecrets:ShoppingRazorClientSecret")?.Sha256() ?? string.Empty;
            var blazorClientSecret = configuration.GetValue<string>("ClientSecrets:ShoppingBlazorClientSecret")?.Sha256() ?? string.Empty;

            // Token expiration settings
            var absoluteRefreshTokenLifetime = jwtSettings.RememberMeTokenExpirationDays * 86400;
            var slidingRefreshTokenLifetime = jwtSettings.TokenExpirationHours * 3600;


            return new Client[]
            { 
        // Client for machine to machine authentication (no UI interaction, client credentials flow)
        new Client
        {
            ClientName = "Shopping Machine 2 Machine Client",
            ClientId = "shoppingm2m",
            ClientSecrets =
            {
                // Retrieve the secret from appsettings.json and hash it
                new Secret(configuration["ClientSecrets:ShoppingMachineClientSecret"].Sha256())
            },
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            AllowedScopes =
            {
                // Allowed scope for the client
                "catalogapi.fullaccess"
            }
        },
        
        // Client for web app with interactive login (OAuth2 Authorization Code flow)
        new Client
        {
            ClientName = "Shopping Web App Interactive Client",
            ClientId = "shopping_web_client_interactive",
            ClientSecrets =
            { 
                // Retrieve and hash the client secret from appsettings.json
                new Secret(configuration["ClientSecrets:ShoppingWebClientSecret"].Sha256())
            },
            AllowedGrantTypes = GrantTypes.Code, // Using the Authorization Code flow
            RedirectUris = { $"{razorClientUri}/signin-oidc" }, // Redirect URI after login
            PostLogoutRedirectUris = { $"{razorClientUri}/signout-callback-oidc" }, // Redirect URI after logout
            AllowedScopes = { "openid", "profile", "shoppingbasket.fullaccess" } // Scopes for this client
        },

        // Client for Razor web application (OAuth2 hybrid flow allowing both Authorization Code and Client Credentials)
        new Client
        {
            ClientName = "Shopping Razor Client",
            ClientId = "shopping_web_client",
            ClientUri = razorClientUri, // URI of the Razor client application
            AllowedGrantTypes = GrantTypes.CodeAndClientCredentials, // Hybrid flow
            AllowAccessTokensViaBrowser = false, // Tokens won't be passed in the browser
            AllowOfflineAccess = true, // Allow refresh tokens
            AlwaysIncludeUserClaimsInIdToken = true, // Include user claims in ID token
            RedirectUris = new List<string>
            {
                // Redirect URI for the Razor application after login
                $"{razorClientUri}/signin-oidc"
            },
            PostLogoutRedirectUris = new List<string>
            {
                // Redirect URI after logout
                $"{razorClientUri}/signout-callback-oidc"
            },
            AllowedScopes =
            {
                // Allowing standard scopes (OpenID, Profile, etc.) and custom scopes for the Razor client
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.Address,
                IdentityServerConstants.StandardScopes.OfflineAccess,
                "roles",
                "shoppinggateway.fullaccess",
                "shoppingaggregator.fullaccess"
            },
            ClientSecrets =
            {
                // Retrieve and hash the secret for the Razor client
                new Secret(configuration["ClientSecrets:ShoppingRazorClientSecret"].Sha256())
            }
        },

        // Client for downstream service token exchange (client credentials flow for service-to-service communication)
        new Client
        {
            ClientId = "downstreamservicestokenexchangeclient",
            ClientName = "Downstream Services Token Exchange Client",
            AllowedGrantTypes = new[] { "urn:ietf:params:oauth:grant-type:token-exchange" }, // Token exchange grant type
            ClientSecrets = { new Secret(configuration["ClientSecrets:DownstreamServiceClientSecret"].Sha256()) }, // Client secret for token exchange
            AllowedScopes =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                "discount.fullaccess",
                "ordersagaorchestrator.fullaccess",
                "catalogapi.fullaccess" //I have added 
            }
        },

        // Client for token exchange between gateway/aggregator and downstream services
        new Client
        {
            ClientId = "gatewayandaggregatortodownstreamtokenexchangeclient",
            ClientName = "Gateway And Aggregator To Downstream Token Exchange Client",
            AllowedGrantTypes = new[] { "urn:ietf:params:oauth:grant-type:token-exchange" }, // Token exchange grant type
            ClientSecrets = { new Secret(configuration["ClientSecrets:GatewayAggregatorSecret"].Sha256()) }, // Client secret for token exchange
            AllowedScopes =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                "catalogapi.fullaccess",
                "basketapi.fullaccess",
                "orderapi.fullaccess"
            }
        },

        // Client for token exchange for Order Saga Orchestrator
        new Client
        {
            ClientId = "ordersagaorchestratortokenexchangeclient",
            ClientName = "Order Saga Orchestrator Token Exchange Client",
            AllowedGrantTypes = new[] { "urn:ietf:params:oauth:grant-type:token-exchange" }, // Token exchange grant type
            ClientSecrets = { new Secret(configuration["ClientSecrets:OrderSagaOrchestratorSecret"].Sha256()) }, // Client secret for token exchange
            AllowedScopes =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                "orderapi.fullaccess",
                "paymentapi.fullaccess",
                "deliveryapi.fullaccess"
            }
        },

        // Angular client with OAuth2 Authorization Code flow and support for offline access and PKCE
        new Client
        {
            ClientName = "Shopping Angular Client",
            ClientId = "angular-client",
            AllowedGrantTypes = GrantTypes.Code,
            RedirectUris = new List<string>
            {
                $"{angularClientUri}/home"
                // $"{angularClientUri}/silent-refresh.html"
            },
            RequirePkce = true,
            AllowAccessTokensViaBrowser = true, // Allow access tokens to be passed via the browser
            AllowOfflineAccess = true, // Allow refresh tokens
            AlwaysIncludeUserClaimsInIdToken = true, // Always include user claims in ID token
            AllowedScopes =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.Address,
                IdentityServerConstants.StandardScopes.OfflineAccess,
                "roles",
                "shoppinggateway.fullaccess",
                "shoppingaggregator.fullaccess",
                "catalogapi.read"  //I have added 
            },
            AllowedCorsOrigins = { $"{angularClientUri}" }, // CORS allowed origins for Angular client
            RequireClientSecret = false, // No client secret required for Angular client
            PostLogoutRedirectUris = new List<string> { $"{angularClientUri}/home" }, // Redirect URI after logout
            RequireConsent = false, // No consent required for this client
            AccessTokenLifetime = 300 // Access token lifetime (in seconds)
        },

        //// IdentityServer4 Configuration (Enable ROP Flow)
          new Client
        {
            ClientId = "angular-client-password",
            ClientName = "Shopping Angular Client (Password Grant)",
            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
            AllowOfflineAccess = true, // ✅ Enables refresh tokens for "Remember Me"
            RequireClientSecret = false,
            AllowedScopes =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.OfflineAccess,
                "email",
                "roles",
                "catalogapi.read"
            },
            AccessTokenLifetime = 3600, // ⏳ 1 hour access token
            RefreshTokenUsage = TokenUsage.ReUse, // ✅ Allows using the same refresh token
            RefreshTokenExpiration = TokenExpiration.Sliding, // ✅ Extends refresh token validity on use
            AbsoluteRefreshTokenLifetime = jwtSettings.RememberMeTokenExpirationDays * 86400, // Convert days to seconds
            SlidingRefreshTokenLifetime = jwtSettings.TokenExpirationHours * 3600, // Convert hours to seconds
            AllowedCorsOrigins = { $"{angularClientUri}" },
            RequireConsent = false,
            AlwaysIncludeUserClaimsInIdToken = true, // 🔹 Ensure claims are included
        },

        // Swagger UI clients for various APIs (catalog, basket, order, etc.)
        new Client
        {
            ClientId = "catalogswaggerui",
            ClientName = "Catalog Swagger UI",
            AllowedGrantTypes = GrantTypes.Implicit, // Implicit flow for Swagger UI
            AllowAccessTokensViaBrowser = true,
            RedirectUris = { $"{catalogApiPublicUrl}/swagger/oauth2-redirect.html" },
            PostLogoutRedirectUris = { $"{catalogApiPublicUrl}/swagger/" },
            AllowedScopes =
            {
                "catalogapi.fullaccess"
            },
             AlwaysIncludeUserClaimsInIdToken = true, // 🔹 Ensure claims are included
        },

//        new Client
//{
//                ClientId = "catalogswaggerui",
//    ClientName = "Catalog Swagger UI",
//    AllowedGrantTypes = GrantTypes.Implicit,  // 🔹 Use Implicit Flow
//    AllowAccessTokensViaBrowser = true,       // 🔹 Required for browser-based apps like Swagger UI

//    RedirectUris = { "http://localhost:5002/swagger/oauth2-redirect.html" },
//    PostLogoutRedirectUris = { "http://localhost:5002/swagger/" },
//    AllowedCorsOrigins = { "http://localhost:5002" },

//    AllowedScopes = { "catalogapi.fullaccess" },
//    RequirePkce = false, // 🔹 PKCE is not needed for Implicit Flow
//    RequireClientSecret = false,  // 🔹 No client secret needed for Implicit Flow


//    //ClientId = "catalogswaggerui",
//    //ClientName = "Catalog Swagger UI",
//    //AllowedGrantTypes = GrantTypes.Code, // 🔹 Use Authorization Code Flow with PKCE
//    //RequireClientSecret = false, // 🔹 No client secret needed for public clients like Swagger UI
//    //RedirectUris = { $"{catalogApiUrl}/swagger/oauth2-redirect.html" },
//    //PostLogoutRedirectUris = { $"{catalogApiUrl}/swagger/" },
//    //AllowedCorsOrigins = { catalogApiUrl }, // 🔹 Allow CORS for Swagger UI
//    //AllowedScopes = { "catalogapi.fullaccess" },
//    //AllowAccessTokensViaBrowser = true, // 🔹 Allow access tokens in the browser
//    //RequirePkce = true, // 🔹 Enforce PKCE (causing the issue now)

    
            
//    //        ClientId = "catalogswaggerui",
//    //ClientName = "Catalog Swagger UI",

//    //AllowedGrantTypes = GrantTypes.Code, // ✅ Use Authorization Code Flow
//    //RequireClientSecret = false,         // ✅ Remove client secret requirement (PKCE)
//    //RequirePkce = true,                  // ✅ Enable PKCE for security

//    //AllowAccessTokensViaBrowser = true, // Required for Swagger UI
//    //RedirectUris = { $"{catalogApiUrl}/swagger/oauth2-redirect.html" },
//    //PostLogoutRedirectUris = { $"{catalogApiUrl}/swagger/" },

//    //AllowedScopes = { "catalogapi.fullaccess" },

//    //AlwaysIncludeUserClaimsInIdToken = true // Ensure user claims are available
//},

        new Client
        {
            ClientId = "basketswaggerui",
            ClientName = "Basket Swagger UI",
            AllowedGrantTypes = GrantTypes.Implicit, // Implicit flow for Swagger UI
            AllowAccessTokensViaBrowser = true,
            RedirectUris = { $"{basketApiUrl}/swagger/oauth2-redirect.html" },
            PostLogoutRedirectUris = { $"{basketApiUrl}/swagger/" },
            AllowedScopes =
            {
                "basketapi.fullaccess"
            }
        },
        new Client
        {
            ClientId = "orderingswaggerui",
            ClientName = "Order Swagger UI",
            AllowedGrantTypes = GrantTypes.Implicit, // Implicit flow for Swagger UI
            AllowAccessTokensViaBrowser = true,
            RedirectUris = { $"{orderApiUrl}/swagger/oauth2-redirect.html" },
            PostLogoutRedirectUris = { $"{orderApiUrl}/swagger/" },
            AllowedScopes =
            {
                "orderapi.fullaccess"
            }
        },
        new Client
        {
            ClientId = "paymentswaggerui",
            ClientName = "Payment Swagger UI",
            AllowedGrantTypes = GrantTypes.Implicit, // Implicit flow for Swagger UI
            AllowAccessTokensViaBrowser = true,
            RedirectUris = { $"{paymentApiUrl}/swagger/oauth2-redirect.html" },
            PostLogoutRedirectUris = { $"{paymentApiUrl}/swagger/" },
            AllowedScopes =
            {
                "paymentapi.fullaccess"
            }
        },
        new Client
        {
            ClientId = "deliveryswaggerui",
            ClientName = "Delivery Swagger UI",
            AllowedGrantTypes = GrantTypes.Implicit, // Implicit flow for Swagger UI
            AllowAccessTokensViaBrowser = true,
            RedirectUris = { $"{deliveryApiUrl}/swagger/oauth2-redirect.html" },
            PostLogoutRedirectUris = { $"{deliveryApiUrl}/swagger/" },
            AllowedScopes =
            {
                "deliveryapi.fullaccess"
            }
        },
        new Client
        {
            ClientId = "shoppingaggregatorswaggerui",
            ClientName = "Shopping Aggregator Swagger UI",
            AllowedGrantTypes = GrantTypes.Implicit, // Implicit flow for Swagger UI
            AllowAccessTokensViaBrowser = true,
            RedirectUris = { $"{shoppingAggregatorApiUrl}/swagger/oauth2-redirect.html" },
            PostLogoutRedirectUris = { $"{shoppingAggregatorApiUrl}/swagger/" },
            AllowedScopes =
            {
                "shoppingaggregator.fullaccess"
            }
        },
        new Client  //test 
        {
            ClientName = "Shopping Resource Owner Password Client",
            ClientId = "shopping_password_client",
            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword, // Resource owner password flow
            ClientSecrets =
            {
                new Secret(configuration["ClientSecrets:ShoppingPasswordClientSecret"].Sha256()) // Reference secret from appsettings.json
            },
            AllowedScopes = { "openid", "profile", "roles", "catalogapi.fullaccess" },
            AllowOfflineAccess = true,
        },
        new Client //test
        {
            ClientId = "node-client",
            ClientName = "Node Client",
            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword, // ✅ Ensure ROPC is enabled
            ClientSecrets = { new Secret("NodeClientSecret".Sha256()) }, // ✅ Ensure secret is hashed
            //AllowedScopes = { "openid", "profile", "roles", "offline_access", "shoppinggateway.fullaccess", "catalogapi.fullaccess" },
            AllowedScopes = { "openid", "profile", "roles", "offline_access", "shoppinggateway.fullaccess", "catalogapi.read" },

            AllowOfflineAccess = true, // ✅ Enable refresh tokens (important for remember_me)
            RequireClientSecret = true, // ✅ Required if using client secret
            RefreshTokenExpiration = TokenExpiration.Sliding,
            AbsoluteRefreshTokenLifetime = jwtSettings.RememberMeTokenExpirationDays * 86400, // Convert days to seconds
            SlidingRefreshTokenLifetime = jwtSettings.TokenExpirationHours * 3600, // Convert hours to seconds
            AlwaysIncludeUserClaimsInIdToken = true,
        }
            };
        }


        //        public static IEnumerable<Client> Clients(IConfiguration configuration)
        //        {

        //            var test = configuration["WebClientUrls:Razor"];
        //            // Get JWT settings from configuration
        //            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

        //            // Read values from environment variables (flat structure)
        //            var razorClientUri = Environment.GetEnvironmentVariable("WebClientUrls__Razor") ??
        //                                 configuration.GetValue<string>("WebClientUrls:Razor") ??
        //                                 "https://localhost:4999"; // Default value

        //            var angularClientUri = Environment.GetEnvironmentVariable("WebClientUrls__Angular") ??
        //                                   configuration.GetValue<string>("WebClientUrls:Angular") ??
        //                                   "http://localhost:4200"; // Default value

        //            var blazorClientUri = Environment.GetEnvironmentVariable("WebClientUrls__Blazor") ??
        //                                  configuration.GetValue<string>("WebClientUrls:Blazor") ??
        //                                  "https://localhost:5002"; // Default value

        //            var catalogApiUrl = Environment.GetEnvironmentVariable("ApiUrls__Catalog") ??
        //                                configuration.GetValue<string>("ApiUrls:Catalog") ??
        //                                "http://localhost:8000"; // Default value

        //            var basketApiUrl = Environment.GetEnvironmentVariable("ApiUrls__Basket") ??
        //                               configuration.GetValue<string>("ApiUrls:Basket") ??
        //                               "http://localhost:5001"; // Default value

        //            var orderApiUrl = Environment.GetEnvironmentVariable("ApiUrls__Order") ??
        //                              configuration.GetValue<string>("ApiUrls:Order") ??
        //                              "http://localhost:5004"; // Default value

        //            var paymentApiUrl = Environment.GetEnvironmentVariable("ApiUrls__Payment") ??
        //                                configuration.GetValue<string>("ApiUrls:Payment") ??
        //                                "http://localhost:5009"; // Default value

        //            var deliveryApiUrl = Environment.GetEnvironmentVariable("ApiUrls__Delivery") ??
        //                                 configuration.GetValue<string>("ApiUrls:Delivery") ??
        //                                 "http://localhost:5010"; // Default value

        //            var shoppingAggregatorApiUrl = Environment.GetEnvironmentVariable("ApiUrls__ShoppingAggregator") ??
        //                                           configuration.GetValue<string>("ApiUrls:ShoppingAggregator") ??
        //                                           "http://localhost:5005"; // Default value

        //            var nodeApiUrl = Environment.GetEnvironmentVariable("ApiUrls__Node") ??
        //                             configuration.GetValue<string>("ApiUrls:Node") ??
        //                             "http://localhost:3000"; // Default value

        //            // Retrieve client secrets from configuration
        //            var shoppingM2MSecret = configuration.GetValue<string>("ClientSecrets:ShoppingMachineClientSecret")?.Sha256() ?? string.Empty;
        //            var razorClientSecret = configuration.GetValue<string>("ClientSecrets:ShoppingRazorClientSecret")?.Sha256() ?? string.Empty;
        //            var blazorClientSecret = configuration.GetValue<string>("ClientSecrets:ShoppingBlazorClientSecret")?.Sha256() ?? string.Empty;

        //            // Token expiration settings
        //            var absoluteRefreshTokenLifetime = jwtSettings.RememberMeTokenExpirationDays * 86400;
        //            var slidingRefreshTokenLifetime = jwtSettings.TokenExpirationHours * 3600;


        //            return new Client[]
        //            { 
        //        // Client for machine to machine authentication (no UI interaction, client credentials flow)
        //        new Client
        //        {
        //            ClientName = "Shopping Machine 2 Machine Client",
        //            ClientId = "shoppingm2m",
        //            ClientSecrets =
        //            {
        //                // Retrieve the secret from appsettings.json and hash it
        //                new Secret(configuration["ClientSecrets:ShoppingMachineClientSecret"].Sha256())
        //            },
        //            AllowedGrantTypes = GrantTypes.ClientCredentials,
        //            AllowedScopes =
        //            {
        //                // Allowed scope for the client
        //                "catalogapi.fullaccess"
        //            }
        //        },

        //        // Client for web app with interactive login (OAuth2 Authorization Code flow)
        //        new Client
        //        {
        //            ClientName = "Shopping Web App Interactive Client",
        //            ClientId = "shopping_web_client_interactive",
        //            ClientSecrets =
        //            { 
        //                // Retrieve and hash the client secret from appsettings.json
        //                new Secret(configuration["ClientSecrets:ShoppingWebClientSecret"].Sha256())
        //            },
        //            AllowedGrantTypes = GrantTypes.Code, // Using the Authorization Code flow
        //            RedirectUris = { "https://localhost:4999/signin-oidc" }, // Redirect URI after login
        //            PostLogoutRedirectUris = { "https://localhost:4999/signout-callback-oidc" }, // Redirect URI after logout
        //            AllowedScopes = { "openid", "profile", "shoppingbasket.fullaccess" } // Scopes for this client
        //        },

        //        // Client for Razor web application (OAuth2 hybrid flow allowing both Authorization Code and Client Credentials)
        //        new Client
        //        {
        //            ClientName = "Shopping Razor Client",
        //            ClientId = "shopping_web_client",
        //            ClientUri = razorClientUri,//configuration["WebClientUrls:Razor"], // URI of the Razor client application
        //            AllowedGrantTypes = GrantTypes.CodeAndClientCredentials, // Hybrid flow
        //            AllowAccessTokensViaBrowser = false, // Tokens won't be passed in the browser
        //            AllowOfflineAccess = true, // Allow refresh tokens
        //            AlwaysIncludeUserClaimsInIdToken = true, // Include user claims in ID token
        //            RedirectUris = new List<string>
        //            {
        //                // Redirect URI for the Razor application after login
        //                //$"{configuration["WebClientUrls:Razor"]}/signin-oidc"
        //                 $"{razorClientUri}/signin-oidc"
        //            },
        //            PostLogoutRedirectUris = new List<string>
        //            {
        //                // Redirect URI after logout
        //                //$"{configuration["WebClientUrls:Razor"]}/signout-callback-oidc"
        //                 $"{razorClientUri}/signout-callback-oidc"
        //            },
        //            AllowedScopes =
        //            {
        //                // Allowing standard scopes (OpenID, Profile, etc.) and custom scopes for the Razor client
        //                IdentityServerConstants.StandardScopes.OpenId,
        //                IdentityServerConstants.StandardScopes.Profile,
        //                IdentityServerConstants.StandardScopes.Address,
        //                IdentityServerConstants.StandardScopes.OfflineAccess,
        //                "roles",
        //                "shoppinggateway.fullaccess",
        //                "shoppingaggregator.fullaccess"
        //            },
        //            ClientSecrets =
        //            {
        //                // Retrieve and hash the secret for the Razor client
        //                new Secret(configuration["ClientSecrets:ShoppingRazorClientSecret"].Sha256())
        //            }
        //        },

        //        // Client for downstream service token exchange (client credentials flow for service-to-service communication)
        //        new Client
        //        {
        //            ClientId = "downstreamservicestokenexchangeclient",
        //            ClientName = "Downstream Services Token Exchange Client",
        //            AllowedGrantTypes = new[] { "urn:ietf:params:oauth:grant-type:token-exchange" }, // Token exchange grant type
        //            ClientSecrets = { new Secret(configuration["ClientSecrets:DownstreamServiceClientSecret"].Sha256()) }, // Client secret for token exchange
        //            AllowedScopes =
        //            {
        //                IdentityServerConstants.StandardScopes.OpenId,
        //                IdentityServerConstants.StandardScopes.Profile,
        //                "discount.fullaccess",
        //                "ordersagaorchestrator.fullaccess",
        //                 "catalogapi.fullaccess" //I have added 
        //            }
        //        },

        //        // Client for token exchange between gateway/aggregator and downstream services
        //        new Client
        //        {
        //            ClientId = "gatewayandaggregatortodownstreamtokenexchangeclient",
        //            ClientName = "Gateway And Aggregator To Downstream Token Exchange Client",
        //            AllowedGrantTypes = new[] { "urn:ietf:params:oauth:grant-type:token-exchange" }, // Token exchange grant type
        //            ClientSecrets = { new Secret(configuration["ClientSecrets:GatewayAggregatorSecret"].Sha256()) }, // Client secret for token exchange
        //            AllowedScopes =
        //            {
        //                IdentityServerConstants.StandardScopes.OpenId,
        //                IdentityServerConstants.StandardScopes.Profile,
        //                "catalogapi.fullaccess",
        //                "basketapi.fullaccess",
        //                "orderapi.fullaccess"
        //            }
        //        },

        //        // Client for token exchange for Order Saga Orchestrator
        //        new Client
        //        {
        //            ClientId = "ordersagaorchestratortokenexchangeclient",
        //            ClientName = "Order Saga Orchestrator Token Exchange Client",
        //            AllowedGrantTypes = new[] { "urn:ietf:params:oauth:grant-type:token-exchange" }, // Token exchange grant type
        //            ClientSecrets = { new Secret(configuration["ClientSecrets:OrderSagaOrchestratorSecret"].Sha256()) }, // Client secret for token exchange
        //            AllowedScopes =
        //            {
        //                IdentityServerConstants.StandardScopes.OpenId,
        //                IdentityServerConstants.StandardScopes.Profile,
        //                "orderapi.fullaccess",
        //                "paymentapi.fullaccess",
        //                "deliveryapi.fullaccess"
        //            }
        //        },

        //    //    // Angular client with OAuth2 Authorization Code flow and support for offline access and PKCE
        //        new Client
        //        {
        //            ClientName = "Shopping Angular Client",
        //            ClientId = "angular-client",
        //                    AllowedGrantTypes = GrantTypes.Code,
        //                    RedirectUris = new List<string>
        //                    {
        //                        $"{configuration["WebClientUrls:Angular"] }/home"
        //                        //$"{configuration["WebClientUrls:Angular"]}/silent-refresh.html"
        //                    },
        //                    RequirePkce = true,
        //            AllowAccessTokensViaBrowser = true, // Allow access tokens to be passed via the browser
        //            AllowOfflineAccess = true, // Allow refresh tokens
        //            AlwaysIncludeUserClaimsInIdToken = true, // Always include user claims in ID token
        //            AllowedScopes =
        //            {
        //                IdentityServerConstants.StandardScopes.OpenId,
        //                IdentityServerConstants.StandardScopes.Profile,
        //                IdentityServerConstants.StandardScopes.Address,
        //                IdentityServerConstants.StandardScopes.OfflineAccess,
        //                "roles",
        //                "shoppinggateway.fullaccess",
        //                "shoppingaggregator.fullaccess",
        //                "catalogapi.fullaccess"  //I have added 
        //            },
        //            AllowedCorsOrigins = { $"{configuration["WebClientUrls:Angular"]}" }, // CORS allowed origins for Angular client


        //            RequireClientSecret = false, // No client secret required for Angular client

        //            PostLogoutRedirectUris = new List<string> { $"{configuration["WebClientUrls:Angular"]}/home" }, // Redirect URI after logout
        //            RequireConsent = false, // No consent required for this client
        //            AccessTokenLifetime = 300 // Access token lifetime (in seconds)
        //        },

        //        // Swagger UI clients for various APIs (catalog, basket, order, etc.)
        //        new Client
        //        {
        //            ClientId = "catalogswaggerui",
        //            ClientName = "Catalog Swagger UI",
        //            AllowedGrantTypes = GrantTypes.Implicit, // Implicit flow for Swagger UI
        //            AllowAccessTokensViaBrowser = true,
        //            RedirectUris = { $"{configuration["WebClientUrls:CatalogApi"]}/swagger/oauth2-redirect.html" },
        //            PostLogoutRedirectUris = { $"{configuration["WebClientUrls:CatalogApi"]}/swagger/" },
        //            AllowedScopes =
        //            {
        //                "catalogapi.fullaccess"
        //            }
        //        },
        //        new Client
        //        {
        //            ClientId = "basketswaggerui",
        //            ClientName = "Basket Swagger UI",
        //            AllowedGrantTypes = GrantTypes.Implicit, // Implicit flow for Swagger UI
        //            AllowAccessTokensViaBrowser = true,
        //            RedirectUris = { $"{configuration["WebClientUrls:BasketApi"]}/swagger/oauth2-redirect.html" },
        //            PostLogoutRedirectUris = { $"{configuration["WebClientUrls:BasketApi"]}/swagger/" },
        //            AllowedScopes =
        //            {
        //                "basketapi.fullaccess"
        //            }
        //        },
        //        new Client
        //        {
        //            ClientId = "orderingswaggerui",
        //            ClientName = "Order Swagger UI",
        //            AllowedGrantTypes = GrantTypes.Implicit, // Implicit flow for Swagger UI
        //            AllowAccessTokensViaBrowser = true,
        //            RedirectUris = { $"{configuration["WebClientUrls:OrderApi"]}/swagger/oauth2-redirect.html" },
        //            PostLogoutRedirectUris = { $"{configuration["WebClientUrls:OrderApi"]}/swagger/" },
        //            AllowedScopes =
        //            {
        //                "orderapi.fullaccess"
        //            }
        //        },
        //        new Client
        //        {
        //            ClientId = "paymentswaggerui",
        //            ClientName = "Payment Swagger UI",
        //            AllowedGrantTypes = GrantTypes.Implicit, // Implicit flow for Swagger UI
        //            AllowAccessTokensViaBrowser = true,
        //            RedirectUris = { $"{configuration["WebClientUrls:PaymentApi"]}/swagger/oauth2-redirect.html" },
        //            PostLogoutRedirectUris = { $"{configuration["WebClientUrls:PaymentApi"]}/swagger/" },
        //            AllowedScopes =
        //            {
        //                "paymentapi.fullaccess"
        //            }
        //        },
        //        new Client
        //        {
        //            ClientId = "deliveryswaggerui",
        //            ClientName = "Delivery Swagger UI",
        //            AllowedGrantTypes = GrantTypes.Implicit, // Implicit flow for Swagger UI
        //            AllowAccessTokensViaBrowser = true,
        //            RedirectUris = { $"{configuration["WebClientUrls:DeliveryApi"]}/swagger/oauth2-redirect.html" },
        //            PostLogoutRedirectUris = { $"{configuration["WebClientUrls:DeliveryApi"]}/swagger/" },
        //            AllowedScopes =
        //            {
        //                "deliveryapi.fullaccess"
        //            }
        //        },
        //        new Client
        //        {
        //            ClientId = "shoppingaggregatorswaggerui",
        //            ClientName = "Shopping Aggregator Swagger UI",
        //            AllowedGrantTypes = GrantTypes.Implicit, // Implicit flow for Swagger UI
        //            AllowAccessTokensViaBrowser = true,
        //            RedirectUris = { $"{configuration["WebClientUrls:ShoppingAggregatorApi"]}/swagger/oauth2-redirect.html" },
        //            PostLogoutRedirectUris = { $"{configuration["WebClientUrls:ShoppingAggregatorApi"]}/swagger/" },
        //            AllowedScopes =
        //            {
        //                "shoppingaggregator.fullaccess"
        //            }
        //        },
        //                new Client  //test 
        //        {
        //            ClientName = "Shopping Resource Owner Password Client",
        //            ClientId = "shopping_password_client",
        //            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword, // Resource owner password flow
        //            ClientSecrets =
        //            {
        //                new Secret(configuration["ClientSecrets:ShoppingPasswordClientSecret"].Sha256()) // Reference secret from appsettings.json
        //            },
        //            AllowedScopes = { "openid", "profile", "roles", "catalogapi.fullaccess" },
        //            AllowOfflineAccess = true,

        //        },

        //                new Client //test
        //{
        //    ClientId = "node-client",
        //    ClientName = "Node Client",

        //    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword, // ✅ Ensure ROPC is enabled
        //    ClientSecrets = { new Secret("NodeClientSecret".Sha256()) }, // ✅ Ensure secret is hashed
        //    AllowedScopes = { "openid", "profile", "roles", "offline_access", "shoppinggateway.fullaccess", "catalogapi.fullaccess" },
        //    AllowOfflineAccess = true, // ✅ Enable refresh tokens (important for remember_me)
        //    RequireClientSecret = true, // ✅ Required if using client secret
        //        RefreshTokenExpiration = TokenExpiration.Sliding,
        //        AbsoluteRefreshTokenLifetime = jwtSettings.RememberMeTokenExpirationDays * 86400, // Convert days to seconds
        //        SlidingRefreshTokenLifetime = jwtSettings.TokenExpirationHours * 3600, // Convert hours to seconds
        //}
        //            };
        //        }

    }
}

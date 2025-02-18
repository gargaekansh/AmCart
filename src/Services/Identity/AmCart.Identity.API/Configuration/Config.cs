using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

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
            };

        // This will set 'aud' value in access token which is used for authentication on API
        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
                new ApiResource("catalogapi", "Catalog API")
                {
                    Scopes = { "catalogapi.read", "catalogapi.fullaccess" },
                    UserClaims = new List<string> { "role" }
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
            var jwtSettings = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);

            var AbsoluteRefreshTokenLifetime = jwtSettings.RememberMeTokenExpirationDays * 86400; // Convert days to seconds
           var SlidingRefreshTokenLifetime = jwtSettings.TokenExpirationHours * 3600;

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
            RedirectUris = { "https://localhost:4999/signin-oidc" }, // Redirect URI after login
            PostLogoutRedirectUris = { "https://localhost:4999/signout-callback-oidc" }, // Redirect URI after logout
            AllowedScopes = { "openid", "profile", "shoppingbasket.fullaccess" } // Scopes for this client
        },

        // Client for Razor web application (OAuth2 hybrid flow allowing both Authorization Code and Client Credentials)
        new Client
        {
            ClientName = "Shopping Razor Client",
            ClientId = "shopping_web_client",
            ClientUri = configuration["WebClientUrls:Razor"], // URI of the Razor client application
            AllowedGrantTypes = GrantTypes.CodeAndClientCredentials, // Hybrid flow
            AllowAccessTokensViaBrowser = false, // Tokens won't be passed in the browser
            AllowOfflineAccess = true, // Allow refresh tokens
            AlwaysIncludeUserClaimsInIdToken = true, // Include user claims in ID token
            RedirectUris = new List<string>
            {
                // Redirect URI for the Razor application after login
                $"{configuration["WebClientUrls:Razor"]}/signin-oidc"
            },
            PostLogoutRedirectUris = new List<string>
            {
                // Redirect URI after logout
                $"{configuration["WebClientUrls:Razor"]}/signout-callback-oidc"
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

    //    // Angular client with OAuth2 Authorization Code flow and support for offline access and PKCE
        new Client
        {
            ClientName = "Shopping Angular Client",
            ClientId = "angular-client",
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = new List<string>
                    {
                        $"{configuration["WebClientUrls:Angular"] }/home"
                        //$"{configuration["WebClientUrls:Angular"]}/silent-refresh.html"
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
                "catalogapi.fullaccess"  //I have added 
            },
            AllowedCorsOrigins = { $"{configuration["WebClientUrls:Angular"]}" }, // CORS allowed origins for Angular client


            RequireClientSecret = false, // No client secret required for Angular client
          
            PostLogoutRedirectUris = new List<string> { $"{configuration["WebClientUrls:Angular"]}/home" }, // Redirect URI after logout
            RequireConsent = false, // No consent required for this client
            AccessTokenLifetime = 300 // Access token lifetime (in seconds)
        },

        // Swagger UI clients for various APIs (catalog, basket, order, etc.)
        new Client
        {
            ClientId = "catalogswaggerui",
            ClientName = "Catalog Swagger UI",
            AllowedGrantTypes = GrantTypes.Implicit, // Implicit flow for Swagger UI
            AllowAccessTokensViaBrowser = true,
            RedirectUris = { $"{configuration["WebClientUrls:CatalogApi"]}/swagger/oauth2-redirect.html" },
            PostLogoutRedirectUris = { $"{configuration["WebClientUrls:CatalogApi"]}/swagger/" },
            AllowedScopes =
            {
                "catalogapi.fullaccess"
            }
        },
        new Client
        {
            ClientId = "basketswaggerui",
            ClientName = "Basket Swagger UI",
            AllowedGrantTypes = GrantTypes.Implicit, // Implicit flow for Swagger UI
            AllowAccessTokensViaBrowser = true,
            RedirectUris = { $"{configuration["WebClientUrls:BasketApi"]}/swagger/oauth2-redirect.html" },
            PostLogoutRedirectUris = { $"{configuration["WebClientUrls:BasketApi"]}/swagger/" },
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
            RedirectUris = { $"{configuration["WebClientUrls:OrderApi"]}/swagger/oauth2-redirect.html" },
            PostLogoutRedirectUris = { $"{configuration["WebClientUrls:OrderApi"]}/swagger/" },
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
            RedirectUris = { $"{configuration["WebClientUrls:PaymentApi"]}/swagger/oauth2-redirect.html" },
            PostLogoutRedirectUris = { $"{configuration["WebClientUrls:PaymentApi"]}/swagger/" },
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
            RedirectUris = { $"{configuration["WebClientUrls:DeliveryApi"]}/swagger/oauth2-redirect.html" },
            PostLogoutRedirectUris = { $"{configuration["WebClientUrls:DeliveryApi"]}/swagger/" },
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
            RedirectUris = { $"{configuration["WebClientUrls:ShoppingAggregatorApi"]}/swagger/oauth2-redirect.html" },
            PostLogoutRedirectUris = { $"{configuration["WebClientUrls:ShoppingAggregatorApi"]}/swagger/" },
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
    AllowedScopes = { "openid", "profile", "roles", "offline_access", "shoppinggateway.fullaccess", "catalogapi.fullaccess" },
    AllowOfflineAccess = true, // ✅ Enable refresh tokens (important for remember_me)
    RequireClientSecret = true, // ✅ Required if using client secret
        RefreshTokenExpiration = TokenExpiration.Sliding,
        AbsoluteRefreshTokenLifetime = jwtSettings.RememberMeTokenExpirationDays * 86400, // Convert days to seconds
        SlidingRefreshTokenLifetime = jwtSettings.TokenExpirationHours * 3600, // Convert hours to seconds
}
            };
        }

    }
}

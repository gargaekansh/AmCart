using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AmCart.Identity.API.Models;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using IdentityServer4;
using System.Linq;
using AmCart.Identity.API.Configuration;
using IdentityServer4.Models;
using System.Security.Claims;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using IdentityModel;
using IdentityServer4.Validation;
using System.IdentityModel.Tokens.Jwt;
using IdentityServer4.Configuration;
using AmCart.Identity.API.Services;
using System.Collections.Specialized;
using System;
using System.Collections.Specialized; // Add this for NameValueCollection

namespace AmCart.Identity.API.Controllers
{
    /// <summary>
    /// Controller for user authentication and registration.
    /// </summary>
    [Route("api/auth2")]
    [ApiController]
    public class AuthController2 : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthController2> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly JwtSettings _jwtSettings;
        private readonly ITokenService _tokenService;

        /// <summary>
        /// Constructor for AuthController.
        /// </summary>
        public AuthController2(UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager,
                              ILogger<AuthController2> logger,
                              IIdentityServerInteractionService interaction,
                              IClientStore clientStore,
                              IAuthenticationSchemeProvider schemeProvider,
                              IEventService events,
                              IOptions<JwtSettings> jwtSettings,
                              ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _jwtSettings = jwtSettings.Value; // Access the settings from IOptions
            _tokenService = tokenService;
        }


        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] ExtendedRegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Gender = model.Gender,
                MobileNumber = model.MobileNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User '{Email}' registered successfully.", model.Email);

                var roleResult = await _userManager.AddToRoleAsync(user, "User"); // Or your role name
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Failed to add user '{Email}' to role. Errors: {Errors}", model.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    await _userManager.DeleteAsync(user); // Rollback
                    return BadRequest(new { errors = new[] { "Failed to assign role. Please contact support." } });
                }

                return CreatedAtAction(nameof(GetUserProfile), new { email = user.Email }, new
                {
                    message = "User registered successfully.",
                    user = new { user.Id, user.Email, user.Gender, user.MobileNumber }
                });
            }
            else
            {
                _logger.LogWarning("User registration failed for {Email}. Errors: {Errors}", model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }
        }


        /// <summary>
        /// Logs in a user.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] ExtendedLoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed. Email '{Email}' not found.", model.Email);
                return Unauthorized("Invalid email or password.");
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return await HandleSuccessfulLogin(user, model.ReturnUrl, model.RememberMe, context);
            }
            else
            {
                _logger.LogWarning("Login failed for '{Email}'. Incorrect credentials.", model.Email);
                return Unauthorized("Invalid email or password.");
            }

        }


        // Example GetUser action (for CreatedAtAction)
        [HttpGet("user/{email}", Name = "GetUserProfile")] // Named route for CreatedAtAction
        public async Task<IActionResult> GetUserProfile(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(new { user.Id, user.Email, user.Gender, user.MobileNumber });
        }






        private async Task<IActionResult> HandleSuccessfulLogin(ApplicationUser user, string? returnUrl, bool rememberMe, AuthorizationRequest? context)
        {
            // Sign in the user with Remember Me
            await _signInManager.SignInAsync(user, rememberMe);

            if (context == null)
            {
                return BadRequest("Invalid authorization request.");
            }

            // Retrieve the client configuration from IdentityServer
            var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
            if (client == null)
            {
                return BadRequest("Invalid client configuration.");
            }

            // Create claims for the authenticated user
            var claims = new List<Claim>
    {
        new Claim(JwtClaimTypes.Subject, user.Id),
        new Claim(JwtClaimTypes.Name, user.UserName),
        new Claim(JwtClaimTypes.Email, user.Email),
    };

            // Add roles if available
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            var identity = new ClaimsIdentity(claims, IdentityServerConstants.DefaultCookieAuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // ✅ Step 1: Get Token Expiration from Configuration
            var tokenExpiration = rememberMe
                ? TimeSpan.FromDays(_jwtSettings.RememberMeTokenExpirationDays) // Longer token lifetime
                : TimeSpan.FromHours(_jwtSettings.TokenExpirationHours); // Shorter token lifetime

            var tokenRequest = new TokenCreationRequest
            {
                Subject = principal,
                ValidatedRequest = new ValidatedRequest
                {
                    Client = client,
                    Raw = new NameValueCollection { { "client_id", client.ClientId } },
                    Options = new IdentityServerOptions()
                },
                ValidatedResources = new ResourceValidationResult
                {
                    Resources = new Resources(
                        identityResources: Config.IdentityResources,
                        apiResources: Config.ApiResources,
                        apiScopes: Config.ApiScopes)
                },
                IncludeAllIdentityClaims = true
            };

            // ✅ Step 2: Generate Token
            var token = await _tokenService.CreateAccessTokenAsync(tokenRequest);

            if (token == null)
            {
                return Unauthorized("Failed to create access token.");
            }

            // ✅ Step 3: Apply Custom Expiration
            token.Lifetime = (int)tokenExpiration.TotalSeconds;

            // ✅ Step 4: Convert Token to JWT
            var securityToken = await _tokenService.CreateSecurityTokenAsync(token);

            var result = new
            {
                AccessToken = securityToken, // ✅ Correct JWT token string
                ExpiresIn = token.Lifetime // ✅ Token expiration time
            };

            // Redirect user if return URL is valid, otherwise return a JSON response
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Ok(result);
        }









        //    private async Task<IActionResult> HandleSuccessfulLogin(ApplicationUser user, string? returnUrl, bool rememberMe, AuthorizationRequest? context)
        //    {
        //        // 1️⃣ Get User Roles
        //        var roles = await _userManager.GetRolesAsync(user);

        //        // 2️⃣ Create Claims for the Token
        //        var claims = new List<Claim>
        //{
        //    new Claim(JwtClaimTypes.Subject, user.Id),  // ✅ IdentityServer expects 'sub' claim
        //    new Claim(JwtClaimTypes.Name, user.UserName ?? user.Email), // ✅ Standard name claim
        //    new Claim(JwtClaimTypes.Email, user.Email) // ✅ Standard email claim
        //};

        //        // 3️⃣ Add Role Claims (Important for Role-Based Authorization)
        //        foreach (var role in roles)
        //        {
        //            claims.Add(new Claim(JwtClaimTypes.Role, role)); // ✅ Ensuring role is included
        //        }

        //        // 4️ Determine Expiration Time Based on Remember Me
        //        var tokenExpiration = rememberMe
        //            ? TimeSpan.FromDays(_jwtSettings.RememberMeTokenExpirationDays) // Longer token lifetime
        //            : TimeSpan.FromHours(_jwtSettings.TokenExpirationHours); // Shorter token lifetime

        //        // 5️ Create a Fake Validated Request (Required by IdentityServer)
        //        var validatedRequest = new ValidatedTokenRequest
        //        {
        //            Client = context?.Client ?? new Client { ClientId = "default-client" } // Fallback to default client
        //        };

        //        // 6️ Create Token Request
        //        var tokenRequest = new TokenCreationRequest
        //        {
        //            Subject = new ClaimsPrincipal(new ClaimsIdentity(claims, IdentityServerConstants.DefaultCookieAuthenticationScheme)), // Attach Claims
        //            ValidatedRequest = validatedRequest, // Use the validated request
        //            IncludeAllIdentityClaims = true // Ensures all identity claims are included
        //        };

        //        // 7️ Generate Access Token Using IdentityServer4
        //        var accessToken = await _tokenService.CreateAccessTokenAsync(tokenRequest);

        //        // 8️ Convert IdentityServer4 Token to JWT String
        //        var jwtToken = await _tokenService.CreateSecurityTokenAsync(accessToken); // ✅ Correct way to generate JWT

        //        // 9️⃣ Raise IdentityServer Login Success Event
        //        await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context?.Client.ClientId));

        //        // 🔟 Handle Redirect (OAuth2 Login Flow)
        //        if (context != null)
        //        {
        //            return Redirect(context.RedirectUri); // Redirect the user if an authorization request exists
        //        }

        //        // 🔟 Return the Token in API Response
        //        return Ok(new
        //        {
        //            access_token = jwtToken, // ✅ Correctly formatted JWT
        //            expires_in = accessToken.Lifetime, // Expiration in seconds
        //            token_type = "Bearer" // OAuth2-compliant token type
        //        });
        //    }







        //private async Task<IActionResult> HandleSuccessfulLogin(ApplicationUser user, string? returnUrl, bool rememberMe, AuthorizationRequest? context)
        //{


        //    // 1. Get User Roles (for claims)
        //    var roles = await _userManager.GetRolesAsync(user);

        //    // 2. Create Claims
        //    var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.Name, user.UserName), // Or user.Email
        //    new Claim(ClaimTypes.Email, user.Email),
        //    new Claim(ClaimTypes.NameIdentifier, user.Id), // This is important!

        //new Claim(JwtClaimTypes.Subject, user.Id),  // ✅ IdentityServer expects 'sub' claim
        //new Claim(JwtClaimTypes.Name, user.UserName ?? user.Email), // ✅ Use IdentityServer claim types
        //new Claim(JwtClaimTypes.Email, user.Email)
        //};

        //    foreach (var role in roles)
        //    {
        //        claims.Add(new Claim(JwtClaimTypes.Role, role)); // ✅ Ensuring role is included
        //    }

        //    // 3. Create ClaimsIdentity
        //    var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);

        //    // 4. Create ClaimsPrincipal
        //    var principal = new ClaimsPrincipal(identity); // Create the ClaimsPrincipal

        //    // 5. Create AuthenticationProperties (for RememberMe)
        //    var props = new AuthenticationProperties();

        //    // Set expiration time for the token
        //    var expiration = rememberMe
        //        ? DateTime.Now.AddDays(_jwtSettings.RememberMeTokenExpirationDays)
        //        : DateTime.Now.AddHours(_jwtSettings.TokenExpirationHours);

        //    if (rememberMe)
        //    {
        //        props.IsPersistent = true;
        //        props.ExpiresUtc = expiration; //DateTimeOffset.UtcNow.AddDays(30); // Example: 30 days
        //    }

        //    // issue authentication cookie with subject ID and username
        //    var isuser = new IdentityServerUser(user.Id) { DisplayName = user.UserName };

        //    // 6. Sign In (Correct way)
        //    //await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, props); // Correct SignInAsync usage

        //    await HttpContext.SignInAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme, principal, props); // ✅ Fix SignIn scheme


        //    // 7. Raise the UserLoginSuccessEvent (Correct way)
        //    //await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), "local")); // Or your authentication method

        //    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context?.Client.ClientId));

        //    // 8. Handle Return URL or Redirect
        //    // var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

        //    if (context != null)
        //    {
        //        return Redirect(context.RedirectUri);
        //    }

        //    return Ok(new { message = "User logged in successfully." });
        //}

    }
}
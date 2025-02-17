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
                              IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _jwtSettings = jwtSettings.Value; // Access the settings from IOptions
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

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed. Email '{Email}' not found.", model.Email);
                return Unauthorized("Invalid email or password.");
            }

                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return await HandleSuccessfulLogin(user, model.ReturnUrl, model.RememberMe);
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


        private async Task<IActionResult> HandleSuccessfulLogin(ApplicationUser user, string? returnUrl, bool rememberMe)
        {
            // 1. Get User Roles (for claims)
            var roles = await _userManager.GetRolesAsync(user);

            // 2. Create Claims
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName), // Or user.Email
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id) // This is important!
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 3. Create ClaimsIdentity
            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);

            // 4. Create ClaimsPrincipal
            var principal = new ClaimsPrincipal(identity); // Create the ClaimsPrincipal

            // 5. Create AuthenticationProperties (for RememberMe)
            var props = new AuthenticationProperties();

            // Set expiration time for the token
            var expiration = rememberMe
                ? DateTime.Now.AddDays(_jwtSettings.RememberMeTokenExpirationDays)
                : DateTime.Now.AddHours(_jwtSettings.TokenExpirationHours);

            if (rememberMe)
            {
                props.IsPersistent = true;
                props.ExpiresUtc = expiration; //DateTimeOffset.UtcNow.AddDays(30); // Example: 30 days
            }

            // 6. Sign In (Correct way)
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, props); // Correct SignInAsync usage


            // 7. Raise the UserLoginSuccessEvent (Correct way)
            await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), "local")); // Or your authentication method

            // 8. Handle Return URL or Redirect
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context != null)
            {
                return Redirect(context.RedirectUri);
            }

            return Ok(new { message = "User logged in successfully." });
        }

    }
}
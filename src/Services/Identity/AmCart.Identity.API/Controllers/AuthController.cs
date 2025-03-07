﻿//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using System.Net.Mime;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using AmCart.Identity.API.Models;
//using AmCart.Identity.API.Services.Interfaces;
//using Microsoft.AspNetCore.Identity.Data;
//using IdentityServer4.Test;
//using System.Security.Claims;

//namespace IdentityService.Controllers
//{
//    /// <summary>
//    /// Controller for user authentication and registration.
//    /// </summary>
//    [Route("api/auth")]
//    [ApiController]
//    public class AuthController : ControllerBase
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly ILogger<AuthController> _logger;
//        private readonly ITokenService _tokenService;

//        /// <summary>
//        /// Constructor for AuthController.
//        /// </summary>
//        public AuthController(UserManager<ApplicationUser> userManager,
//            SignInManager<ApplicationUser> signInManager,
//            ILogger<AuthController> logger,
//            ITokenService tokenService)
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//            _logger = logger;
//            _tokenService = tokenService;
//        }

//        /// <summary>
//        /// Registers a new user in the system.
//        /// </summary>
//        /// <param name="model">User registration request model.</param>
//        /// <returns>Success or error message.</returns>
//        [HttpPost("register")]
//        [AllowAnonymous]
//        [Consumes(MediaTypeNames.Application.Json)]
//        [ProducesResponseType(StatusCodes.Status201Created)] // No need for typeof(string) if it's just a string
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        public async Task<IActionResult> Register([FromBody] ExtendedRegisterRequest model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState); // Return detailed validation errors
//            }

//            var user = new ApplicationUser
//            {
//                UserName = model.Email, // Or a generated username if you don't want email as username
//                Email = model.Email,
//                Gender = model.Gender,
//                MobileNumber = model.MobileNumber
//            };

//            var result = await _userManager.CreateAsync(user, model.Password);

//            //if (result.Succeeded)
//            //{
//            //    _logger.LogInformation("User '{Email}' registered successfully.", model.Email);




//            //    // Improved: Return a more meaningful Created response with user details
//            //    return CreatedAtAction(nameof(GetUser), new { email = user.Email }, new { message = "User registered successfully.", user = new { user.Id, user.Email, user.Gender, user.MobileNumber } }); // 201 Created with location header

//            //}

//            if (result.Succeeded)
//            {
//                _logger.LogInformation("User '{Email}' registered successfully.", model.Email);

//                // 1. Add User to Role
//                var roleResult = await _userManager.AddToRoleAsync(user, "User"); // Or use a constant: Roles.User
//                if (!roleResult.Succeeded)
//                {
//                    _logger.LogError("Failed to add user '{Email}' to role. Errors: {Errors}", model.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
//                    await _userManager.DeleteAsync(user); // Rollback
//                    return BadRequest(new { errors = new[] { "Failed to assign role. Please contact support." } });
//                }

//                // 2. Generate JWT Token with Role Information (Claims)
//                var claims = new List<Claim>
//            {
//                new Claim(ClaimTypes.Name, user.UserName), // Or user.Email
//                new Claim(ClaimTypes.Email, user.Email),
//                new Claim(ClaimTypes.NameIdentifier, user.Id), // Crucial for user identification
//                // Add other user claims as needed (Gender, MobileNumber, etc.)
//            };

//                // Add roles as claims (important!)
//                var roles = await _userManager.GetRolesAsync(user);
//                foreach (var role in roles)
//                {
//                    claims.Add(new Claim(ClaimTypes.Role, role)); // Or use a custom claim type: "roles"
//                }

//                var token = _tokenService.GenerateJwtToken(user, false, claims);  // Pass claims to your token service


//                return CreatedAtAction(nameof(GetUser), new { email = user.Email }, new
//                {
//                    message = "User registered successfully.",
//                    user = new { user.Id, user.Email, user.Gender, user.MobileNumber },
//                    token = token
//                });
//            }

//            else
//            {
//                _logger.LogWarning("User registration failed for {Email}. Errors: {Errors}",
//                    model.Email, string.Join(", ", result.Errors.Select(e => e.Description))); // Log error descriptions

//                // Improved: Return a more specific error response
//                var errors = new List<string>();
//                foreach (var error in result.Errors)
//                {
//                    errors.Add(error.Description);
//                }
//                return BadRequest(new { errors = errors }); // Return errors in JSON format
//            }
//        }


//        // Example GetUser action (for CreatedAtAction)
//        [HttpGet("user/{email}", Name = "GetUser")] // Named route for CreatedAtAction
//        public async Task<IActionResult> GetUser(string email)
//        {
//            var user = await _userManager.FindByEmailAsync(email);
//            if (user == null)
//            {
//                return NotFound();
//            }
//            return Ok(new { user.Id, user.Email, user.Gender, user.MobileNumber });
//        }


//        /// <summary>
//        /// Logs in a user and returns a JWT token.
//        /// </summary>
//        /// <param name="model">User login request model.</param>
//        /// <returns>A 200 OK response with a JWT token, or a 401 Unauthorized if login fails.</returns>
//        [HttpPost("login")]
//        [AllowAnonymous]
//        [Consumes(MediaTypeNames.Application.Json)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)] // Corrected status code
//        public async Task<IActionResult> Login([FromBody] ExtendedLoginRequest model)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var user = await _userManager.FindByEmailAsync(model.Email);
//            if (user == null)
//            {
//                _logger.LogWarning("Login failed. Email '{Email}' not found.", model.Email);
//                return Unauthorized("Invalid email or password.");
//            }

//            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
//            if (!result.Succeeded)
//            {
//                _logger.LogWarning("Login failed for '{Email}'. Incorrect credentials.", model.Email);
//                return Unauthorized("Invalid email or password.");
//            }

//            var claims = new List<Claim>();
//            var roles = await _userManager.GetRolesAsync(user);
//            foreach (var role in roles)
//            {
//                claims.Add(new Claim(ClaimTypes.Role, role));
//            }

//            var token = _tokenService.GenerateJwtToken(user, model.RememberMe, claims); // Include claims here

//            _logger.LogInformation("User '{Email}' logged in successfully.", model.Email);
//            return Ok(new { Token = token });
//        }


//        /// <summary>
//        /// Logs out the authenticated user.
//        /// </summary>
//        /// <returns>Success message if logout is successful.</returns>
//        [HttpPost("logout")]
//        [Authorize] // Requires authentication to log out
//        public async Task<IActionResult> Logout()
//        {
//            string userId = User.Identity?.Name ?? "Unknown"; // Safe null check

//            await _signInManager.SignOutAsync();  // Clears authentication session

//            _logger.LogInformation("User '{UserId}' logged out successfully.", userId);

//            return Ok(new { message = "User logged out successfully." });
//        }


//        /// <summary>
//        /// Retrieves the profile of the currently logged-in user.
//        /// </summary>
//        [HttpGet("profile")]
//        [Authorize] // Requires authentication
//        public async Task<IActionResult> GetProfile()
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            if (userId == null) return Unauthorized();

//            var user = await _userManager.FindByIdAsync(userId);
//            if (user == null) return NotFound("User not found");

//            return Ok(new
//            {
//                user.Id,
//                user.Email,
//                user.Gender,
//                user.MobileNumber
//            });
//        }


//        /// <summary>
//        /// Updates the profile information of the logged-in user.
//        /// </summary>
//        [HttpPut("profile")]
//        [Authorize] // User must be logged in to update profile
//        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest model)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            if (userId == null) return Unauthorized();

//            var user = await _userManager.FindByIdAsync(userId);
//            if (user == null) return NotFound("User not found");

//            // Update user details
//            user.Gender = model.Gender;
//            user.MobileNumber = model.MobileNumber;

//            var result = await _userManager.UpdateAsync(user);
//            if (!result.Succeeded)
//            {
//                return BadRequest(result.Errors);
//            }

//            return Ok(new { message = "Profile updated successfully." });
//        }



//    }
//}

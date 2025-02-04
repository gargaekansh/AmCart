using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AmCart.Identity.API.Models;
using AmCart.Identity.API.Services;
using AmCart.Identity.API.Services.Interfaces;

namespace IdentityService.Controllers
{
    /// <summary>
    /// Controller for user authentication.
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthController> _logger;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<ApplicationUser> userManager
            , SignInManager<ApplicationUser> signInManager
            , ILogger<AuthController> logger
            , ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Logs in a user and returns a JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ExtendedLoginRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized("Invalid email or password");

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (!result.Succeeded)
                return Unauthorized("Invalid email or password");

            // Generate JWT token
            var token = _tokenService.GenerateJwtToken(user, model.RememberMe);
            return Ok(new { Token = token });
        }



    }
}

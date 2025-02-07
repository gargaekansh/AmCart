using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace User.API.Controllers
{

    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet("profile")]
        [Authorize(Policy = "UserApiScope")]
        public IActionResult GetUserProfile()
        {
            var userClaims = User.Claims;

            var userProfile = new
            {
                Id = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                Email = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                Name = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                Gender = userClaims.FirstOrDefault(c => c.Type == "gender")?.Value,
                MobileNumber = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value
            };

            return Ok(userProfile);
        }
    }

}

using Microsoft.AspNetCore.Identity;

namespace AmCart.Identity.API.Models
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// User gender.
        /// </summary>
        public string? Gender { get; set; }

        /// <summary>
        /// User mobile number.
        /// </summary>
        public string? MobileNumber { get; set; }
    }
}

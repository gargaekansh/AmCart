using System.ComponentModel.DataAnnotations;

namespace AmCart.Identity.API.Models
{

    /// <summary>
    /// Represents a user registration request.
    /// </summary>
    public class ExtendedRegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "garg@gmail.com";

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "Pass@word123";

        [Required]
        public string Gender { get; set; } = "M";

        [Required, MinLength(10), MaxLength(10)]
        public string MobileNumber { get; set; } = "7406990823";
    }
}

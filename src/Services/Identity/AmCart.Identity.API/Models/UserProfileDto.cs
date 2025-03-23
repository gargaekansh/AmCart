namespace AmCart.Identity.API.Models
{
    /// <summary>
    /// DTO for returning user profile information.
    /// </summary>
    public class UserProfileDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string MobileNumber { get; set; }

        public UserProfileDto(ApplicationUser user)
        {
            Id = user.Id;
            Email = user.Email;
            Gender = user.Gender;
            MobileNumber = user.MobileNumber;
        }
    }
}

namespace AmCart.Identity.API.Configuration
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int TokenExpirationHours { get; set; } = 1; // Default token expiration time
        public int RememberMeTokenExpirationDays { get; set; } = 7; // Default "Remember Me" expiration time
    }

}

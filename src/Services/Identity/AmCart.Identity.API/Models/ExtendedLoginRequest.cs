﻿using Microsoft.AspNetCore.Identity.Data;

namespace AmCart.Identity.API.Models
{
    /// <summary>
    /// The request type for the "/login" endpoint, including the RememberMe functionality.
    /// </summary>
    public sealed class ExtendedLoginRequest
    {
        /// <summary>
        /// The user's email address which acts as a user name.
        /// </summary>
        public required string Email { get; init; }

        /// <summary>
        /// The user's password.
        /// </summary>
        public required string Password { get; init; }

        /// <summary>
        /// The optional two-factor authenticator code. This may be required for users who have enabled two-factor authentication.
        /// This is not required if a <see cref="TwoFactorRecoveryCode"/> is sent.
        /// </summary>
        public string? TwoFactorCode { get; init; }

        /// <summary>
        /// An optional two-factor recovery code from <see cref="TwoFactorResponse.RecoveryCodes"/>.
        /// This is required for users who have enabled two-factor authentication but lost access to their <see cref="TwoFactorCode"/>.
        /// </summary>
        public string? TwoFactorRecoveryCode { get; init; }

        /// <summary>
        /// Indicates whether the user wants to be remembered (i.e., stay logged in after authentication).
        /// </summary>
        public bool RememberMe { get; init; }


        /// <summary>
        /// The URL to redirect to after successful login.
        /// </summary>
        public string? ReturnUrl { get; init; } // Added ReturnUrl property
    }
}

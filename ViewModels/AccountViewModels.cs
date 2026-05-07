using System.ComponentModel.DataAnnotations;

namespace nsia.ViewModels
{
    public class RegisterViewModel
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = default!;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = default!;

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = default!;

        [Required]
        public string Gender { get; set; } = default!;
        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(100)]
        public string? HowDidYouHear { get; set; }

        [Required, MinLength(8)]
        public string Password { get; set; } = default!;

        [Required, Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = default!;

        public bool AgreeToTerms { get; set; }
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;

        public bool RememberMe { get; set; }
    }

    public class VerifyEmailJsonModel
    {
        public string Email { get; set; } = default!;
        public string Otp { get; set; } = default!;
    }

    public class ResendOtpModel
    {
        public string Email { get; set; } = default!;
    }

    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;
    }
}
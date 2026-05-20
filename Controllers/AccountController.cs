using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nsia.Data;
using nsia.Models;
using nsia.Services;
using nsia.ViewModels;
using BCrypt.Net;

namespace Nsia.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _email;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            ApplicationDbContext db,
            IEmailService email,
            ILogger<AccountController> logger)
        {
            _db = db;
            _email = email;
            _logger = logger;
        }


        // ── GET /Account/Register
        [HttpGet]
        public IActionResult Register() =>
            IsLoggedIn() ? RedirectToDashboard() : View();

        // ── POST /Account/Register
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {

            if (!ModelState.IsValid) return View(model);

            if (!model.AgreeToTerms)
            {
                ModelState.AddModelError("", "You must agree to the Terms of Use and Privacy Policy.");
                return View(model);
            }

            var exists = await _db.Applications
                .AnyAsync(a => a.Email == model.Email.ToLowerInvariant());

            if (exists)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(model);
            }

            var otp = GenerateOtp();
            var refNumber = GenerateReference();

            var application = new Application
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.ToLowerInvariant().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Phone = model.PhoneNumber.Trim(),
                Gender = model.Gender,
                // Location = model.Location?.Trim(),
                // HowDidYouHear = model.HowDidYouHear?.Trim(),
                IsEmailVerified = false,
                EmailVerificationOtp = otp,
                OtpExpiresAt = DateTime.UtcNow.AddMinutes(10),
                ReferenceNumber = refNumber,
                Status = "Draft",
                ApplicationStep = 1,
                // AgreesToNsiaPrivacyPolicy = model.AgreeToTerms
            };

            _db.Applications.Add(application);
            await _db.SaveChangesAsync();
            await _email.SendOtpEmailAsync(application.Email, application.FullName, otp);

            TempData["VerifyEmail"] = application.Email;
            TempData["MaskedEmail"] = MaskEmail(application.Email);
            return RedirectToAction("VerifyEmail");
        }

        // GET /Account/VerifyEmail
        [HttpGet]
        public IActionResult VerifyEmail()
        {
            var email = TempData["VerifyEmail"] as string ?? "";
            ViewBag.MaskedEmail = TempData["MaskedEmail"] as string ?? "your email";
            ViewBag.ActualEmail = email;

            // Keep for resend button
            TempData["VerifyEmail"] = email;
            TempData["MaskedEmail"] = ViewBag.MaskedEmail;
            return View();
        }

        // POST /Account/VerifyEmail — receives JSON from fetch
        [HttpPost]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailJsonModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Otp) || string.IsNullOrEmpty(model.Email))
                return BadRequest(new { message = "Invalid request." });

            var application = await _db.Applications
                .FirstOrDefaultAsync(a => a.Email == model.Email.ToLowerInvariant());

            if (application == null)
                return BadRequest(new { message = "Account not found." });

            if (application.IsEmailVerified)
            {
                SignIn(application);
                return Ok(new { redirectUrl = "/Application/Dashboard" });
            }

            if (application.EmailVerificationOtp != model.Otp ||
                application.OtpExpiresAt < DateTime.UtcNow)
                return BadRequest(new { message = "Invalid or expired code. Please request a new one." });

            application.IsEmailVerified = true;
            application.EmailVerificationOtp = null;
            application.OtpExpiresAt = null;
            application.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _email.SendWelcomeEmailAsync(
                application.Email,
                application.FullName,
                application.ReferenceNumber!);

            SignIn(application);
            return Ok(new { redirectUrl = "/Application/Dashboard" });
        }

        // POST /Account/ResendOtp — receives JSON
        [HttpPost]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email))
                return BadRequest(new { success = false, message = "Email is required." });

            var application = await _db.Applications
                .FirstOrDefaultAsync(a => a.Email == model.Email.ToLowerInvariant());

            if (application == null)
                return BadRequest(new { success = false, message = "Account not found." });

            if (application.IsEmailVerified)
                return BadRequest(new { success = false, message = "Email already verified." });

            var otp = GenerateOtp();
            application.EmailVerificationOtp = otp;
            application.OtpExpiresAt = DateTime.UtcNow.AddMinutes(10);
            application.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await _email.SendOtpEmailAsync(application.Email, application.FullName, otp);

            return Ok(new { success = true });
        }

        // ── GET /Account/Login
        [HttpGet]
        public IActionResult Login() =>
            IsLoggedIn() ? RedirectToDashboard() : View();

        // ── POST /Account/Login
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var application = await _db.Applications
                .FirstOrDefaultAsync(a => a.Email == model.Email.ToLowerInvariant());

            if (application == null ||
                !BCrypt.Net.BCrypt.Verify(model.Password, application.PasswordHash))
            {
                ViewBag.ShowError = true;
                ViewBag.ErrorText = "Invalid email or password. Please try again.";
                return View(model);
            }

            if (!application.IsEmailVerified)
            {
                TempData["VerifyEmail"] = application.Email;
                TempData["MaskedEmail"] = MaskEmail(application.Email);
                return RedirectToAction("VerifyEmail");
            }

            SignIn(application, model.RememberMe);
            return RedirectToAction("Dashboard", "Application");
        }

        // ── GET /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ViewBag.Error = "Please enter your email address.";
                return View();
            }

            var app = await _db.Applications
                .FirstOrDefaultAsync(a => a.Email == Email.ToLowerInvariant().Trim());

            // Always show success to prevent email enumeration
            ViewBag.Success = true;
            ViewBag.Email = Email;

            if (app == null) return View();

            // Generate OTP
            var otp = new Random().Next(100000, 999999).ToString();
            app.EmailVerificationOtp = otp;
            app.OtpExpiresAt = DateTime.UtcNow.AddMinutes(15);
            app.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Send reset email
            await _email.SendOtpEmailAsync(app.Email!, app.FullName!, otp);

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string Email, string Otp, string NewPassword)
        {
            ViewBag.Email = Email;

            var app = await _db.Applications
                .FirstOrDefaultAsync(a => a.Email == Email.ToLowerInvariant().Trim());

            if (app == null ||
                app.EmailVerificationOtp != Otp ||
                app.OtpExpiresAt < DateTime.UtcNow)
            {
                ViewBag.Error = "Invalid or expired code. Please request a new one.";
                return View();
            }

            app.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            app.EmailVerificationOtp = null;
            app.OtpExpiresAt = null;
            app.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            ViewBag.Done = true;
            return View();
        }
        // ─────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────

        private void SignIn(Application application, bool rememberMe = false)
        {
            HttpContext.Session.SetString("ApplicationId", application.Id.ToString());
            HttpContext.Session.SetString("ApplicantName", application.FullName);
            HttpContext.Session.SetString("ApplicantEmail", application.Email);
        }

        private bool IsLoggedIn() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString("ApplicationId"));

        private IActionResult RedirectToDashboard() =>
            RedirectToAction("Dashboard", "Application");

        private static string GenerateOtp() =>
            Random.Shared.Next(100000, 999999).ToString();

        private static string GenerateReference()
        {
            var year = DateTime.UtcNow.Year;
            var rand = Random.Shared.Next(10000, 99999);
            return $"NPI-{year}-{rand}";
        }

        private static string MaskEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2) return email;
            var local = parts[0];
            var masked = local.Length <= 2
                ? local[0] + "***"
                : local[0] + "***" + local[^1];
            return $"{masked}@{parts[1]}";
        }
    }
}
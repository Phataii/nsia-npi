using System.Text;
using System.Text.Json;

namespace nsia.Services
{
    public class EmailService : IEmailService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        private string ApiKey => _config["Brevo:ApiKey"] ?? throw new InvalidOperationException("Brevo API key not configured.");
        private string SenderName => _config["Brevo:SenderName"] ?? "NSIA Prize for Innovation";
        private string SenderEmail => _config["Brevo:SenderEmail"] ?? "corporatecommunications@loftyincltd.biz";

        public EmailService(
            HttpClient http,
            IConfiguration config,
            ILogger<EmailService> logger)
        {
            _http = http;
            _config = config;
            _logger = logger;
        }

        // ─────────────────────────────────
        // PRIVATE SEND HELPER
        // ─────────────────────────────────

        private async Task SendAsync(
            string toEmail,
            string toName,
            string subject,
            string htmlContent)
        {
            var payload = new
            {
                sender = new { name = SenderName, email = SenderEmail },
                to = new[] { new { email = toEmail, name = toName } },
                subject,
                htmlContent,
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://api.brevo.com/v3/smtp/email");

            request.Headers.Add("api-key", ApiKey);
            request.Headers.Add("Accept", "application/json");
            request.Content = content;

            try
            {
                var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Brevo returned {Status} sending to {Email}: {Error}",
                        (int)response.StatusCode, toEmail, error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Brevo request failed sending to {Email}", toEmail);
            }
        }

        // ─────────────────────────────────
        // OTP EMAIL
        // ─────────────────────────────────

        public async Task SendOtpEmailAsync(string toEmail, string toName, string otp)
        {
            var html = $@"
            <div style='font-family:DM Sans,sans-serif;max-width:520px;margin:0 auto;padding:32px 24px;background:#faf8f3'>
                <div style='background:#20A1B8;border-radius:12px;padding:32px;margin-bottom:24px'>
                    <p style='font-size:11px;color:white;letter-spacing:.08em;text-transform:uppercase;margin:0 0 16px'>
                        NSIA · Prize for Innovation
                    </p>
                    <h1 style='font-family:Georgia,serif;font-size:28px;color:white;margin:0 0 8px;line-height:1.1'>
                        Verify your <span style='color:#83c240;font-style:italic'>email address.</span>
                    </h1>
                    <p style='font-size:14px;color:rgba(255,255,255,0.55);margin:0'>
                        Enter this code to confirm your account.
                    </p>
                </div>
                <div
                    style='background:white;border-radius:12px;border:1px solid rgba(0,0,0,0.08);padding:32px;text-align:center;margin-bottom:16px'>
                    <p style='font-size:13px;color:#5a6b60;margin:0 0 16px'>Your verification code</p>
                    <div
                        style='font-family:Georgia,serif;font-size:48px;font-weight:700;color:#0d1a12;letter-spacing:.25em;margin:0 0 16px'>
                        {otp}
                    </div>
                    <p style='font-size:12px;color:#5a6b60;margin:0'>
                        This code expires in <strong>10 minutes</strong>.
                        Never share it with anyone, including NSIA staff.
                    </p>
                </div>
                <div
                    style='background:rgba(14,134,171,0.06);border:1px solid rgba(14,134,171,0.15);border-radius:8px;padding:14px 16px;margin-bottom:16px'>
                    <p style='font-size:12px;color:#0a6b8a;margin:0'>
                        If you did not create an NSIA NPI account, you can safely ignore this email.
                    </p>
                </div>
                <p style='font-size:11px;color:#aab5af;text-align:center;margin:0'>
                    &copy; {DateTime.UtcNow.Year} Nigeria Sovereign Investment Authority
                </p>
            </div>";

            await SendAsync(toEmail, toName, "Your NSIA NPI verification code", html);
        }

        // ─────────────────────────────────
        // WELCOME EMAIL
        // ─────────────────────────────────

        public async Task SendWelcomeEmailAsync(
            string toEmail,
            string toName,
            string referenceNumber)
        {
            var firstName = toName.Split(' ')[0];

            var html = $@"
            <div style='font-family:DM Sans,sans-serif;max-width:520px;margin:0 auto;padding:32px 24px;background:#faf8f3'>
                <div style='background:#20A1B8;border-radius:12px;padding:32px;margin-bottom:24px'>
                    <p style='font-size:11px;color:white;letter-spacing:.08em;text-transform:uppercase;margin:0 0 16px'>
                        NSIA · Prize for Innovation
                    </p>
                    <h1 style='font-family:Georgia,serif;font-size:28px;color:white;margin:0 0 8px;line-height:1.1'>
                        Welcome, <span style='color:#83c240;font-style:italic'>{firstName}.</span>
                    </h1>
                    <p style='font-size:14px;color:rgba(255,255,255,0.55);margin:0'>
                        Your email has been verified. You can now complete your NPI 4.0 application.
                    </p>
                </div>
                <div style='background:white;border-radius:12px;border:1px solid rgba(0,0,0,0.08);padding:24px;margin-bottom:16px'>
                    <p style='font-size:12px;color:#5a6b60;margin:0 0 8px;text-transform:uppercase;letter-spacing:.06em'>
                        Your application reference
                    </p>
                    <p
                        style='font-family:Georgia,serif;font-size:24px;font-weight:700;color:#0d1a12;letter-spacing:.05em;margin:0 0 16px'>
                        {referenceNumber}
                    </p>
                    <p style='font-size:13px;color:#5a6b60;margin:0;line-height:1.6'>
                        Keep this reference number safe — you will need it for any correspondence
                        with the NSIA NPI team.
                    </p>
                </div>
                <div
                    style='background:rgba(8,112,58,0.06);border:1px solid rgba(8,112,58,0.15);border-radius:8px;padding:14px 16px;margin-bottom:16px'>
                    <p style='font-size:12px;color:#065a2f;margin:0 0 4px;font-weight:500'>Next steps</p>
                    <p style='font-size:12px;color:#08703a;margin:0;line-height:1.6'>
                        Log in to your dashboard and complete all 7 sections of your application
                        before the deadline of <strong>5 June 2026</strong>.
                    </p>
                </div>
                <p style='font-size:11px;color:#aab5af;text-align:center;margin:0'>
                    Questions? Email
                    <a href='mailto:corporatecommunications@loftyincltd.biz' style='color:#0e86ab;text-decoration:none'>corporatecommunications@loftyincltd.biz</a>
                    &nbsp;·&nbsp;
                    &copy; {DateTime.UtcNow.Year} Nigeria Sovereign Investment Authority
                </p>
            </div>";

            await SendAsync(
                toEmail,
                toName,
                $"Welcome to NPI 4.0 — Your reference: {referenceNumber}",
                html);
        }

        // ─────────────────────────────────
        // SUBMISSION CONFIRMATION
        // ─────────────────────────────────

        public async Task SendSubmissionConfirmationAsync(
            string toEmail,
            string toName,
            string referenceNumber)
        {
            var firstName = toName.Split(' ')[0];
            var submittedOn = DateTime.UtcNow.ToString("dd MMMM yyyy");
            var submittedAt = DateTime.UtcNow.ToString("HH:mm") + " UTC";

            var html = $@"
            <div style='font-family:DM Sans,sans-serif;max-width:520px;margin:0 auto;padding:32px 24px;background:#faf8f3'>
                <div style='background:#20A1B8;border-radius:12px;padding:32px;margin-bottom:24px'>
                    <p style='font-size:11px;color: white;letter-spacing:.08em;text-transform:uppercase;margin:0 0 16px'>
                        NSIA · Prize for Innovation
                    </p>
                    <h1 style='font-family:Georgia,serif;font-size:28px;color:white;margin:0 0 8px;line-height:1.1'>
                        Application <span style='color:#83c240;font-style:italic'>received!</span>
                    </h1>
                    <p style='font-size:14px;color:rgba(255,255,255,0.55);margin:0'>
                        Thank you {firstName} — your NPI 4.0 application has been successfully submitted.
                    </p>
                </div>
                <div style='background:white;border-radius:12px;border:1px solid rgba(0,0,0,0.08);padding:24px;margin-bottom:16px'>
                    <table style='width:100%;border-collapse:collapse'>
                        <tr>
                            <td
                                style='font-size:12px;color:#5a6b60;padding:6px 0;text-transform:uppercase;letter-spacing:.06em;width:45%'>
                                Reference</td>
                            <td style='font-family:Georgia,serif;font-size:16px;font-weight:700;color:#0d1a12;padding:6px 0'>
                                {referenceNumber}
                            </td>
                        </tr>
                        <tr>
                            <td style='font-size:12px;color:#5a6b60;padding:6px 0;text-transform:uppercase;letter-spacing:.06em'>
                                Submitted on</td>
                            <td style='font-size:14px;color:#0d1a12;padding:6px 0'>{submittedOn}</td>
                        </tr>
                        <tr>
                            <td style='font-size:12px;color:#5a6b60;padding:6px 0;text-transform:uppercase;letter-spacing:.06em'>
                                Time</td>
                            <td style='font-size:14px;color:#0d1a12;padding:6px 0'>{submittedAt}</td>
                        </tr>
                        <tr>
                            <td style='font-size:12px;color:#5a6b60;padding:6px 0;text-transform:uppercase;letter-spacing:.06em'>
                                Status</td>
                            <td style='padding:6px 0'>
                                <span
                                    style='background:rgba(8,112,58,0.1);color:#08703a;font-size:11px;font-weight:500;padding:3px 10px;border-radius:100px'>
                                    Submitted
                                </span>
                            </td>
                        </tr>
                    </table>
                </div>
                <div
                    style='background:rgba(14,134,171,0.06);border:1px solid rgba(14,134,171,0.15);border-radius:8px;padding:14px 16px;margin-bottom:16px'>
                    <p style='font-size:12px;color:#0a6b8a;margin:0 0 4px;font-weight:500'>What happens next?</p>
                    <p style='font-size:12px;color:#0e86ab;margin:0;line-height:1.7'>
                        Our review panel will assess your application. Shortlisted applicants will be
                        contacted by <strong>31 July 2026</strong>. You may log in at any time to
                        review your submitted application.
                    </p>
                </div>
                <p style='font-size:11px;color:#aab5af;text-align:center;margin:0;line-height:1.8'>
                    Questions? Email
                    <a href='mailto:corporatecommunications@loftyincltd.biz' style='color:#0e86ab;text-decoration:none'>corporatecommunications@loftyincltd.biz</a>
                    <br />
                    &copy; {DateTime.UtcNow.Year} Nigeria Sovereign Investment Authority &nbsp;·&nbsp; Abuja, Nigeria
                </p>
            </div>";

            await SendAsync(
                toEmail,
                toName,
                $"Application Received — NPI 4.0 Reference: {referenceNumber}",
                html);
        }
    }
}
namespace nsia.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string toName, string otp);
        Task SendWelcomeEmailAsync(string toEmail, string toName, string referenceNumber);
        Task SendSubmissionConfirmationAsync(string toEmail, string toName, string referenceNumber);
    }
}
namespace WordWisp.API.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string email, string verificationCode, string userName);
        Task SendCertificateEmailAsync(string email, string userName, string englishLevel, DateTime completedAt, int totalScore);
    }
}

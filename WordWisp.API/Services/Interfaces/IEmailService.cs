namespace WordWisp.API.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string email, string verificationCode, string userName);
    }
}

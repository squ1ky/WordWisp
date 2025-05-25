using MailKit.Net.Smtp;
using MimeKit;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Services.Implementations
{
    public class YandexEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public YandexEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendVerificationEmailAsync(string email, string verificationCode, string userName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("WordWisp", _configuration["Email:From"]));
            message.To.Add(new MailboxAddress(userName, email));
            message.Subject = "Подтверждение регистрации в WordWisp";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <h2>Добро пожаловать в WordWisp, {userName}!</h2>
                    <p>Для завершения регистрации введите код подтверждения:</p>
                    <h3 style='color: #007bff; font-size: 24px; letter-spacing: 3px;'>{verificationCode}</h3>
                    <p>Код действителен в течение 15 минут.</p>
                    <p>Если вы не регистрировались в WordWisp, проигнорируйте это письмо.</p>
                ",
                TextBody = $@"
                    Добро пожаловать в WordWisp, {userName}!
                    
                    Для завершения регистрации введите код подтверждения: {verificationCode}
                    
                    Код действителен в течение 15 минут.
                    
                    Если вы не регистрировались в WordWisp, проигнорируйте это письмо.
                "
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            await client.ConnectAsync(_configuration["Email:Host"],
                int.Parse(_configuration["Email:Port"]),
                MailKit.Security.SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_configuration["Email:Username"],
                _configuration["Email:Password"]);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}

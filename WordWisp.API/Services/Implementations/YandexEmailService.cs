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

            await SendEmailAsync(message);
        }

        public async Task SendCertificateEmailAsync(string email, string userName, string englishLevel, DateTime completedAt, int totalScore)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("WordWisp", _configuration["Email:From"]));
            message.To.Add(new MailboxAddress(userName, email));
            message.Subject = "Сертификат о прохождении теста уровня английского языка - WordWisp";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = GenerateCertificateHtml(userName, englishLevel, completedAt, totalScore),
                TextBody = GenerateCertificateText(userName, englishLevel, completedAt, totalScore)
            };

            message.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(message);
        }

        private async Task SendEmailAsync(MimeMessage message)
        {
            using var client = new SmtpClient();

            await client.ConnectAsync(_configuration["Email:Host"],
                int.Parse(_configuration["Email:Port"]),
                MailKit.Security.SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_configuration["Email:Username"],
                _configuration["Email:Password"]);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private string GenerateCertificateHtml(string userName, string englishLevel, DateTime completedAt, int totalScore)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: 'Arial', sans-serif; margin: 0; padding: 20px; background-color: #f8f9fa; }}
                        .certificate {{ max-width: 800px; margin: 0 auto; background: white; border-radius: 15px; box-shadow: 0 10px 30px rgba(0,0,0,0.1); overflow: hidden; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 40px; text-align: center; }}
                        .header h1 {{ margin: 0; font-size: 2.5rem; font-weight: 700; }}
                        .header .subtitle {{ margin: 10px 0 0; font-size: 1.2rem; opacity: 0.9; }}
                        .content {{ padding: 40px; text-align: center; }}
                        .recipient {{ font-size: 1.8rem; color: #2c3e50; margin: 20px 0; font-weight: 600; }}
                        .level {{ font-size: 3rem; color: #27ae60; font-weight: 700; margin: 30px 0; text-shadow: 2px 2px 4px rgba(0,0,0,0.1); }}
                        .details {{ background: #f8f9fa; padding: 30px; border-radius: 10px; margin: 30px 0; }}
                        .details h3 {{ color: #2c3e50; margin-bottom: 20px; }}
                        .detail-item {{ display: inline-block; margin: 10px 20px; padding: 15px 25px; background: white; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .detail-label {{ display: block; font-size: 0.9rem; color: #7f8c8d; margin-bottom: 5px; }}
                        .detail-value {{ display: block; font-size: 1.2rem; color: #2c3e50; font-weight: 600; }}
                        .footer {{ background: #2c3e50; color: white; padding: 30px; text-align: center; }}
                        .logo {{ font-size: 1.5rem; font-weight: 700; margin-bottom: 10px; }}
                    </style>
                </head>
                <body>
                    <div class='certificate'>
                        <div class='header'>
                            <h1>🎓 СЕРТИФИКАТ</h1>
                            <div class='subtitle'>о прохождении теста уровня английского языка</div>
                        </div>
                        
                        <div class='content'>
                            <p style='font-size: 1.2rem; color: #7f8c8d; margin-bottom: 10px;'>Настоящим подтверждается, что</p>
                            
                            <div class='recipient'>{userName}</div>
                            
                            <p style='font-size: 1.2rem; color: #7f8c8d; margin: 20px 0;'>успешно прошел(а) тест на определение уровня английского языка и показал(а) результат:</p>
                            
                            <div class='level'>{englishLevel}</div>
                            
                            <div class='details'>
                                <h3>Детали тестирования</h3>
                                <div class='detail-item'>
                                    <span class='detail-label'>Общий балл</span>
                                    <span class='detail-value'>{totalScore} из 110</span>
                                </div>
                                <div class='detail-item'>
                                    <span class='detail-label'>Дата прохождения</span>
                                    <span class='detail-value'>{completedAt:dd.MM.yyyy}</span>
                                </div>
                                <div class='detail-item'>
                                    <span class='detail-label'>Время прохождения</span>
                                    <span class='detail-value'>{completedAt:HH:mm}</span>
                                </div>
                            </div>
                            
                            <p style='color: #7f8c8d; margin-top: 30px; font-style: italic;'>
                                Тест включал оценку грамматики, словарного запаса и навыков чтения.<br>
                                Результат соответствует международной шкале CEFR.
                            </p>
                        </div>
                        
                        <div class='footer'>
                            <div class='logo'>WordWisp</div>
                            <p style='margin: 0; opacity: 0.8;'>Платформа для изучения английского языка</p>
                            <p style='margin: 5px 0 0; font-size: 0.9rem; opacity: 0.7;'>wordwisp.com</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        private string GenerateCertificateText(string userName, string englishLevel, DateTime completedAt, int totalScore)
        {
            return $@"
                СЕРТИФИКАТ
                о прохождении теста уровня английского языка
                
                Настоящим подтверждается, что {userName} успешно прошел(а) тест на определение уровня английского языка.
                
                РЕЗУЛЬТАТ: {englishLevel}
                
                Детали тестирования:
                - Общий балл: {totalScore} из 110
                - Дата прохождения: {completedAt:dd.MM.yyyy}
                - Время прохождения: {completedAt:HH:mm}
                
                Тест включал оценку грамматики, словарного запаса и навыков чтения.
                Результат соответствует международной шкале CEFR.
                
                WordWisp
                Платформа для изучения английского языка
                wordwisp.com
            ";
        }
    }
}

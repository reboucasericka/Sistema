using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Sistema.Services
{
    public interface IEmailService
    {
        Task SendActivationEmailAsync(string email, string firstName, string activationLink);
        Task SendEmailAsync(string email, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
            _smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
            _fromEmail = _configuration["Email:FromEmail"] ?? "";
            _fromName = _configuration["Email:FromName"] ?? "Ewellin Jordão";
        }

        public async Task SendActivationEmailAsync(string email, string firstName, string activationLink)
        {
            var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = "Ativação da sua conta - Ewellin Jordão",
                Body = CreateActivationEmailBody(firstName, activationLink),
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendEmailAsync(string email, string subject, string body)
        {
            var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }

        private string CreateActivationEmailBody(string firstName, string activationLink)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: 'Montserrat', Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #C97D5D 0%, #D9A791 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f8f8f8; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .btn {{ display: inline-block; background: linear-gradient(135deg, #C97D5D 0%, #D9A791 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 25px; font-weight: 600; margin: 20px 0; }}
                        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 14px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Bem-vinda, {firstName}!</h1>
                            <p>Ative sua conta para começar a agendar</p>
                        </div>
                        <div class='content'>
                            <h2>Quase lá! 🎉</h2>
                            <p>Obrigada por se cadastrar no nosso sistema. Para ativar sua conta e começar a agendar seus tratamentos de beleza, clique no botão abaixo:</p>
                            
                            <div style='text-align: center;'>
                                <a href='{activationLink}' class='btn'>Ativar Minha Conta</a>
                            </div>
                            
                            <p>Após a ativação, você será redirecionada para nossa página de agendamento onde poderá escolher seus serviços favoritos.</p>
                            
                            <p><strong>Não consegue clicar no botão?</strong><br>
                            Copie e cole este link no seu navegador:<br>
                            <a href='{activationLink}' style='color: #C97D5D;'>{activationLink}</a></p>
                            
                            <p>Este link expira em 24 horas por motivos de segurança.</p>
                        </div>
                        <div class='footer'>
                            <p>Com carinho,<br><strong>Equipe Ewellin Jordão</strong></p>
                            <p>Se você não criou esta conta, pode ignorar este email.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }
    }
}


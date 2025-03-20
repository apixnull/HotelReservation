using System.Net;
using System.Net.Mail;

namespace HotelReservation.Services
{
    public class EmailService
    {
        private readonly string _fromEmail;
        private readonly string _fromPassword;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        public EmailService(IConfiguration configuration)
        {
            _fromEmail = configuration["EmailSettings:FromEmail"]
                ?? throw new ArgumentNullException(nameof(configuration), "FromEmail is missing in configuration.");

            _fromPassword = configuration["EmailSettings:AppPassword"]
                ?? throw new ArgumentNullException(nameof(configuration), "AppPassword is missing in configuration.");

            _smtpHost = configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"] ?? "587");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using (var smtpClient = new SmtpClient(_smtpHost))
            {
                smtpClient.Port = _smtpPort;
                smtpClient.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}

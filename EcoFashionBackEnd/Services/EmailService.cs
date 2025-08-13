using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using EcoFashionBackEnd.Helpers;

namespace EcoFashionBackEnd.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task<bool> SendEmailAsync(MailData mailData)
        {
            try
            {
                using var smtpClient = new SmtpClient(_mailSettings.Server)
                {
                    Port = int.Parse(_mailSettings.Port),
                    Credentials = new NetworkCredential(_mailSettings.UserName, _mailSettings.Password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_mailSettings.SenderEmail, _mailSettings.SenderName),
                    Subject = mailData.EmailSubject,
                    Body = mailData.EmailBody,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(new MailAddress(mailData.EmailToId, mailData.EmailToName));

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception here if you have a logging service
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }
    }
}

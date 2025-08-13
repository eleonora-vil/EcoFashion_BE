using EcoFashionBackEnd.Helpers;

namespace EcoFashionBackEnd.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(MailData mailData);
    }
}

using System.Threading.Tasks;

namespace CiergeLib.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}

using System.Threading.Tasks;

namespace Nexplorer.Web.Services.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(EmailMessage emailMessage);
        Task SendEmailConfirmationAsync(string email, string username, string link);
    }
}

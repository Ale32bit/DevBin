using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace DevBin.Services
{
    public class SendGrid
    {
        private readonly string token;
        private readonly string address;
        private readonly SendGridClient client;
        public SendGrid(string _token, string from)
        {
            token = _token;
            address = from;
            client = new SendGridClient(token);
        }

        public async Task SendEmail(EmailAddress recipient, string subject, string content, string htmlContent)
        {
            var from = new EmailAddress(address, "DevBin");
            var msg = MailHelper.CreateSingleEmail(from, recipient, subject, content, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}

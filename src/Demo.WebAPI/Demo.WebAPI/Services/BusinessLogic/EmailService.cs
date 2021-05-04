using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Demo.WebAPI.Services.BusinessLogic {
    public class EmailService {
        private readonly string companyMailAddress;
        private readonly string smtpAddress;
        private readonly int smtpPort;
        private readonly string password;

        public EmailService(IConfiguration configuration) {
            companyMailAddress = configuration.GetValue<string>("Email:CompanyMailAddress");
            smtpAddress = configuration.GetValue<string>("Email:SmtpAddress");
            smtpPort = int.Parse(configuration.GetValue<string>("Email:SmtpPort"));
            password = configuration.GetValue<string>("Email:Password");
        }

        public async Task SendEmailAsync(string email, string subject, string message,
            CancellationToken cancellationToken) {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Администрация сайта", companyMailAddress));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) {
                Text = message
            };

            using var client = new SmtpClient();
            //TODO SSL?
            await client.ConnectAsync(smtpAddress, smtpPort, false, cancellationToken);
            await client.AuthenticateAsync(companyMailAddress, password, cancellationToken);
            await client.SendAsync(emailMessage, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}
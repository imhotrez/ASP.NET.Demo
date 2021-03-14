using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Demo.Services.BusinessLogic {
    public class EmailService {
        private readonly string _companyMailAddress;
        private readonly string _smtpAddress;
        private readonly int _smtpPort;
        private readonly string _password;

        public EmailService(IConfiguration configuration) {
            _companyMailAddress = configuration.GetValue<string>("Email:CompanyMailAddress");
            _smtpAddress = configuration.GetValue<string>("Email:SmtpAddress");
            _smtpPort = int.Parse(configuration.GetValue<string>("Email:SmtpPort"));
            _password = configuration.GetValue<string>("Email:Password");
        }

        public async Task SendEmailAsync(string email, string subject, string message,
            CancellationToken cancellationToken) {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Администрация сайта", _companyMailAddress));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) {
                Text = message
            };

            using var client = new SmtpClient();
            //TODO SSL?
            await client.ConnectAsync(_smtpAddress, _smtpPort, false, cancellationToken);
            await client.AuthenticateAsync(_companyMailAddress, _password, cancellationToken);
            await client.SendAsync(emailMessage, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}
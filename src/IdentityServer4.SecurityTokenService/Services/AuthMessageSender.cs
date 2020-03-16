using System.Threading.Tasks;
using IdentityServer4.SecurityTokenService.Common;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace IdentityServer4.SecurityTokenService.Services
{
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private readonly ILogger _logger;
        private readonly STSOptions _options;
        private readonly IHostEnvironment _environment;

        public AuthMessageSender(ILogger<AuthMessageSender> logger,
            STSOptions options, IHostEnvironment environment)
        {
            _logger = logger;
            _options = options;
            _environment = environment;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            if (_environment.IsDevelopment())
            {
                _logger.LogInformation($"Email: {email} \r\n Subject: {subject} \r\n Message: {message}");
            }
            else
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(_options.EmailDisplayName, _options.EmailAccount));
                mimeMessage.To.Add(new MailboxAddress(email, email));
                mimeMessage.Subject = subject;
                mimeMessage.Body = new TextPart(TextFormat.Html) {Text = message};
                using var client = new SmtpClient();
                await client.ConnectAsync(_options.EmailHost, _options.EmailPort, _options.EmailEnableSSL);
                await client.AuthenticateAsync(_options.EmailAccount, _options.EmailPassword);
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
            }
        }

        public Task SendSmsAsync(string number, string message)
        {
            if (_environment.IsDevelopment())
            {
                _logger.LogInformation($"Phone: {number} \r\n Message: {message}");
                return Task.CompletedTask;
            }

            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
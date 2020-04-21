using System.Collections.Generic;
using System.Net.Http;
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
        private readonly IHttpClientFactory _clientFactory;

        public AuthMessageSender(ILogger<AuthMessageSender> logger,
            STSOptions options, IHostEnvironment environment, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _options = options;
            _environment = environment;
            _clientFactory = clientFactory;
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

        public async Task SendSmsAsync(string phoneNumber, string code)
        {
            var message = string.Format(_options.SmsCodeTemplate, code);
            if (_environment.IsDevelopment())
            {
                _logger.LogInformation($"Phone: {phoneNumber} \r\n Message: {message}");
            }
            else
            {
                var client = _clientFactory.CreateClient("SMS_Send");
                var dict = new Dictionary<string, string>()
                {
                    {"account", _options.SmsAccount},
                    {"pswd", _options.SmsPassword},
                    {"mobile", phoneNumber},
                    {"msg", message},
                    {"needstatus", "true"},
                    {"resptype", "json"}
                };
                using var body = new FormUrlEncodedContent(dict);
                await client.PostAsync(_options.SmsPostUrl, body);
            }
        }
    }
}
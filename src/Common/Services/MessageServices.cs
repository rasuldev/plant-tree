using System;
using System.Threading.Tasks;
using Common.Errors;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Common.Services
{
    public static class Services
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }
    }


    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public AuthMessageSender(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<AuthMessageSender>();
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var smtpServer = _configuration["email:server"];
            var port = int.Parse(_configuration["email:port"]);
            var login = _configuration["email:login"];
            var password = _configuration["email:password"];

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Administrator", login));
            mimeMessage.To.Add(new MailboxAddress("New user", email));
            mimeMessage.Subject = subject;

            mimeMessage.Body = new TextPart("html")
            {
                Text = message
            };

            try
            {
                using (var client = new SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect(smtpServer, port, true);
                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate(login, password);
                    await client.SendAsync(mimeMessage);
                    client.Disconnect(true);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Send mail error: \r\n" + e.ToString());
                var userError = new ApiUserError("Error occured while sending mail. Try again later.", ApiErrorCodes.MailSendError);
                var systemError = new ApiSystemError(e.ToString(), ApiErrorCodes.MailSendError);
                throw new ApiException(userError, systemError);
            }
        }

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}

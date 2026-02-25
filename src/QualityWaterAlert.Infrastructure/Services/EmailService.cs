using System.Net;
using System.Net.Mail;
using QualityWaterAlert.Infrastructure.Interfaces;

namespace QualityWaterAlert.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _fromEmail;
        private readonly string _senderName;
        private readonly string? _username;
        private readonly string? _password;
        private readonly bool _enableSsl;

        /// <summary>
        /// Initializes a new instance of the EmailService class with SMTP configuration.
        /// </summary>
        /// <param name="smtpServer">The SMTP server address (e.g., "smtp.gmail.com")</param>
        /// <param name="smtpPort">The SMTP server port (typically 587 for TLS or 465 for SSL)</param>
        /// <param name="fromEmail">The email address to send from</param>
        /// <param name="senderName">The display name for the sender</param>
        /// <param name="username">Optional username for SMTP authentication</param>
        /// <param name="password">Optional password for SMTP authentication</param>
        /// <param name="enableSsl">Whether to enable SSL/TLS (default: true)</param>
        public EmailService(
            string smtpServer,
            int smtpPort,
            string fromEmail,
            string senderName,
            string? username = null,
            string? password = null,
            bool enableSsl = true)
        {
            _smtpServer = smtpServer ?? throw new ArgumentNullException(nameof(smtpServer));
            _smtpPort = smtpPort;
            _fromEmail = fromEmail ?? throw new ArgumentNullException(nameof(fromEmail));
            _senderName = senderName ?? throw new ArgumentNullException(nameof(senderName));
            _username = username;
            _password = password;
            _enableSsl = enableSsl;
        }

        /// <summary>
        /// Sends an email to the specified recipient with the given subject and body.
        /// </summary>
        /// <param name="to">The recipient's email address</param>
        /// <param name="subject">The email subject</param>
        /// <param name="body">The email body (supports HTML)</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="ArgumentException">Thrown if the email address is null or empty</exception>
        /// <exception cref="SmtpException">Thrown if the SMTP operation fails</exception>
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentException("Recipient email address cannot be null or empty.", nameof(to));
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentException("Email subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentException("Email body cannot be null or empty.", nameof(body));
            }

            using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = _enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
            {
                smtpClient.Credentials = new NetworkCredential(_username, _password);
            }

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                throw new SmtpException($"Failed to send email to {to}. SMTP Server: {_smtpServer}:{_smtpPort}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An unexpected error occurred while sending email to {to}.", ex);
            }
        }
    }
}

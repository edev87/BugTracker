using BugTracker.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BugTracker.Services
{
    public class EmailService : IEmailSender
    {
		private readonly EmailSettings _emailSettings;

		public EmailService(IOptions<EmailSettings> emailSettings) {
			_emailSettings = emailSettings.Value;
		}
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
			try
			{
				var emailAddress = _emailSettings.EmailAddress ?? Environment.GetEnvironmentVariable("EmailAddress");
                var emailPassword = _emailSettings.EmailPassword ?? Environment.GetEnvironmentVariable("EmailPassword");
                var emailHost = _emailSettings.EmailHost ?? Environment.GetEnvironmentVariable("EmailAddress");
                var emailPort = _emailSettings.EmailPort != 0 ? _emailSettings.EmailPort : int.Parse(Environment.GetEnvironmentVariable("EmailPort")!);

				MimeMessage newEmail = new MimeMessage();

				//Attach the email
				newEmail.Sender = MailboxAddress.Parse(emailAddress);

				//Attah the email recipients
				foreach (string address in email.Split(";"))
				{
					newEmail.To.Add(MailboxAddress.Parse(address));
				}

				//Set the subject
				newEmail.Subject = subject;

				//Format the body
				BodyBuilder emailBody = new BodyBuilder();
				emailBody.HtmlBody = htmlMessage;
				newEmail.Body = emailBody.ToMessageBody();

				//prep the service and send the email
				using SmtpClient smtpClient = new SmtpClient();

				try
				{
					//setup connection, to go gmail.com
					await smtpClient.ConnectAsync(emailHost, emailPort, SecureSocketOptions.StartTls);
					//google autheitcation, login into gmail
					await smtpClient.AuthenticateAsync(emailAddress, emailPassword);
                    //send email
                    await smtpClient.SendAsync(newEmail);

					//Disconnect the connection, logout
                    await smtpClient.DisconnectAsync(true);
				}
				catch (Exception)
				{
					//var error = ex.Message;

					throw;
				}

            }
			catch (Exception)
			{

				throw;
			}
        }
    }
}

using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;  
using System.Threading.Tasks;
using JWT.DTO;
using JWT.Model.Settings;

namespace JWT.Services
{
    public class MailingService : IMailingServices
	{
		private readonly MailSettings _mailSettings;

	
		public MailingService(IOptions<MailSettings> mailSettings)
		{
			_mailSettings = mailSettings.Value;
		}

		public async Task SendEmailAsync(string ToMail, string Subject, string body)
		{
			var email = new MimeMessage();
			email.Sender = MailboxAddress.Parse(_mailSettings.Email);
			email.To.Add(MailboxAddress.Parse(ToMail));
			email.Subject = Subject;

			var builder = new BodyBuilder
			{
				HtmlBody = body
			};
			email.Body = builder.ToMessageBody();

			using (var smtp = new SmtpClient())
			{
				smtp.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
				await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
				await smtp.AuthenticateAsync(_mailSettings.Email, _mailSettings.Password);
				await smtp.SendAsync(email);
				await smtp.DisconnectAsync(true);
			}
		}
		
		



	}
}

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace HeyImIn.MailNotifier.Impl
{
	public class MailSender : IMailSender
	{
		public MailSender(ISendGridClient sendGridClient, ILogger<MailSender> logger)
		{
			_sendGridClient = sendGridClient;
			_logger = logger;
		}

		public Task SendMailAsync(string recipientEmail, string subject, string bodyText)
		{
			return SendMailAsync(new[] { recipientEmail }, subject, bodyText);
		}

		public async Task SendMailAsync(IReadOnlyCollection<string> recipientEmails, string subject, string bodyText)
		{
			var message = new SendGridMessage();
			message.SetFrom("no-reply@hey-im-in.ch", "Hey, I'm in");

			foreach (string recipient in recipientEmails)
			{
				message.AddTo(recipient);
			}

			message.SetSubject(subject);

			// If desired we could create a HTML based layout
			message.PlainTextContent = bodyText;

			Response sendGridResponse = await _sendGridClient.SendEmailAsync(message);

			// See https://stackoverflow.com/a/44634349
			if (!new HttpResponseMessage(sendGridResponse.StatusCode).IsSuccessStatusCode)
			{
				string responseBody = await sendGridResponse.Body.ReadAsStringAsync();
				_logger.LogError("{0}(subject={1}): Failed to send mail to recipients ({2}), statusCode={3}, errorBody={4}", nameof(SendMailAsync), subject, string.Join(";", recipientEmails), sendGridResponse.StatusCode, responseBody);
			}
		}

		private readonly ISendGridClient _sendGridClient;
		private readonly ILogger<MailSender> _logger;
	}
}

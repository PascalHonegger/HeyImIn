using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HeyImIn.Shared;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace HeyImIn.MailNotifier.Impl
{
	public class MailSender : IMailSender
	{
		public MailSender(ISendGridClient sendGridClient, HeyImInConfiguration configuration, ILogger<MailSender> logger)
		{
			_sendGridClient = sendGridClient;
			_senderMail = configuration.SenderEmailAddress;
			_senderName = configuration.SenderEmailName;
			_logger = logger;
		}

		public Task SendMailAsync(string recipientEmail, string subject, string bodyText)
		{
			return SendMailAsync(new[] { recipientEmail }, subject, bodyText);
		}

		public async Task SendMailAsync(IReadOnlyCollection<string> recipientEmails, string subject, string bodyText)
		{
			var message = new SendGridMessage();
			message.SetFrom(_senderMail, _senderName);

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
		private readonly string _senderMail;
		private readonly string _senderName;
		private readonly ILogger<MailSender> _logger;
	}
}

using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace HeyImIn.MailNotifier.Impl
{
	public class MailSender : IMailSender
	{
		public MailSender(ISendGridClient sendGridClient)
		{
			_sendGridClient = sendGridClient;
		}

		public Task SendMailAsync(string recipientEmail, string subject, string bodyText)
		{
			return SendMailAsync(new[] { recipientEmail }, subject, bodyText);
		}

		public async Task SendMailAsync(IReadOnlyCollection<string> recipientEmails, string subject, string bodyText)
		{
			var message = new SendGridMessage();
			message.SetFrom("no-reply@hey-im-in.ch", "Hey I'm in");

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
				_log.ErrorFormat("{0}(subject={1}): Failed to send mail to recipients ({2}), statusCode={3}, errorBody={4}", nameof(SendMailAsync), subject, string.Join(";", recipientEmails), sendGridResponse.StatusCode, responseBody);
			}
		}

		private readonly ISendGridClient _sendGridClient;
		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}

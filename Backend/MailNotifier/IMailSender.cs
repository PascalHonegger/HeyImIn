using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeyImIn.MailNotifier
{
	/// <summary>
	///     Abstraction of the SendGrid client which also contains the HeyImIn From-Email
	/// </summary>
	public interface IMailSender
	{
		/// <summary>
		///     Sends a mail to a single recipient
		///     Calls <see cref="SendMailAsync(IReadOnlyCollection{string},string,string)" /> with an array containing one email
		/// </summary>
		/// <param name="recipientEmail">Email to send to</param>
		/// <param name="subject">Plain text subject</param>
		/// <param name="bodyText">Plain text body</param>
		Task SendMailAsync(string recipientEmail, string subject, string bodyText);

		/// <summary>
		///     Sends a mail to multiple recipient
		/// </summary>
		/// <param name="recipientEmails">Emails to send to</param>
		/// <param name="subject">Plain text subject</param>
		/// <param name="bodyText">Plain text body</param>
		Task SendMailAsync(IReadOnlyCollection<string> recipientEmails, string subject, string bodyText);
	}
}

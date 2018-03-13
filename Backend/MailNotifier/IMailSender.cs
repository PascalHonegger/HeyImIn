using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeyImIn.MailNotifier
{
	public interface IMailSender
	{
		Task SendMailAsync(string recipient, string subject, string bodyText);
		Task SendMailAsync(IReadOnlyCollection<string> recipients, string subject, string bodyText);
	}
}

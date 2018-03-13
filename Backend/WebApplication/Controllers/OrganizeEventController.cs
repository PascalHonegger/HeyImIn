using System.Reflection;
using System.Web.Http;
using HeyImIn.Database.Context;
using HeyImIn.MailNotifier;
using HeyImIn.WebApplication.Helpers;
using log4net;

namespace HeyImIn.WebApplication.Controllers
{
	public class OrganizeEventController : ApiController
	{
		public OrganizeEventController(INotificationService notificationService, GetDatabaseContext getDatabaseContext)
		{
			_notificationService = notificationService;
			_getDatabaseContext = getDatabaseContext;
		}

		private readonly INotificationService _notificationService;
		private readonly GetDatabaseContext _getDatabaseContext;

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ILog _auditLog = LogHelpers.GetAuditLog();
	}
}

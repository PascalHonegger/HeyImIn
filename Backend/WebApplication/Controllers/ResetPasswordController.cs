using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using HeyImIn.Authentication;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using HeyImIn.WebApplication.FrontendModels;
using log4net;

namespace HeyImIn.WebApplication.Controllers
{
	public class ResetPasswordController : ApiController
	{
		public ResetPasswordController(IPasswordService passwordService, INotificationService notificationService, GetDatabaseContext getDatabaseContext)
		{
			_passwordService = passwordService;
			_notificationService = notificationService;
			_getDatabaseContext = getDatabaseContext;
		}

		/// <summary>
		///     Sends a password reset code to the provided email address, if a user with than email is registered
		/// </summary>
		[HttpPost]
		[ResponseType(typeof(void))]
		[AllowAnonymous]
		public async Task<IHttpActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto requestPasswordResetDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (requestPasswordResetDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				User user = await context.Users.FindAsync(requestPasswordResetDto.Email);

				if (user == null)
				{
					return BadRequest("Für diese E-Mail-Adresse ist kein Konto hinterlegt");
				}

				PasswordReset passwordReset = context.PasswordResets.Create();
				passwordReset.User = user;
				passwordReset.Requested = DateTime.UtcNow;

				context.PasswordResets.Add(passwordReset);

				await context.SaveChangesAsync();

				await _notificationService.SendPasswordResetTokenAsync(passwordReset.Token, user);

				return Ok();
			}
		}

		/// <summary>
		///     Sets a new password for a password reset code
		/// </summary>
		[HttpPost]
		[ResponseType(typeof(void))]
		[AllowAnonymous]
		public async Task<IHttpActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (resetPasswordDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				PasswordReset passwordReset = await context.PasswordResets.FindAsync(resetPasswordDto.PasswordResetToken);

				if (passwordReset == null)
				{
					_log.DebugFormat("{0}(resetToken={1}): Couldn't find the password reset token", nameof(ResetPassword), resetPasswordDto.PasswordResetToken);

					return BadRequest("Der angegebene Code ist ungültig");
				}

				if (passwordReset.Used || (passwordReset.Requested - DateTime.UtcNow > _resetTokenValidTimeSpan))
				{
					_log.InfoFormat("{0}(resetToken={1:D}): Tried to reset password for user {2} with an expired or used token", nameof(ResetPassword), resetPasswordDto.PasswordResetToken, passwordReset.UserId);

					return BadRequest("Der angegebene Code ist abgelaufen oder wurde bereits verwendet");
				}

				passwordReset.User.PasswordHash = _passwordService.HashPassword(resetPasswordDto.NewPassword);
				passwordReset.Used = true;

				await context.SaveChangesAsync();

				_log.InfoFormat("{0}(resetToken={1}): Reset password for user {2}", nameof(ResetPassword), resetPasswordDto.PasswordResetToken, passwordReset.UserId);

				return Ok();
			}
		}

		private readonly IPasswordService _passwordService;
		private readonly INotificationService _notificationService;
		private readonly GetDatabaseContext _getDatabaseContext;

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly TimeSpan _resetTokenValidTimeSpan = TimeSpan.FromHours(2);
	}
}

using System;
using System.Threading.Tasks;
using HeyImIn.Authentication;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using HeyImIn.Shared;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HeyImIn.WebApplication.Controllers
{
	[AllowAnonymous]
	[ApiController]
	[Route("api/ResetPassword")]
	public class ResetPasswordController : ControllerBase
	{
		public ResetPasswordController(IPasswordService passwordService, INotificationService notificationService, HeyImInConfiguration configuration, GetDatabaseContext getDatabaseContext, ILogger<ResetPasswordController> logger)
		{
			_passwordService = passwordService;
			_notificationService = notificationService;
			_resetTokenValidTimeSpan = configuration.Timeouts.PasswordResetTimeout;
			_getDatabaseContext = getDatabaseContext;
			_logger = logger;
		}

		/// <summary>
		///     Sends a password reset code to the provided email address, if a user with than email is registered
		/// </summary>
		[HttpPost(nameof(RequestPasswordReset))]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetDto requestPasswordResetDto)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				User user = await context.Users.FirstOrDefaultAsync(u => u.Email == requestPasswordResetDto.Email);

				if (user == null)
				{
					return BadRequest(RequestStringMessages.NoProfileWithEmailFound);
				}

				var passwordReset = new PasswordReset
				{
					User = user,
					Requested = DateTime.UtcNow
				};

				context.PasswordResets.Add(passwordReset);

				await context.SaveChangesAsync();

				await _notificationService.SendPasswordResetTokenAsync(passwordReset.Token, user);

				return Ok();
			}
		}

		/// <summary>
		///     Sets a new password for a password reset code
		/// </summary>
		[HttpPost(nameof(ResetPassword))]
		[ProducesResponseType(typeof(void), 200)]
		public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				PasswordReset passwordReset = await context.PasswordResets.Include(p => p.User).FirstOrDefaultAsync(r => r.Token == resetPasswordDto.PasswordResetToken);

				if (passwordReset == null)
				{
					_logger.LogDebug("{0}(resetToken={1}): Couldn't find the password reset token", nameof(ResetPassword), resetPasswordDto.PasswordResetToken);

					return BadRequest(RequestStringMessages.ResetCodeInvalid);
				}

				if (passwordReset.Used || (passwordReset.Requested - DateTime.UtcNow > _resetTokenValidTimeSpan))
				{
					_logger.LogInformation("{0}(resetToken={1}): Tried to reset password for user {2} with an expired or used token", nameof(ResetPassword), resetPasswordDto.PasswordResetToken, passwordReset.UserId);

					return BadRequest(RequestStringMessages.ResetCodeAlreadyUsed);
				}

				passwordReset.User.PasswordHash = _passwordService.HashPassword(resetPasswordDto.NewPassword);
				passwordReset.Used = true;

				await context.SaveChangesAsync();

				_logger.LogInformation("{0}(resetToken={1}): Reset password for user {2}", nameof(ResetPassword), resetPasswordDto.PasswordResetToken, passwordReset.UserId);

				return Ok();
			}
		}

		private readonly IPasswordService _passwordService;
		private readonly INotificationService _notificationService;
		private readonly GetDatabaseContext _getDatabaseContext;

		private readonly TimeSpan _resetTokenValidTimeSpan;
		private readonly ILogger<ResetPasswordController> _logger;
	}
}

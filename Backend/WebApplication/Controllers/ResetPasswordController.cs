﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using HeyImIn.Authentication;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.Helpers;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeyImIn.WebApplication.Controllers
{
	public class ResetPasswordController : Controller
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
		[ProducesResponseType(typeof(void), 200)]
		[AllowAnonymous]
		public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto requestPasswordResetDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (requestPasswordResetDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				User user = await context.Users.FirstOrDefaultAsync(u => u.Email == requestPasswordResetDto.Email);

				if (user == null)
				{
					return BadRequest(RequestStringMessages.NoProfileWithEmailFound);
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
		[ProducesResponseType(typeof(void), 200)]
		[AllowAnonymous]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
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

					return BadRequest(RequestStringMessages.ResetCodeInvalid);
				}

				if (passwordReset.Used || (passwordReset.Requested - DateTime.UtcNow > _resetTokenValidTimeSpan))
				{
					_log.InfoFormat("{0}(resetToken={1}): Tried to reset password for user {2} with an expired or used token", nameof(ResetPassword), resetPasswordDto.PasswordResetToken, passwordReset.UserId);

					return BadRequest(RequestStringMessages.ResetCodeAlreadyUsed);
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

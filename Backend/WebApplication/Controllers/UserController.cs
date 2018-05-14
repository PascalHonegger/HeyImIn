﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HeyImIn.Authentication;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using HeyImIn.MailNotifier.Models;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.FrontendModels.ResponseTypes;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.Services;
using HeyImIn.WebApplication.WebApiComponents;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeyImIn.WebApplication.Controllers
{
	[ApiController]
	[Route("api/User")]
	public class UserController : ControllerBase
	{
		public UserController(IPasswordService passwordService, ISessionService sessionService, INotificationService notificationService, IDeleteService deleteService, GetDatabaseContext getDatabaseContext)
		{
			_passwordService = passwordService;
			_sessionService = sessionService;
			_notificationService = notificationService;
			_deleteService = deleteService;
			_getDatabaseContext = getDatabaseContext;
		}

		/// <summary>
		///     Updates the current user's data
		///     Doesn't invalidate any active sessions
		/// </summary>
		/// <param name="setUserDataDto">New user data</param>
		[HttpPost(nameof(SetNewUserData))]
		[ProducesResponseType(typeof(void), 200)]
		[AuthenticateUser]
		public async Task<IActionResult> SetNewUserData(SetUserDataDto setUserDataDto)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				User currentUser = await HttpContext.GetCurrentUserAsync(context);

				if (currentUser.Email != setUserDataDto.NewEmail)
				{
					bool newMailTaken = await context.Users.AnyAsync(u => u.Email == setUserDataDto.NewEmail);
					if (newMailTaken)
					{
						return BadRequest(RequestStringMessages.EmailAlreadyInUse);
					}
				}


				currentUser.Email = setUserDataDto.NewEmail;
				currentUser.FullName = setUserDataDto.NewFullName;

				await context.SaveChangesAsync();

				_log.InfoFormat("{0}(): Updated user data", nameof(SetNewUserData));

				return Ok();
			}
		}

		/// <summary>
		///     Changes the current user's password
		///     Doesn't invalidate any active sessions
		/// </summary>
		/// <param name="setPasswordDto">Current and new password</param>
		[HttpPost(nameof(SetNewPassword))]
		[ProducesResponseType(typeof(void), 200)]
		[AuthenticateUser]
		public async Task<IActionResult> SetNewPassword(SetPasswordDto setPasswordDto)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				User currentUser = await HttpContext.GetCurrentUserAsync(context);

				bool currentPasswordCorrect = _passwordService.VerifyPassword(setPasswordDto.CurrentPassword, currentUser.PasswordHash);

				if (!currentPasswordCorrect)
				{
					return BadRequest(RequestStringMessages.CurrentPasswordWrong);
				}

				currentUser.PasswordHash = _passwordService.HashPassword(setPasswordDto.NewPassword);

				await context.SaveChangesAsync();

				_log.InfoFormat("{0}(): User changed his password", nameof(SetNewPassword));

				return Ok();
			}
		}

		/// <summary>
		///     Deletes the current user's account and all connections
		///     E.g. sends cancelation for organized and participating events
		/// </summary>
		[HttpDelete(nameof(DeleteAccount))]
		[ProducesResponseType(typeof(void), 200)]
		[AuthenticateUser]
		public async Task<IActionResult> DeleteAccount()
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				User currentUser = await HttpContext.GetCurrentUserAsync(context);

				// Appointments the user participates, excluding his organized events
				List<Appointment> userAppointments = currentUser.AppointmentParticipations
					.Select(a => a.Appointment)
					.Where(a => a.Event.OrganizerId != currentUser.Id)
					.ToList();

				List<EventNotificationInformation> notificationInformations = _deleteService.DeleteUserLocally(context, currentUser);

				await context.SaveChangesAsync();

				_auditLog.InfoFormat("{0}(): Deleted user {1} ({2}) and all of his events", nameof(DeleteAccount), currentUser.Id, currentUser.FullName);

				foreach (EventNotificationInformation notificationInformation in notificationInformations)
				{
					await _notificationService.NotifyEventDeletedAsync(notificationInformation);
				}

				foreach (Appointment appointment in userAppointments)
				{
					await _notificationService.SendLastMinuteChangeIfRequiredAsync(appointment);
				}

				return Ok();
			}
		}

		/// <summary>
		///     Registeres a new user
		/// </summary>
		/// <returns>A newly created <see cref="FrontendSession" /> so the registering user doesn't have to log in manually</returns>
		[HttpPost(nameof(Register))]
		[ProducesResponseType(typeof(FrontendSession), 200)]
		[AllowAnonymous]
		public async Task<IActionResult> Register(RegisterDto registerDto)
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				bool userWithSameMailExists = await context.Users.AnyAsync(u => u.Email == registerDto.Email);

				if (userWithSameMailExists)
				{
					return BadRequest(RequestStringMessages.EmailAlreadyInUse);
				}

				var newUser = new User
				{
					FullName = registerDto.FullName,
					Email = registerDto.Email,
					PasswordHash = _passwordService.HashPassword(registerDto.Password)
				};

				context.Users.Add(newUser);

				await context.SaveChangesAsync();

				_auditLog.InfoFormat("{0}(): Registered user {1} ({2})", nameof(Register), newUser.Id, newUser.FullName);

				Guid createdSessionToken = await _sessionService.CreateSessionAsync(newUser.Id, true);

				return Ok(new FrontendSession(createdSessionToken, newUser.Id, registerDto.FullName, registerDto.Email));
			}
		}

		private readonly IPasswordService _passwordService;
		private readonly ISessionService _sessionService;
		private readonly INotificationService _notificationService;
		private readonly IDeleteService _deleteService;
		private readonly GetDatabaseContext _getDatabaseContext;

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ILog _auditLog = LogHelpers.GetAuditLog(MethodBase.GetCurrentMethod().DeclaringType);
	}
}

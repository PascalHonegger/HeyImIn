using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using HeyImIn.Authentication;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.FrontendModels.ResponseTypes;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.WebApiComponents;
using log4net;

namespace HeyImIn.WebApplication.Controllers
{
	public class UserController : ApiController
	{
		public UserController(IPasswordService passwordService, ISessionService sessionService, INotificationService notificationService, GetDatabaseContext getDatabaseContext)
		{
			_passwordService = passwordService;
			_sessionService = sessionService;
			_notificationService = notificationService;
			_getDatabaseContext = getDatabaseContext;
		}

		/// <summary>
		///     Updates the current user's data
		///     Doesn't invalidate any active sessions
		/// </summary>
		/// <param name="setUserDataDto">New user data</param>
		[HttpPost]
		[ResponseType(typeof(void))]
		[AuthenticateUser]
		public async Task<IHttpActionResult> SetNewUserData([FromBody] SetUserDataDto setUserDataDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (setUserDataDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

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
		[HttpPost]
		[ResponseType(typeof(void))]
		[AuthenticateUser]
		public async Task<IHttpActionResult> SetNewPassword([FromBody] SetPasswordDto setPasswordDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (setPasswordDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

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
		[HttpDelete]
		[ResponseType(typeof(void))]
		[AuthenticateUser]
		public async Task<IHttpActionResult> DeleteAccount()
		{
			using (IDatabaseContext context = _getDatabaseContext())
			{
				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

				// Token based relations
				context.Sessions.RemoveRange(currentUser.Sessions);
				context.PasswordResets.RemoveRange(currentUser.PasswordResets);

				// Events the user is part of
				List<Event> organizedEvents = currentUser.OrganizedEvents.ToList();
				List<(Event Event, List<User> Participations)> eventWithParticipations = organizedEvents.Select(e => (Event: e, Participations: e.EventParticipations.Select(ep => ep.Participant).ToList())).ToList();
				context.EventParticipations.RemoveRange(currentUser.EventParticipations);

				// Appointments the user is part of
				List<AppointmentParticipation> userAppointmentParticipations = currentUser.AppointmentParticipations.ToList();
				List<Appointment> userAppointments = userAppointmentParticipations.Select(a => a.Appointment).ToList();
				context.AppointmentParticipations.RemoveRange(userAppointmentParticipations);

				// Events the user organized
				List<EventParticipation> organizedParticipations = organizedEvents.SelectMany(o => o.EventParticipations).ToList();
				List<EventInvitation> organizedInvitations = organizedEvents.SelectMany(o => o.EventInvitations).ToList();
				List<Appointment> organizedAppointments = organizedEvents.SelectMany(o => o.Appointments).ToList();
				List<AppointmentParticipation> organizedAppointmentParticipations = organizedAppointments.SelectMany(o => o.AppointmentParticipations).ToList();
				context.EventParticipations.RemoveRange(organizedParticipations);
				context.EventInvitations.RemoveRange(organizedInvitations);
				context.Appointments.RemoveRange(organizedAppointments);
				context.AppointmentParticipations.RemoveRange(organizedAppointmentParticipations);
				context.Events.RemoveRange(organizedEvents);

				// User himself
				context.Users.Remove(currentUser);

				await context.SaveChangesAsync();

				_auditLog.InfoFormat("{0}(): Deleted user {1} ({2}) and all of his events", nameof(DeleteAccount), currentUser.Id, currentUser.FullName);

				foreach (var (@event, participations) in eventWithParticipations)
				{
					await _notificationService.NotifyEventDeletedAsync(@event.Title, participations);
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
		[HttpPost]
		[ResponseType(typeof(FrontendSession))]
		[AllowAnonymous]
		public async Task<IHttpActionResult> Register([FromBody] RegisterDto registerDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (registerDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				bool userWithSameMailExists = await context.Users.AnyAsync(u => u.Email == registerDto.Email);

				if (userWithSameMailExists)
				{
					return BadRequest(RequestStringMessages.EmailAlreadyInUse);
				}

				User newUser = context.Users.Create();
				newUser.FullName = registerDto.FullName;
				newUser.Email = registerDto.Email;
				newUser.PasswordHash = _passwordService.HashPassword(registerDto.Password);

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
		private readonly GetDatabaseContext _getDatabaseContext;

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ILog _auditLog = LogHelpers.GetAuditLog();
	}
}

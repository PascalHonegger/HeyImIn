﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.MailNotifier;
using HeyImIn.WebApplication.FrontendModels.ParameterTypes;
using HeyImIn.WebApplication.Helpers;
using HeyImIn.WebApplication.WebApiComponents;
using log4net;

namespace HeyImIn.WebApplication.Controllers
{
	[AuthenticateUser]
	public class InviteToEventController : ApiController
	{
		public InviteToEventController(INotificationService notificationService, GetDatabaseContext getDatabaseContext)
		{
			_notificationService = notificationService;
			_getDatabaseContext = getDatabaseContext;
		}

		/// <summary>
		///     Adds new appointsments to the event
		/// </summary>
		[HttpPost]
		[ResponseType(typeof(void))]
		public async Task<IHttpActionResult> InviteParticipants([FromBody] InviteParticipantsDto inviteParticipantsDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (inviteParticipantsDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				Event @event = await context.Events
					.Include(e => e.Organizer)
					.Include(e => e.EventInvitations)
					.FirstOrDefaultAsync(e => e.Id == inviteParticipantsDto.EventId);

				if (@event == null)
				{
					return BadRequest(RequestStringMessages.EventNotFound);
				}

				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

				if (@event.Organizer != currentUser)
				{
					_log.InfoFormat("{0}(): Tried to add appointments to the event {1}, which he's not organizing", nameof(InviteParticipants), @event.Id);

					return BadRequest(RequestStringMessages.OrganizorRequired);
				}

				var mailInvites = new List<(string email, EventInvitation invite)>();

				foreach (string emailAddress in inviteParticipantsDto.EmailAddresses)
				{
					EventInvitation invite = context.EventInvitations.Create();
					invite.Requested = DateTime.UtcNow;
					invite.Event = @event;
					context.EventInvitations.Add(invite);

					mailInvites.Add((emailAddress, invite));
				}

				await context.SaveChangesAsync();

				await _notificationService.SendInvitationLinkAsync(mailInvites);

				return Ok();
			}
		}


		[HttpPost]
		[ResponseType(typeof(void))]
		public async Task<IHttpActionResult> AcceptInvitation([FromBody] AcceptInvitationDto acceptInvitationDto)
		{
			// Validate parameters
			if (!ModelState.IsValid || (acceptInvitationDto == null))
			{
				return BadRequest();
			}

			using (IDatabaseContext context = _getDatabaseContext())
			{
				EventInvitation invitation = await context.EventInvitations.Include(e => e.Event).FirstOrDefaultAsync(e => e.Token == acceptInvitationDto.InviteToken);

				if (invitation == null)
				{
					return BadRequest(RequestStringMessages.InvitationInvalid);
				}

				if (invitation.Event.IsPrivate)
				{
					// Invitations to private events require to still be valid and not already used

					if (invitation.Used)
					{
						return BadRequest(RequestStringMessages.InvitationAlreadyUsed);
					}

					if (invitation.Requested + _inviteTimeout < DateTime.UtcNow)
					{
						return BadRequest(RequestStringMessages.InvitationInvalid);
					}
				}

				// Invitations to public events are theoretically pointless as the user could join without the invitation
				// => These invitations are always considered as valid, even if already used


				User currentUser = await ActionContext.Request.GetCurrentUserAsync(context);

				EventParticipation participation = context.EventParticipations.Create();
				participation.Event = invitation.Event;
				participation.Participant = currentUser;

				invitation.Event.EventParticipations.Add(participation);

				await context.SaveChangesAsync();

				return Ok();
			}
		}

		private readonly INotificationService _notificationService;
		private readonly GetDatabaseContext _getDatabaseContext;

		/// <summary>
		///     A invite for a private event gets invalidated after this time period passed
		/// </summary>
		private static readonly TimeSpan _inviteTimeout = TimeSpan.FromDays(7);

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}

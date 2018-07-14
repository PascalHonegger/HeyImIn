using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.WebApplication.Controllers;
using HeyImIn.WebApplication.FrontendModels.ResponseTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xunit;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public partial class ParticipateEventControllerTests
	{
		[Fact]
		public async Task GetOverview_GivenSomeEvents_OnlyShowsPublicEvents()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int publicEventId1;
			int publicEventId2;
			int privateEventId;
			int johnId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());
				User organizer = ContextUtilities.CreateRichardRoe();
				Event publicEvent1 = DummyEvent(organizer);
				Event publicEvent2 = DummyEvent(organizer);
				Event privateEvent = DummyEvent(organizer, true);
				context.Events.Add(publicEvent1);
				context.Events.Add(publicEvent2);
				context.Events.Add(privateEvent);

				await context.SaveChangesAsync();

				publicEventId1 = publicEvent1.Id;
				publicEventId2 = publicEvent2.Id;
				privateEventId = privateEvent.Id;
				johnId = john.Entity.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnId);

			IActionResult response = await participateEventController.GetOverview();

			// Assert
			Assert.IsType<OkObjectResult>(response);
			var okObjectResult = (OkObjectResult)response;
			var eventOverview = okObjectResult.Value as EventOverview;
			Assert.NotNull(eventOverview);
			Assert.Empty(eventOverview.YourEvents);
			Assert.Equal(2, eventOverview.PublicEvents.Count);
			Assert.Contains(publicEventId1, eventOverview.PublicEvents.Select(e => e.EventId));
			Assert.Contains(publicEventId2, eventOverview.PublicEvents.Select(e => e.EventId));
			Assert.DoesNotContain(privateEventId, eventOverview.PublicEvents.Select(e => e.EventId));
		}

		[Fact]
		public async Task GetOverview_GivenOrganizedEvents_ShowsYourEvents()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int yourPublicEventId;
			int yourPrivateEventId;
			int johnId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());
				Event yourPublicEvent1 = DummyEvent(john.Entity);
				Event yourPrivateEvent2 = DummyEvent(john.Entity, true);
				context.Events.Add(yourPublicEvent1);
				context.Events.Add(yourPrivateEvent2);

				await context.SaveChangesAsync();

				yourPublicEventId = yourPublicEvent1.Id;
				yourPrivateEventId = yourPrivateEvent2.Id;
				johnId = john.Entity.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnId);

			IActionResult response = await participateEventController.GetOverview();

			// Assert
			Assert.IsType<OkObjectResult>(response);
			var okObjectResult = (OkObjectResult)response;
			var eventOverview = okObjectResult.Value as EventOverview;
			Assert.NotNull(eventOverview);
			Assert.Empty(eventOverview.PublicEvents);
			Assert.Equal(2, eventOverview.YourEvents.Count);
			Assert.Contains(yourPublicEventId, eventOverview.YourEvents.Select(e => e.EventId));
			Assert.Contains(yourPrivateEventId, eventOverview.YourEvents.Select(e => e.EventId));
		}

		[Fact]
		public async Task GetOverview_GivenParticipatingEvents_ShowsYourAndPublicEvents()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);
			int richardsPublicEventId;
			int richardsPrivateEventId;
			int richardsPublicEventParticipatingId;
			int richardsPrivateEventParticipatingId;
			int johnId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> john = context.Users.Add(ContextUtilities.CreateJohnDoe());
				User richard = ContextUtilities.CreateRichardRoe();

				Event richardsPublicEvent = DummyEvent(richard);
				Event richardsPrivateEvent = DummyEvent(richard, true);

				Event richardsPublicEventParticipating = DummyEvent(richard);
				Event richardsPrivateEventParticipating = DummyEvent(richard, true);

				context.Events.Add(richardsPublicEvent);
				context.Events.Add(richardsPrivateEvent);

				context.Events.Add(richardsPublicEventParticipating);
				context.Events.Add(richardsPrivateEventParticipating);

				context.EventParticipations.Add(new EventParticipation { Participant = john.Entity, Event = richardsPublicEventParticipating });
				context.EventParticipations.Add(new EventParticipation { Participant = john.Entity, Event = richardsPrivateEventParticipating });

				await context.SaveChangesAsync();

				richardsPublicEventId = richardsPublicEvent.Id;
				richardsPrivateEventId = richardsPrivateEvent.Id;
				richardsPublicEventParticipatingId = richardsPublicEventParticipating.Id;
				richardsPrivateEventParticipatingId = richardsPrivateEventParticipating.Id;
				johnId = john.Entity.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnId);

			IActionResult response = await participateEventController.GetOverview();

			// Assert
			Assert.IsType<OkObjectResult>(response);
			var okObjectResult = (OkObjectResult)response;
			var eventOverview = okObjectResult.Value as EventOverview;
			Assert.NotNull(eventOverview);
			Assert.Single(eventOverview.PublicEvents);
			Assert.Equal(2, eventOverview.YourEvents.Count);
			Assert.Contains(richardsPublicEventId, eventOverview.PublicEvents.Select(e => e.EventId));
			Assert.DoesNotContain(richardsPrivateEventId, eventOverview.PublicEvents.Select(e => e.EventId));
			Assert.Contains(richardsPublicEventParticipatingId, eventOverview.YourEvents.Select(e => e.EventId));
			Assert.Contains(richardsPrivateEventParticipatingId, eventOverview.YourEvents.Select(e => e.EventId));
		}

		[Fact]
		public async Task GetOverview_GivenSomeEvents_EventsSortedByDateOfUpcomingAppointment()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);

			(Event eventWithoutAppointments, Event eventWithSoonerAppointment, Event eventWithLaterAppointment) yourEvents;
			(Event eventWithoutAppointments, Event eventWithSoonerAppointment, Event eventWithLaterAppointment) publicEvents;

			int johnId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();
				User richard = ContextUtilities.CreateRichardRoe();

				yourEvents = CreateEvents(context, john);
				publicEvents = CreateEvents(context, richard);

				await context.SaveChangesAsync();

				johnId = john.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnId);

			IActionResult response = await participateEventController.GetOverview();

			// Assert
			Assert.IsType<OkObjectResult>(response);
			var okObjectResult = (OkObjectResult)response;
			var eventOverview = okObjectResult.Value as EventOverview;
			Assert.NotNull(eventOverview);
			AssertEventOrder(eventOverview.YourEvents, yourEvents);
			AssertEventOrder(eventOverview.PublicEvents, publicEvents);

			(Event eventWithoutAppointments, Event eventWithSoonerAppointment, Event eventWithLaterAppointment) CreateEvents(IDatabaseContext context, User organizer)
			{
				Event eventWithoutAppointments = DummyEvent(organizer);
				Event eventWithSoonerAppointment = DummyEvent(organizer);
				Event eventWithLaterAppointment = DummyEvent(organizer);

				context.Events.Add(eventWithLaterAppointment);
				context.Events.Add(eventWithSoonerAppointment);
				context.Events.Add(eventWithoutAppointments);

				context.Appointments.Add(new Appointment { Event = eventWithSoonerAppointment, StartTime = DateTime.UtcNow + TimeSpan.FromHours(1) });
				context.Appointments.Add(new Appointment { Event = eventWithLaterAppointment, StartTime = DateTime.UtcNow + TimeSpan.FromHours(2) });

				return (eventWithoutAppointments, eventWithSoonerAppointment, eventWithLaterAppointment);
			}

			void AssertEventOrder(List<EventOverviewInformation> loadedEvents, (Event eventWithoutAppointments, Event eventWithSoonerAppointment, Event eventWithLaterAppointment) providedEvents)
			{
				Assert.Equal(3, loadedEvents.Count);
				Assert.Equal(providedEvents.eventWithSoonerAppointment.Id, loadedEvents[0].EventId);
				Assert.Equal(providedEvents.eventWithLaterAppointment.Id, loadedEvents[1].EventId);
				Assert.Equal(providedEvents.eventWithoutAppointments.Id, loadedEvents[2].EventId);
			}
		}

		[Fact]
		public async Task GetOverview_GivenEventWithAppointments_AppointmentDetailsCorrect()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);

			DateTime expectedDateTime = DateTime.UtcNow + TimeSpan.FromDays(1);
			Event yourEvent;
			Event publicEvent;
			int johnId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();
				User richard = ContextUtilities.CreateRichardRoe();

				yourEvent = CreateEventWithAppointments(context, john, expectedDateTime);
				publicEvent = CreateEventWithAppointments(context, richard, expectedDateTime);

				await context.SaveChangesAsync();

				johnId = john.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnId);

			IActionResult response = await participateEventController.GetOverview();

			// Assert
			Assert.IsType<OkObjectResult>(response);
			var okObjectResult = (OkObjectResult)response;
			var eventOverview = okObjectResult.Value as EventOverview;
			Assert.NotNull(eventOverview);
			AssertAppointmentSummary(eventOverview.YourEvents, yourEvent);
			AssertAppointmentSummary(eventOverview.PublicEvents, publicEvent);

			Event CreateEventWithAppointments(IDatabaseContext context, User organizer, DateTime dateOfNewestAppointment)
			{
				Event @event = DummyEvent(organizer);

				context.Events.Add(@event);
				context.Appointments.Add(new Appointment { Event = @event, StartTime = dateOfNewestAppointment + TimeSpan.FromSeconds(1) });
				context.Appointments.Add(new Appointment { Event = @event, StartTime = dateOfNewestAppointment });
				context.Appointments.Add(new Appointment { Event = @event, StartTime = dateOfNewestAppointment + TimeSpan.FromSeconds(2) });

				return @event;
			}

			// ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
			void AssertAppointmentSummary(IReadOnlyList<EventOverviewInformation> loadedEvents, Event expectedEvent)
			{
				Assert.Single(loadedEvents);
				EventOverviewInformation loadedYourEvent = loadedEvents[0];
				Assert.Equal(expectedEvent.Id, loadedYourEvent.EventId);
				Assert.Equal(expectedDateTime, loadedYourEvent.LatestAppointmentInformation.StartTime);
			}
		}

		[Fact]
		public async Task GetOverview_GivenAppointmentWithParticipation_SummaryCountsCorrect()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext(_output);

			Event yourEvent;
			int johnId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				User john = ContextUtilities.CreateJohnDoe();
				User participantWithExplicitNoAnswer = ContextUtilities.CreateRandomUser();
				User participantAccepted = ContextUtilities.CreateRandomUser();
				User participantDeclined = ContextUtilities.CreateRandomUser();

				yourEvent = DummyEvent(john);

				context.Events.Add(yourEvent);
				context.EventParticipations.Add(new EventParticipation { Participant = john, Event = yourEvent });
				context.EventParticipations.Add(new EventParticipation { Participant = participantWithExplicitNoAnswer, Event = yourEvent });
				context.EventParticipations.Add(new EventParticipation { Participant = participantAccepted, Event = yourEvent });
				context.EventParticipations.Add(new EventParticipation { Participant = participantDeclined, Event = yourEvent });

				EntityEntry<Appointment> appointmentEntry = context.Appointments.Add(new Appointment { Event = yourEvent, StartTime = DateTime.UtcNow + TimeSpan.FromDays(1) });
				context.AppointmentParticipations.Add(new AppointmentParticipation { Participant = participantWithExplicitNoAnswer, Appointment = appointmentEntry.Entity, AppointmentParticipationAnswer = null });
				context.AppointmentParticipations.Add(new AppointmentParticipation { Participant = participantAccepted, Appointment = appointmentEntry.Entity, AppointmentParticipationAnswer = AppointmentParticipationAnswer.Accepted });
				context.AppointmentParticipations.Add(new AppointmentParticipation { Participant = participantDeclined, Appointment = appointmentEntry.Entity, AppointmentParticipationAnswer = AppointmentParticipationAnswer.Declined });

				await context.SaveChangesAsync();

				johnId = john.Id;
			}

			// Act
			(ParticipateEventController participateEventController, _) = CreateController(getContext, johnId);

			IActionResult response = await participateEventController.GetOverview();

			// Assert
			Assert.IsType<OkObjectResult>(response);
			var okObjectResult = (OkObjectResult)response;
			var eventOverview = okObjectResult.Value as EventOverview;
			Assert.NotNull(eventOverview);
			Assert.Empty(eventOverview.PublicEvents);
			Assert.Single(eventOverview.YourEvents);
			EventOverviewInformation loadedYourEvent = eventOverview.YourEvents[0];
			Assert.Equal(yourEvent.Id, loadedYourEvent.EventId);
			Assert.Equal(4, loadedYourEvent.ViewEventInformation.TotalParticipants);
			Assert.NotNull(loadedYourEvent.LatestAppointmentInformation);
			Assert.Equal(1, loadedYourEvent.LatestAppointmentInformation.AcceptedParticipants);
			Assert.Equal(1, loadedYourEvent.LatestAppointmentInformation.DeclinedParticipants);
			Assert.Equal(2, loadedYourEvent.LatestAppointmentInformation.NotAnsweredParticipants);
		}
	}
}

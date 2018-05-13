using System;
using HeyImIn.Database.Models;
using HeyImIn.Shared.Tests;
using HeyImIn.WebApplication.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HeyImIn.WebApplication.Tests.Controllers
{
	public abstract class ControllerTestBase : TestBase
	{
		protected ControllerTestBase(ITestOutputHelper output) : base(output)
		{
		}

		protected static ControllerContext CurrentUserContext(int? johnDoeId, Guid sessionToken = default)
		{
			var context =  new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};

			if (johnDoeId.HasValue)
			{
				context.HttpContext.SetUserId(johnDoeId.Value);
			}

			context.HttpContext.SetSessionToken(sessionToken);

			return context;
		}

		protected static Event DummyEvent(User organizer, bool isPrivate = false)
		{
			return new Event
			{
				Title = "Fancy event",
				Description = "Very fancy, much wow",
				MeetingPlace = "Somewhere",
				Organizer = organizer,
				IsPrivate = isPrivate,
				ReminderTimeWindowInHours = 10,
				SummaryTimeWindowInHours = 5
			};
		}
	}
}

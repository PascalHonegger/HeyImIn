using System;
using System.Threading.Tasks;
using HeyImIn.Authentication.Impl;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xunit;

namespace HeyImIn.Authentication.Tests
{
	public class SessionServiceTests
	{
		[Fact]
		public async Task CreateActiveSessionIsActivated()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext();
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> userEntry = await context.Users.AddAsync(ContextUtilities.JohnDoe);
				int johnDoeId = userEntry.Entity.Id;

				var sessionService = new SessionService(getContext);
				Guid sessionId = await sessionService.CreateSessionAsync(johnDoeId, true);

				Session createdSession = await context.Sessions.FindAsync(sessionId);
				Assert.NotNull(createdSession);
				Assert.InRange(createdSession.Created, DateTime.UtcNow - TimeSpan.FromMinutes(1), DateTime.UtcNow);
				Assert.NotNull(createdSession.ValidUntil);
				Assert.True(createdSession.ValidUntil >= DateTime.UtcNow);
			}
		}

		[Fact]
		public async Task CreateInactiveSessionIsNotActivated()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext();
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> userEntry = await context.Users.AddAsync(ContextUtilities.JohnDoe);
				int johnDoeId = userEntry.Entity.Id;

				var sessionService = new SessionService(getContext);
				Guid sessionId = await sessionService.CreateSessionAsync(johnDoeId, false);

				Session createdSession = await context.Sessions.FindAsync(sessionId);
				Assert.NotNull(createdSession);
				Assert.InRange(createdSession.Created, DateTime.UtcNow - TimeSpan.FromMinutes(1), DateTime.UtcNow);
				Assert.Null(createdSession.ValidUntil);
			}
		}
	}
}

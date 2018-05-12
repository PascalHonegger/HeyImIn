using System;
using System.Threading.Tasks;
using HeyImIn.Authentication.Impl;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.Shared;
using HeyImIn.Shared.Tests;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xunit;
using Xunit.Abstractions;

namespace HeyImIn.Authentication.Tests
{
	public class SessionServiceTests : TestBase
	{
		private (GetDatabaseContext, SessionService) SetupSessionService()
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext();
			var sessionService = new SessionService(_configuration, getContext, DummyLogger<SessionService>());
			return (getContext, sessionService);
		}

		private static readonly HeyImInConfiguration _configuration = new HeyImInConfiguration();

		#region CreateSession

		[Fact]
		public async Task CreateSession_GivenIsActiveTrue_SessionIsActivated()
		{
			(GetDatabaseContext getContext, SessionService sessionService) = SetupSessionService();
			int johnDoeId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> userEntry = context.Users.Add(ContextUtilities.JohnDoe);
				await context.SaveChangesAsync();
				johnDoeId = userEntry.Entity.Id;
			}

			// Act
			Guid sessionId = await sessionService.CreateSessionAsync(johnDoeId, true);

			// Assert
			using (IDatabaseContext context = getContext())
			{
				Session createdSession = await context.Sessions.FindAsync(sessionId);
				Assert.NotNull(createdSession);
				Assert.InRange(createdSession.Created, DateTime.UtcNow - TimeSpan.FromMinutes(1), DateTime.UtcNow);
				Assert.NotNull(createdSession.ValidUntil);
				Assert.True(createdSession.ValidUntil >= DateTime.UtcNow);
			}
		}

		[Fact]
		public async Task CreateSession_GivenIsActiveFalse_SessionIsNotActivated()
		{
			(GetDatabaseContext getContext, SessionService sessionService) = SetupSessionService();
			int johnDoeId;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> userEntry = context.Users.Add(ContextUtilities.JohnDoe);
				await context.SaveChangesAsync();
				johnDoeId = userEntry.Entity.Id;
			}

			// Act
			Guid sessionId = await sessionService.CreateSessionAsync(johnDoeId, false);

			// Assert
			using (IDatabaseContext context = getContext())
			{
				Session createdSession = await context.Sessions.FindAsync(sessionId);
				Assert.NotNull(createdSession);
				Assert.InRange(createdSession.Created, DateTime.UtcNow - TimeSpan.FromMinutes(1), DateTime.UtcNow);
				Assert.Null(createdSession.ValidUntil);
			}
		}

		#endregion

		#region GetSession

		[Fact]
		public async Task GetSession_GivenInexistantSession_NullReturned()
		{
			(_, SessionService sessionService) = SetupSessionService();

			// Act
			Session loadedSession = await sessionService.GetAndExtendSessionAsync(Guid.NewGuid());

			// Assert
			Assert.Null(loadedSession);
		}

		[Fact]
		public async Task GetSession_GivenValidSession_ExtendsSession()
		{
			(GetDatabaseContext getContext, SessionService sessionService) = SetupSessionService();
			Guid sessionToken;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> userEntry = context.Users.Add(ContextUtilities.JohnDoe);

				var validUserSession = new Session
				{
					Created = DateTime.UtcNow,
					ValidUntil = null,
					User = userEntry.Entity
				};
				context.Sessions.Add(validUserSession);
				await context.SaveChangesAsync();

				sessionToken = validUserSession.Token;
			}

			// Act
			Session loadedSession = await sessionService.GetAndExtendSessionAsync(sessionToken);

			// Assert
			Assert.NotNull(loadedSession);
			Assert.True(loadedSession.ValidUntil >= DateTime.UtcNow);

			DateTime firstValidUntil = loadedSession.ValidUntil.Value;

			await Task.Delay(1);

			loadedSession = await sessionService.GetAndExtendSessionAsync(loadedSession.Token);

			Assert.NotNull(loadedSession);
			Assert.True(firstValidUntil < loadedSession.ValidUntil);
		}

		[Fact]
		public async Task GetSession_GivenInvalidSession_SessionUnchanged()
		{
			(GetDatabaseContext getContext, SessionService sessionService) = SetupSessionService();
			Session invalidUserSession;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> userEntry = context.Users.Add(ContextUtilities.JohnDoe);

				invalidUserSession = new Session
				{
					Created = DateTime.UtcNow - TimeSpan.FromHours(10),
					ValidUntil = DateTime.UtcNow - TimeSpan.FromHours(5),
					User = userEntry.Entity
				};
				context.Sessions.Add(invalidUserSession);
				await context.SaveChangesAsync();
			}

			// Act
			Session loadedSession = await sessionService.GetAndExtendSessionAsync(invalidUserSession.Token);

			// Assert
			using (IDatabaseContext context = getContext())
			{
				Assert.Null(loadedSession);

				Session newlyLoadedSession = await context.Sessions.FindAsync(invalidUserSession.Token);
				Assert.NotNull(newlyLoadedSession);
				Assert.Equal(invalidUserSession.ValidUntil, newlyLoadedSession.ValidUntil);
			}
		}

		[Fact]
		public async Task GetSession_GivenExpiredUnusedSession_SessionUnchanged()
		{
			(GetDatabaseContext getContext, SessionService sessionService) = SetupSessionService();
			Session invalidUserSession;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> userEntry = context.Users.Add(ContextUtilities.JohnDoe);

				invalidUserSession = new Session
				{
					Created = DateTime.UtcNow - _configuration.Timeouts.UnusedSessionExpirationTimeout,
					ValidUntil = null,
					User = userEntry.Entity
				};
				context.Sessions.Add(invalidUserSession);
				await context.SaveChangesAsync();
			}

			// Act
			Session loadedSession = await sessionService.GetAndExtendSessionAsync(invalidUserSession.Token);

			// Assert
			using (IDatabaseContext context = getContext())
			{
				Assert.Null(loadedSession);

				Session newlyLoadedSession = await context.Sessions.FindAsync(invalidUserSession.Token);
				Assert.NotNull(newlyLoadedSession);
				Assert.Null(newlyLoadedSession.ValidUntil);
			}
		}

		#endregion

		#region InvalidateSession

		[Fact]
		public async Task InvalidateSession_GivenInexistantSession_NoErrorThrown()
		{
			(_, SessionService sessionService) = SetupSessionService();

			// Act
			await sessionService.InvalidateSessionAsync(Guid.NewGuid());

			// If no exception was thrown, this test was a success
		}

		[Fact]
		public async Task InvalidateSession_GivenValidSession_SessionIsInvalidated()
		{
			(GetDatabaseContext getContext, SessionService sessionService) = SetupSessionService();
			Session validUserSession;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> userEntry = context.Users.Add(ContextUtilities.JohnDoe);

				validUserSession = new Session
				{
					Created = DateTime.UtcNow,
					ValidUntil = DateTime.UtcNow + TimeSpan.FromHours(1),
					User = userEntry.Entity
				};
				context.Sessions.Add(validUserSession);
				await context.SaveChangesAsync();
			}

			// Act
			await sessionService.InvalidateSessionAsync(validUserSession.Token);

			// Assert
			using (IDatabaseContext context = getContext())
			{
				Session newlyLoadedSession = await context.Sessions.FindAsync(validUserSession.Token);
				Assert.NotNull(newlyLoadedSession);
				Assert.NotEqual(validUserSession.ValidUntil, newlyLoadedSession.ValidUntil);
				Assert.True(newlyLoadedSession.ValidUntil <= DateTime.UtcNow);
			}
		}

		[Fact]
		public async Task InvalidateSession_GivenInvalidSession_SessionUnchanged()
		{
			(GetDatabaseContext getContext, SessionService sessionService) = SetupSessionService();
			Session invalidUserSession;

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> userEntry = context.Users.Add(ContextUtilities.JohnDoe);

				invalidUserSession = new Session
				{
					Created = DateTime.UtcNow - TimeSpan.FromHours(10),
					ValidUntil = DateTime.UtcNow - TimeSpan.FromHours(5),
					User = userEntry.Entity
				};
				context.Sessions.Add(invalidUserSession);
				await context.SaveChangesAsync();
			}

			// Act
			await sessionService.InvalidateSessionAsync(invalidUserSession.Token);

			// Assert
			using (IDatabaseContext context = getContext())
			{
				Session newlyLoadedSession = await context.Sessions.FindAsync(invalidUserSession.Token);
				Assert.NotNull(newlyLoadedSession);
				Assert.Equal(invalidUserSession.ValidUntil, newlyLoadedSession.ValidUntil);
			}
		}

		#endregion

		public SessionServiceTests(ITestOutputHelper output) : base(output)
		{
		}
	}
}

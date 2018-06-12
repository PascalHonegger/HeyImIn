using System;
using HeyImIn.Database.Context;
using HeyImIn.Database.Context.Impl;
using HeyImIn.Database.Models;
using HeyImIn.Shared.Tests;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace HeyImIn.Database.Tests
{
	public static class ContextUtilities
	{
		/// <summary>
		///     Created an in memory database which can be accessed using the returned function
		/// </summary>
		public static GetDatabaseContext CreateInMemoryContext(ITestOutputHelper output)
		{
			string databaseName = Guid.NewGuid().ToString();

			var builder = new DbContextOptionsBuilder<HeyImInDatabaseContext>();
			builder
				.UseInMemoryDatabase(databaseName)
				.EnableSensitiveDataLogging()
				.UseLoggerFactory(new XUnitLoggerFactory(output));

			var heyImInDatabaseContext = new HeyImInDatabaseContext(builder.Options);
			heyImInDatabaseContext.Database.EnsureDeleted();
			heyImInDatabaseContext.Database.EnsureCreated();

			return () => new HeyImInDatabaseContext(builder.Options);
		}

		/// <summary>
		///     The main dummy user who is used within tests
		///     Usually the user which perfoms an action
		///     E.g. join an event
		/// </summary>
		public static User CreateJohnDoe()
		{
			return new User
			{
				FullName = "John Doe",
				Email = "john.doe@email.com"
			};
		}

		/// <summary>
		///     A second dummy user used to accommodate <see cref="CreateJohnDoe" /> in his goal
		///     E.g. organizes an event someone else joins
		/// </summary>
		public static User CreateRichardRoe()
		{
			return new User
			{
				FullName = "Richard Roe",
				Email = "richard.roe@email.com"
			};
		}

		/// <summary>
		///     Creates a completely random user
		/// </summary>
		public static User CreateRandomUser()
		{
			string randomName = Guid.NewGuid().ToString("N").Substring(0, 10);
			return new User
			{
				FullName = randomName,
				Email = $"{randomName}@email.com"
			};
		}
	}
}

using System;
using HeyImIn.Database.Context;
using HeyImIn.Database.Context.Impl;
using HeyImIn.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace HeyImIn.Database.Tests
{
	public static class ContextUtilities
	{
		/// <summary>
		///     Created an in memory database which can be accessed using the returned function
		/// </summary>
		public static GetDatabaseContext CreateInMemoryContext()
		{
			string databaseName = Guid.NewGuid().ToString();

			var builder = new DbContextOptionsBuilder<HeyImInDatabaseContext>();
			builder.UseInMemoryDatabase(databaseName);

			var heyImInDatabaseContext = new HeyImInDatabaseContext(builder.Options);
			heyImInDatabaseContext.Database.EnsureDeleted();
			heyImInDatabaseContext.Database.EnsureCreated();

			return () => new HeyImInDatabaseContext(builder.Options);
		}

		/// <summary>
		/// The main dummy user who is used within tests
		/// Usually the user which perfoms an action
		/// E.g. join an event
		/// </summary>
		public static User JohnDoe => new User
		{
			FullName = "John Doe",
			Email = "john.doe@email.com"
		};

		/// <summary>
		/// A second dummy user used to accommodate <see cref="JohnDoe"/> in his goal
		/// E.g. organizes an event someone else joins
		/// </summary>
		public static User RichardRoe => new User
		{
			FullName = "Richard Roe",
			Email = "richard.roe@email.com"
		};
	}
}

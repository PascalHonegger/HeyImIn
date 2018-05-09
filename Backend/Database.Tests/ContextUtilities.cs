using System;
using HeyImIn.Database.Context;
using HeyImIn.Database.Context.Impl;
using HeyImIn.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace HeyImIn.Database.Tests
{
	public static class ContextUtilities
	{
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

		public static User JohnDoe => new User
		{
			FullName = "John Doe",
			Email = "john.doe@email.com"
		};
	}
}

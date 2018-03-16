using System.Data.Entity;
using HeyImIn.Database.Context.Impl;
using HeyImIn.Database.Migrations;

namespace HeyImIn.Database.Context
{
	public static class DatabaseConfiguration
	{
		/// <summary>
		///     Enables automatic migrations
		/// </summary>
		public static void ConfigureMigrations()
		{
			System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<HeyImInDatabaseContext, Configuration>(
				true /* As we inject the connection string using DI, this is required */));
		}
	}
}

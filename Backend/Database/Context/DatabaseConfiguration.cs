using System.Data.Entity;
using HeyImIn.Database.Context.Impl;
using HeyImIn.Database.Migrations;

namespace HeyImIn.Database.Context
{
	public static class DatabaseConfiguration
	{
		public static void ConfigureMigrations()
		{
			System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<HeyImInDatabaseContext, Configuration>(true));
		}
	}
}

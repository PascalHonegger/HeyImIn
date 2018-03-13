using System.Data.Entity.Migrations;
using HeyImIn.Database.Context.Impl;

namespace HeyImIn.Database.Migrations
{
	internal sealed class Configuration : DbMigrationsConfiguration<HeyImInDatabaseContext>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = true;

			// TODO Remove once production data could be affected
			AutomaticMigrationDataLossAllowed = true;
		}

		protected override void Seed(HeyImInDatabaseContext context)
		{
			//  This method will be called after migrating to the latest version.
		}
	}
}

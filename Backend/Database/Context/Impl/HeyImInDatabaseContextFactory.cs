using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace HeyImIn.Database.Context.Impl
{
	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global => Entity Framework
	public class HeyImInDatabaseContextFactory : IDesignTimeDbContextFactory<HeyImInDatabaseContext>
	{
		public HeyImInDatabaseContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<HeyImInDatabaseContext>();
			optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=HeyImIn;Trusted_Connection=True;");

			return new HeyImInDatabaseContext(optionsBuilder.Options);
		}
	}
}

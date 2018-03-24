using Microsoft.EntityFrameworkCore;

namespace HeyImIn.Database.Context
{
	public static class TempExtensions
	{
		public static T Create<T>(this DbSet<T> source) where T : class, new()
		{
			// TODO Remove
			return new T();
		}
	}
}

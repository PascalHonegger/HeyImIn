using System.Data.Entity;
using HeyImIn.Database.Models;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace HeyImIn.Database.Context.Impl
{
	public class HeyImInDatabaseContext : DbContext, IDatabaseContext
	{
		// For Designer
		/*public HeyImInDatabaseContext() : base("Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=HeyImIn;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|\\HeyImIn.mdf")
		{
		}*/

		public HeyImInDatabaseContext(string connectionString) : base(connectionString)
		{
		}

		// Main tables
		public virtual DbSet<User> Users { get; set; }
		public virtual DbSet<Appointment> Appointments { get; set; }
		public virtual DbSet<Event> Events { get; set; }
		public virtual DbSet<Session> Sessions { get; set; }

		// Many-To-Many relation tables
		public virtual DbSet<AppointmentParticipation> AppointmentParticipations { get; set; }
		public virtual DbSet<EventParticipation> EventParticipations { get; set; }

		// Token tables
		public virtual DbSet<PasswordReset> PasswordResets { get; set; }
		public virtual DbSet<EventInvitation> EventInvitations { get; set; }
	}
}
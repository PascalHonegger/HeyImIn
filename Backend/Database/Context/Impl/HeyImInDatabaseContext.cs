using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using HeyImIn.Database.Models;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace HeyImIn.Database.Context.Impl
{
	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global => Entity Framework
	public class HeyImInDatabaseContext : DbContext, IDatabaseContext
	{
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

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			// We manually remove / update relations before deleting the referenced entry
			modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
			modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
		}
	}
}

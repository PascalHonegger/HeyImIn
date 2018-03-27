using System.Collections.Generic;
using System.Reflection;
using HeyImIn.Database.Models;
using log4net;
using Microsoft.EntityFrameworkCore;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace HeyImIn.Database.Context.Impl
{
	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global => Entity Framework
	public class HeyImInDatabaseContext : DbContext, IDatabaseContext
	{
		// ReSharper disable once SuggestBaseTypeForParameter
		public HeyImInDatabaseContext(DbContextOptions<HeyImInDatabaseContext> options) : base(options)
		{
		}

		// Main tables
		public void Migrate()
		{
			IEnumerable<string> pendingMigrations = Database.GetPendingMigrations();

			_log.InfoFormat("{0}(): Trying to apply migrations ({1})", nameof(Migrate), string.Join(',', pendingMigrations));

			Database.Migrate();

			_log.InfoFormat("{0}(): Applied migrations", nameof(Migrate));
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
			modelBuilder.Entity<User>().HasMany(u => u.PasswordResets).WithOne(p => p.User).OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<User>().HasMany(u => u.Sessions).WithOne(p => p.User).OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<User>().HasMany(u => u.AppointmentParticipations).WithOne(p => p.Participant).OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<User>().HasMany(u => u.EventParticipations).WithOne(p => p.Participant).OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<User>().HasMany(u => u.OrganizedEvents).WithOne(p => p.Organizer).OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Event>().HasMany(e => e.EventInvitations).WithOne(p => p.Event).OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<Event>().HasMany(e => e.EventParticipations).WithOne(p => p.Event).OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Event>().HasMany(e => e.Appointments).WithOne(p => p.Event).OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Appointment>().HasMany(a => a.AppointmentParticipations).WithOne(p => p.Appointment).OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<AppointmentParticipation>().HasIndex(a => new { a.ParticipantId, a.AppointmentId }).IsUnique();

			modelBuilder.Entity<EventParticipation>().HasIndex(e => new { e.ParticipantId, e.EventId }).IsUnique();

			// TODO Add all relations
			// TODO Cleanup delete code?
		}

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

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
}

using System;
using System.Threading;
using System.Threading.Tasks;
using HeyImIn.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace HeyImIn.Database.Context
{
	/// <summary>
	///     A Delegate to prevent the copy-paste of <see cref="Func{TResult}" /> of <see cref="IDatabaseContext" />
	/// </summary>
	public delegate IDatabaseContext GetDatabaseContext();

	public interface IDatabaseContext : IDisposable
	{
		// DbContext methods we want to provide
		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

		void Migrate();

		// Main tables
		DbSet<User> Users { get; }

		DbSet<Appointment> Appointments { get; }

		DbSet<Event> Events { get; }

		DbSet<Session> Sessions { get; }

		// Many-To-Many relation tables
		DbSet<AppointmentParticipation> AppointmentParticipations { get; }

		DbSet<EventParticipation> EventParticipations { get; }

		// Token tables
		DbSet<PasswordReset> PasswordResets { get; }

		DbSet<EventInvitation> EventInvitations { get; }
	}
}

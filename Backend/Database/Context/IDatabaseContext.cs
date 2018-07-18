using System;
using System.Threading;
using System.Threading.Tasks;
using HeyImIn.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace HeyImIn.Database.Context
{
	/// <summary>
	///     A Delegate to prevent the copy-paste of <see cref="Func{TResult}" /> of <see cref="IDatabaseContext" />
	/// </summary>
	public delegate IDatabaseContext GetDatabaseContext();

	public interface IDatabaseContext : IDisposable
	{
		// DbContext methods we want to provide
		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

		EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

		void Migrate(ILoggerFactory loggerFactory);

		void DiscardChanges();

		/// <summary>
		///     The tracking behavior, for more see <see cref="QueryTrackingBehavior" />
		/// </summary>
		/// <param name="trackingBehavior">The tracking behavior to use for this context</param>
		void WithTrackingBehavior(QueryTrackingBehavior trackingBehavior);

		// Main tables
		DbSet<User> Users { get; }

		DbSet<Appointment> Appointments { get; }

		DbSet<Event> Events { get; }

		DbSet<Session> Sessions { get; }

		DbSet<ChatMessage> ChatMessages { get; set; }

		// Many-To-Many relation tables
		DbSet<AppointmentParticipation> AppointmentParticipations { get; }

		DbSet<EventParticipation> EventParticipations { get; }

		// Token tables
		DbSet<PasswordReset> PasswordResets { get; }

		DbSet<EventInvitation> EventInvitations { get; }
	}
}

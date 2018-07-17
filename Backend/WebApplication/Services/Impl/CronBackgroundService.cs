using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HeyImIn.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HeyImIn.WebApplication.Services.Impl
{
	public class CronBackgroundService : IHostedService, IDisposable
	{
		public CronBackgroundService(IServiceScopeFactory serviceScopeFactory, HeyImInConfiguration configuration, ILogger<CronBackgroundService> logger)
		{
			_serviceScopeFactory = serviceScopeFactory;
			_cronHandlerTimeSpan = configuration.TimeSpans.CronHandlerInterval;
			_logger = logger;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			// Store the task we're executing
			_executingTask = StartCronSchedule();

			// If the task is completed then return it,
			// this will bubble cancellation and failure to the caller
			if (_executingTask.IsCompleted)
			{
				return _executingTask;
			}

			// Otherwise it's running
			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			// Stop called without start
			if (_executingTask == null)
			{
				return;
			}

			try
			{
				_logger.LogInformation("{0}(): Stopping cron background service", nameof(StopAsync));

				// Signal cancellation to the executing method
				_stoppingCts.Cancel();
			}
			finally
			{
				// Wait until the task completes or the stop token triggers
				await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite,
					cancellationToken));
			}
		}

		public void Dispose()
		{
			_stoppingCts.Cancel();
		}

		/// <summary>
		/// Starts executing cron jobs on a regular basis, only returning once the <see cref="_stoppingCts"/> triggers a shutdown
		/// </summary>
		private async Task StartCronSchedule()
		{
			CancellationToken stoppingToken = _stoppingCts.Token;

			_logger.LogInformation("{0}(): Starting cron background service", nameof(StartCronSchedule));

			while (!stoppingToken.IsCancellationRequested)
			{
				using (IServiceScope serviceScope = _serviceScopeFactory.CreateScope())
				{
					var cronRunners = serviceScope.ServiceProvider.GetRequiredService<IEnumerable<ICronService>>();
					await ExecuteCronJobsAsync(cronRunners, stoppingToken);
				}

				await Task.Delay(_cronHandlerTimeSpan, stoppingToken);
			}

			_logger.LogDebug("{0}(): Cron background service task completed", nameof(StartCronSchedule));
		}

		/// <summary>
		///     Runs all cron-jobs
		///     Catches and logs exceptions thrown by the <see cref="ICronService.RunAsync" /> method
		/// </summary>
		private async Task ExecuteCronJobsAsync(IEnumerable<ICronService> cronRunners, CancellationToken token)
		{
			_logger.LogInformation("{0}(): Running cron jobs", nameof(ExecuteCronJobsAsync));

			var cronStopwatch = new Stopwatch();

			foreach (ICronService cronService in cronRunners)
			{
				_logger.LogDebug("{0}(): Start running '{1}'", nameof(ExecuteCronJobsAsync), cronService.DescriptiveName);
				cronStopwatch.Restart();

				try
				{
					await cronService.RunAsync(token);
				}
				catch (Exception e)
				{
					_logger.LogError("{0}(): Error while running '{1}', error={2}", nameof(ExecuteCronJobsAsync), cronService.DescriptiveName, e);
				}
				finally
				{
					cronStopwatch.Stop();
					_logger.LogDebug("{0}(): Finished running '{1}', duration = {2:g}", nameof(ExecuteCronJobsAsync), cronService.DescriptiveName, cronStopwatch.Elapsed);
				}
			}
		}

		private readonly IServiceScopeFactory _serviceScopeFactory;

		private readonly ILogger<CronBackgroundService> _logger;

		private Task _executingTask;
		private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
		private readonly TimeSpan _cronHandlerTimeSpan;
	}
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Sentry;

namespace Eurofurence.App.Server.Web.Jobs;

public class SentryJobListener : IJobListener
{
    private const string SentryCheckInId = "__SentryCheckInId";

    public string Name => nameof(SentryJobListener);

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var id = SentrySdk.CaptureCheckIn(context.Trigger.JobKey.Name, CheckInStatus.InProgress);
        context.Put(SentryCheckInId, id);
        return Task.CompletedTask;
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException jobException,
        CancellationToken cancellationToken = default)
    {
        if (context.Get(SentryCheckInId) is SentryId id)
        {
            SentrySdk.CaptureCheckIn(
                context.Trigger.JobKey.Name,
                jobException is null ? CheckInStatus.Ok : CheckInStatus.Error,
                id,
                DateTimeOffset.UtcNow - context.FireTimeUtc
            );
        }

        return Task.CompletedTask;
    }
}
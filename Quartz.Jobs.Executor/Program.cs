
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Jobs.Executor;
using Quartz.Jobs.Executor.Jobs;
using System.Collections.Specialized;

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices((cxt, services) =>
    {
        services.AddSingleton<ScheduleRepository>();

        services.AddQuartz(q =>
        {
            q.UsePersistentStore(store =>
            {
                //store.PerformSchemaValidation = true; // default
                store.UseProperties = true; // preferred, but not default
                store.RetryInterval = TimeSpan.FromSeconds(15);
                store.UseSqlServer(sqlServer =>
                {
                    sqlServer.ConnectionString = "Server=(LocalDB)\\MSSQLLocalDB;Database=JobsDB;Integrated Security=SSPI";
                    // this is the default
                    //sqlServer.TablePrefix = "QRTZ_";
                });

                store.UseSystemTextJsonSerializer();
                store.UseClustering(c =>
                {
                    c.CheckinMisfireThreshold = TimeSpan.FromSeconds(20);
                    c.CheckinInterval = TimeSpan.FromSeconds(10);
                });
            });
        });

        // Register Quartz Hosted Service for background jobs (optional)
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }).Build();

var schedulerfactory = builder.Services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerfactory.GetScheduler();

await new QuartzHelper().CleanupAllJobs(scheduler);

var jobKey = new JobKey("SyncTriggersJob", "SyncGroup");
var triggerKey = new TriggerKey("SyncTriggersTrigger", "SyncGroup");

IJobDetail? jobDetail = null;

bool jobExists = await scheduler.CheckExists(jobKey);
if (jobExists)
{
    // Try to get the job detail
    jobDetail = await scheduler.GetJobDetail(jobKey);
}
else
{
    // Create the sync job detail
    jobDetail = JobBuilder.Create<SyncJob>()
        .WithIdentity(jobKey)
        .Build();
}

if (jobDetail == null)
{
    throw new SystemException("Cannot setup SYNC job!");
}

// Check if the trigger already exists
bool triggerExists = await scheduler.CheckExists(triggerKey);
if (!triggerExists)
{
    // Create a trigger to run the sync job every 5 minutes
    ITrigger trigger = TriggerBuilder.Create()
    .WithIdentity("SyncTriggersTrigger", "SyncGroup")
    .StartNow()
    .WithSimpleSchedule(x => x.WithIntervalInMinutes(5).RepeatForever())
    .Build();

    // Schedule the sync job
    await scheduler.ScheduleJob(jobDetail, trigger);
}


// will block until the last running job completes
await builder.RunAsync();

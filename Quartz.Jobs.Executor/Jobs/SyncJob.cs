using Microsoft.Extensions.DependencyInjection;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Jobs.Executor.Jobs
{
    public class SyncJob : IJob
    {
        private readonly IScheduler scheduler;
        private readonly ScheduleRepository scheduleRepository;

        public SyncJob(IServiceProvider serviceProvider, ScheduleRepository scheduleRepository)
        {
            var schedulerfactory = serviceProvider.GetRequiredService<ISchedulerFactory>();
            this.scheduler = schedulerfactory.GetScheduler().Result;

            this.scheduleRepository = scheduleRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var activeJobs = scheduleRepository.GetJobsWithSchedules();

            // Remove inactive / deleted jobs from quartz
            await RemoveJobs(activeJobs);

            // Add, Update jobs and schedules in quartz
            foreach (var job in activeJobs)
            {
                var jobKey = new JobKey($"{job.JobName}", job.JobCategory);
                var triggerKey = new TriggerKey($"{job.JobName}-trigger", job.JobCategory);

                var jobDetail = await AddJobAsync(jobKey, job);
                await AddOrUpdateJobTriggerAsync(triggerKey, jobDetail, job);
            }
        }

        private async Task<IJobDetail?> AddJobAsync(JobKey jobKey, JobSchedule job)
        {
            bool jobExists = await scheduler.CheckExists(jobKey);
            if (jobExists)
            {
                // Try to get the job detail
                return await scheduler.GetJobDetail(jobKey);
            }
            else
            {
                switch (job.JobType)
                {
                    case JobType.JobType1:
                        // Create job
                        return JobBuilder.Create<SampleJob1>()
                           .WithIdentity(jobKey)
                           .Build();
                    case JobType.JobType2:
                        return JobBuilder.Create<SampleJob2>()
                           .WithIdentity(jobKey)
                           .Build();
                    case JobType.JobType3:
                        return JobBuilder.Create<SampleJob2>()
                            .WithIdentity(jobKey)
                            .Build();
                    default:
                        throw new ArgumentException("Job type not found!");
                }
            }
        }

        private async Task AddOrUpdateJobTriggerAsync(TriggerKey triggerKey, IJobDetail? jobDetail, JobSchedule job)
        {
            if (jobDetail == null) return;

            // Check if the trigger already exists
            bool triggerExists = await scheduler.CheckExists(triggerKey);
            if (triggerExists)
            {
                // Ensure trigger is in sync with database
               var trigger = await scheduler.GetTrigger(triggerKey) as ISimpleTrigger;
                if (trigger == null)
                {
                    // Log error
                    return;
                }

                if (trigger.RepeatInterval.Seconds != job.Interval)
                {
                    // Create a new trigger with the updated interval
                    ITrigger newTrigger = TriggerBuilder.Create()
                        .WithIdentity(triggerKey)
                        .StartNow()
                        .WithSimpleSchedule(x => x.WithIntervalInSeconds(job.Interval).RepeatForever())
                        .Build();

                    // Reschedule the job with the new trigger (this will automatically replace the old one)
                    await scheduler.RescheduleJob(triggerKey, newTrigger);
                }
            }
            else
            {
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey)
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(job.Interval).RepeatForever())
                    .Build();

                // Schedule the job with the trigger
                await scheduler.ScheduleJob(jobDetail, trigger);
            }
        }

        private async Task RemoveJobs(IEnumerable<JobSchedule> jobs)
        {
            var allQuartzJobs = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            foreach (var quartzJob in allQuartzJobs)
            {
                var isJobActive = jobs.Any(x => quartzJob.Name == x.JobName);
                if (!isJobActive)
                {
                    await scheduler.DeleteJob(quartzJob);
                }
            }
        }
    }
}

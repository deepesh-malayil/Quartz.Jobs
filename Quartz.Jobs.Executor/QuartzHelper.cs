using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Jobs.Executor
{
    public class QuartzHelper
    {
        public async Task CleanupAllJobs(IScheduler scheduler)
        {
            var allJobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()); // Retrieve all jobs

            foreach (var jobKey in allJobKeys)
            {
                // Step 2: Unschedule each job's triggers
                var triggersOfJob = await scheduler.GetTriggersOfJob(jobKey);
                foreach (var trigger in triggersOfJob)
                {
                    // Unschedule each trigger
                    await scheduler.UnscheduleJob(trigger.Key);
                }

                // Step 3: Delete each job
                await scheduler.DeleteJob(jobKey);

                Console.WriteLine($"Job {jobKey.Name} and its triggers have been removed.");
            }
        }

    }
}

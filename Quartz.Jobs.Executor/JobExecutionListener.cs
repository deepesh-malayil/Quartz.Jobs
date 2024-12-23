using Quartz;
using Quartz.Listener;
using System;
using System.Threading.Tasks;

namespace Quartz.Jobs.Executor
{
    public class JobExecutionListener : IJobListener
    {
        private readonly ScheduleRepository scheduleRepository;

        public JobExecutionListener(ScheduleRepository scheduleRepository)
        {
            this.scheduleRepository = scheduleRepository;
        }

        // Specify a unique listener name
        public string Name => "JobExecutionListener";

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        // Called when a job is about to execute
        public async Task JobToBeExecuted(IJobExecutionContext context)
        {
            // Optionally, you can log or perform actions before the job executes
        }

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        // Called after the job is executed
        public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            //var jobHistory = new JobExecutionHistory
            //{
            //    JobName = context.JobDetail.Key.Name,
            //    StartTime = context.FireTimeUtc.LocalDateTime,
            //    EndTime = DateTime.UtcNow,
            //    DurationInMilliseconds = (int)(context.JobRunTime.TotalMilliseconds),
            //    JobStatus = jobException == null ? "Succeeded" : "Failed",
            //    ExceptionMessage = jobException?.Message
            //};

            // Save the execution history to the database
            scheduleRepository.SaveJobExecutionHistory();
        }

        public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
        {
            // You can disable a job execution here
        }
    }

}


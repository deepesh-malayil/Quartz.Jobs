using Microsoft.Data.SqlClient;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Jobs.Executor;
using Quartz.Jobs.Executor.Jobs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Quartz.Logging.OperationName;

public class ScheduleRepository
{
    IEnumerable<JobSchedule> jobsWithSchedules = new List<JobSchedule>();

    public ScheduleRepository()
    {
        jobsWithSchedules = new List<JobSchedule>()
        {
            new JobSchedule{ JobName = "Job-1", JobType = JobType.JobType1, Interval = 2 },
            new JobSchedule{ JobName = "Job-2", JobType = JobType.JobType2, Interval = 4 },
            new JobSchedule{ JobName = "Job-3", JobType = JobType.JobType3, Interval = 8 },
            new JobSchedule{ JobName = "Job-4", JobType = JobType.JobType1, Interval = 10 },
        };
    }

    public IEnumerable<JobSchedule> GetJobsWithSchedules()
    {
        return jobsWithSchedules;
    }

    public void SaveJobExecutionHistory()
    {
        throw new NotImplementedException();
    }
}
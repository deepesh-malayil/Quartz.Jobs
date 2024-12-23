using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Jobs.Executor.Jobs
{
    public class SampleJob1 : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Sample Job - 1 is executing!");
            return Task.CompletedTask;
        }
    }
}

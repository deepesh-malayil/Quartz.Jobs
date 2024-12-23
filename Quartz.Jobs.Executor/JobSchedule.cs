using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Jobs.Executor
{
    public enum JobType
    {
        JobType1 = 1,
        JobType2 = 2,
        JobType3 = 3
    }

    public class JobSchedule
    {
        public string JobName { get; set; } = string.Empty;
        public string JobGroup { get; set; } = string.Empty;
        public JobType JobType { get; set; }
        public string JobCategory { get; set; } = "DEFAULT";
        public int Interval { get; set; } = 10;
    }
}

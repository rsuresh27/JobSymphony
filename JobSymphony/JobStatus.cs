using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSymphony
{
    public enum JobStatus
    {
        Queued,
        Pending,
        Running,
        Completed,
        Errored, 
        Cancelled
    }
}

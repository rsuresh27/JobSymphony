using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSymphony.ServiceExtensions
{
    public interface IJobSchedulerConfiguration
    {
        public void UseSQLiteDB(); 
    }
}

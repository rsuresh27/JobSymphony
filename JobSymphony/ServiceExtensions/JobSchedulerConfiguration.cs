using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSymphony.ServiceExtensions
{
    public class JobSchedulerConfiguration(IServiceCollection serviceCollection) : IJobSchedulerConfiguration
    {
        private readonly IServiceCollection _serviceCollection = serviceCollection;

        public void UseSQLiteDB()
        {

        }
    }
}

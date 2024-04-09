using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSymphony
{
    public record HelloWorldJob(ILogger<HelloWorldJob> logger) : BaseJob
    {
        private readonly ILogger<HelloWorldJob> _logger = logger;

        public override Task Run(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Hello World");

            return Task.CompletedTask;
        }
    }
}

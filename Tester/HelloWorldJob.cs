using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tester
{
    public class HelloWorldJob(ILogger<HelloWorldJob> logger, string name = default, int age = 0) : IJob
    {
        private readonly ILogger<HelloWorldJob> _logger = logger;

        public ValueTask Run(CancellationToken cancellationToken = default)
        {
            if (name is not null && age is not 0)
            {
                _logger.LogInformation("Hello {name}, you are {age} years old", name, age);
            }
            else
            {
                _logger.LogInformation("Hello world");
            }

            return ValueTask.CompletedTask;
        }
    }
}

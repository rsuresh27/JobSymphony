using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tester
{
    public class HelloName(ILogger<HelloName> logger, string name) : IJob
    {
        private readonly ILogger<HelloName> _logger = logger;

        public ValueTask Run(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

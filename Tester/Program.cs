using Core;
using Core.Queues.SQLite.Contexts;
using Core.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Models;
using Tester;


HostApplicationBuilder builder = new(args);
builder.Services.AddJobScheduler().UseSQLiteDb(new JobQueueConfigurationOptions() { Concurrency = 10 }).ConfigureJobs();
builder.Services.AddLogging();

IHost host = builder.Build();

// ensure db is created
using (IServiceScope scope = host.Services.CreateScope())
{
    JobDbContext jobDbContext = scope.ServiceProvider.GetRequiredService<JobDbContext>();
    jobDbContext.Database.EnsureCreated();
}

IJobScheduler jobScheduler = host.Services.GetRequiredService<IJobScheduler>();

// schedule a job to run 5 seconds from now
await jobScheduler.ScheduleWithConfiguration<HelloWorldJob>(new() { Interval = new TimeSpan(0, 0, 5), Recurring = true }, "Mike", "26");

await host.RunAsync();

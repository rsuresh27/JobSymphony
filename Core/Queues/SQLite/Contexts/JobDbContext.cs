using Microsoft.EntityFrameworkCore;
using Models;

namespace Core.Queues.SQLite.Contexts
{
    public class JobDbContext : DbContext
    {
        public DbSet<Job> Jobs { get; private set; }

        public string DbPath { get; }

        public JobDbContext()
        {
            DbPath = Path.Join(Directory.GetCurrentDirectory(), "jobs.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite($"Data Source={DbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Job>().HasAlternateKey(job => job.Id);

            modelBuilder.Entity<Job>().Property(job => job.PayloadArgs).HasConversion(
                args => string.Join('|', args!),
                args => args.Split('|', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}

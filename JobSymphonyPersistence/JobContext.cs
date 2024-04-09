using JobSymphony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; 

namespace JobSymphonyPersistence
{
    public class JobContext : DbContext
    {
        public DbSet<BaseJob> Jobs { get; set; }

        private string _dbPath { get; }

        public JobContext()
        {
            _dbPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "jobs.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite($"Data Source =${_dbPath}"); 



    }
}

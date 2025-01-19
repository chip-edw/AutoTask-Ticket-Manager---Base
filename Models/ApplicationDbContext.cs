using Microsoft.EntityFrameworkCore;

using Serilog;

namespace AutoTaskTicketManager_Base.Models
{
    internal class ApplicationDbContext : DbContext
    {
        public DbSet<ConfigStore> ConfigStore { get; set; }
        public DbSet<CustomerSettings> CustomerSettings { get; set; }
        public DbSet<Scheduler> Schedulers { get; set; }


        #region OnConfiguring Method
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Get the directory of the executing assembly
            var exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            Console.WriteLine("OnConfiguring Model Triggered in ApplicationDbContext");

            if (exePath == null)
            {
                // Log the error using Serilog and throw an exception
                Log.Error("The executable path could not be determined. Ensure the assembly location is accessible.");
                throw new InvalidOperationException("The executable path cannot be null.");
            }

            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(exePath)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlite(connectionString);
            }
        }

        #endregion


        #region OnModelCreating Method
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default values
            modelBuilder.Entity<Scheduler>()
                .Property(s => s.LastRunTime)
                .HasDefaultValueSql("datetime('0001-01-01 00:00:00')"); // SQLite syntax for DateTime.MinValue

            modelBuilder.Entity<Scheduler>()
                .Property(s => s.NextRunTime)
                .HasDefaultValueSql("datetime('9999-12-31 23:59:59')"); // SQLite syntax for DateTime.MaxValue


            //Configure CustomerSettings
            modelBuilder.Entity<CustomerSettings>()
                .Property(c => c.SupportEmail)
                .IsRequired(false) // Make the column nullable
                .HasDefaultValue(null); // Set default value to null

        }
        #endregion

    }
}

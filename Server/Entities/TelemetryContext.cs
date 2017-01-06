using System.Data.Entity;
using Contracts;

namespace Server.Entities
{
    internal class TelemetryContext : DbContext
    {
        public TelemetryContext() : base("name=Server.Properties.Settings.ConnectionString2DB")
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<TelemetryContext>());
        }

        public DbSet<Telemetry> Telemetry { get; set; }
    }
}

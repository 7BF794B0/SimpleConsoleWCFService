using System.Data.Entity;

namespace Server.Entities
{
    internal class TelemetryContext : DbContext
    {
        public TelemetryContext() : base("ConnectionString2DB")
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<TelemetryContext>());
        }

        public DbSet<TelemetryEntity> Telemetry { get; set; }
    }
}
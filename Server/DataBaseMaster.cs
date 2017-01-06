using System.Data.Entity;
using System.Transactions;

using Contracts;
using Server.Entities;

namespace Server
{
    internal class DataBaseMaster
    {
        /// <summary>
        /// The method of adding records to the database.
        /// Called from the method TransactionScopeWithStorage.
        /// </summary>
        /// <param name="context">The context of the interaction with the database.</param>
        /// <param name="entity">The object to be added.</param>
        /// <param name="count">Counter.</param>
        /// <param name="commitCount">Meaning - "every time commitCount doing SaveChanges()".</param>
        /// <returns>The context of the interaction with the database.</returns>
        private static DbContext AddToDb(DbContext context, Telemetry entity, int count, int commitCount)
        {
            context.Set<Telemetry>().Add(entity);

            if (count % commitCount != 0)
                return context;

            context.SaveChanges();

            context.Dispose();

            if (context.GetType() == typeof(TelemetryContext))
                context = new TelemetryContext();

            context.Configuration.AutoDetectChangesEnabled = false;

            return context;
        }

        /// <summary>
        /// Method for generating a transaction with DB.
        /// </summary>
        /// <param name="lst">The list of telemetry data you want to add.</param>
        /// <param name="context">The context of the interaction with the database.</param>
        public void TransactionScope(TelemetryCollection lst, DbContext context)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    context.Configuration.AutoDetectChangesEnabled = false;

                    var count = 0;
                    foreach (var entityToInsert in lst.Collection)
                    {
                        ++count;
                        context = (TelemetryContext)AddToDb(context, entityToInsert, count, 3); // 3 - количество данных, которые приходят за 1 раз.
                    }

                    context.SaveChanges();
                }
                finally
                {
                    context?.Dispose();
                }

                scope.Complete();
            }
        }
    }
}

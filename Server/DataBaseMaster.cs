using System;
using System.Collections.Generic;
using System.Transactions;
using Server.Entities;

namespace Server
{
    /// <summary>
    /// The main component of the interaction with the database.
    /// </summary>
    internal class DataBaseMaster
    {
        /// <summary>
        /// Method for generating a transaction with DB.
        /// </summary>
        /// <param name="lst">The list of telemetry data you want to add.</param>
        public void InsertData(List<TelemetryEntity> lst)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 15, 0)))
            {
                TelemetryContext context = null;
                try
                {
                    context = new TelemetryContext();
                    context.Configuration.AutoDetectChangesEnabled = false;

                    int count = 0;
                    foreach (var entityToInsert in lst)
                    {
                        ++count;

                        context.Set<TelemetryEntity>().Add(entityToInsert);

                        if (count % 3 == 0) // 3 - количество данных, которые приходят за 1 раз.
                        {
                            context.SaveChanges();
                            context.Dispose();
                            context = new TelemetryContext();
                            context.Configuration.AutoDetectChangesEnabled = false;
                        }
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

using System;
using System.ServiceModel.Web;

using NLog;
using Server.Entities;

namespace Server
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            Logger.Info("Starting the process of creating a database");
            try
            {
                var tc = new TelemetryContext();
                tc.Database.Initialize(false);
                Logger.Info("The database was created successfully");

                using (var host = new WebServiceHost(typeof(Implementations.Data)))
                {
                    host.Open();
                    Logger.Info("Service started...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Console.ReadKey();
            }
        }
    }
}

using System;
using System.ServiceModel.Web;

using Server.Entities;
using NLog;

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

                using (var hostTerminal = new WebServiceHost(typeof(Services.Terminal)))
                using (var hostClient = new WebServiceHost(typeof(Services.Client)))
                {
                    hostTerminal.Open();
                    Logger.Info("Service Terminal started...");

                    hostClient.Open();
                    Logger.Info("Service Client started...");

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

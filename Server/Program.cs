using System;
using System.ServiceModel.Web;

using Server.Entities;
using Server.Services;
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

                using (var host = new WebServiceHost(typeof(Terminal)))
                {
                    host.Open();
                    Logger.Info("Terminal Service started...");
                }
                /*
                using (var host = new WebServiceHost(typeof(Client)))
                {
                    host.Open();
                    Logger.Info("Client Service started...");
                }
                */
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Console.ReadKey();
            }
        }
    }
}

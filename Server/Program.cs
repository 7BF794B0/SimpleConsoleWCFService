using System;
using System.ServiceModel;

using NLog;

namespace Server
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            try
            {
                using (var host = new ServiceHost(typeof(Contracts.Implementations.Data)))
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

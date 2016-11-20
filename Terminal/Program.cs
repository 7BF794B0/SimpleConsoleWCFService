using System;

namespace Terminal
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Console.Write("Enter your id:");
            int id = int.Parse(Console.ReadLine());

            try
            {
                using (var client = new ImpData.DataClient("NetTcpBinding_IData"))
                {
                    client.SendData(id);
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }
}

using System;
using System.Net;
using System.Threading;

namespace Terminal
{
    class Program
    {
        /// <summary>
        /// Создать новое подключение к серверу.
        /// </summary>
        /// <param name="obj">Id терминала</param>
        private static void NewConnection(object obj)
        {
            var request = (HttpWebRequest)WebRequest.Create($"http://localhost:8084/terminals/{(int)obj}");
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Console.WriteLine("StatusCode: {0}", response.StatusCode);
                response.Close();
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            int interval;
            Console.Write("Enter the value of re-treatment interval (in milliseconds): ");
            while (!int.TryParse(Console.ReadLine(), out interval))
                Console.Write("Error. Entered invalid value. Enter value of re-treatment interval (in milliseconds): ");

            int id;
            Console.Write("Enter your id: ");
            while (!int.TryParse(Console.ReadLine(), out id))
                Console.Write("Error. Entered invalid value. Enter your id: ");

            try
            {
                var tm = new TimerCallback(NewConnection);
                var timer = new Timer(tm, id, 0, interval);
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

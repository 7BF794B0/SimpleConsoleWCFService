using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

using Contracts;

namespace Terminal
{
    internal class Program
    {
        private static Timer _timer;
        private static readonly RandomGenerator Rnd = new RandomGenerator();

        /// <summary>
        /// Авторизоваться на сервере.
        /// </summary>
        /// <param name="terminalId"></param>
        private static void LogIn(int terminalId)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create($"http://localhost:8084/terminals/login/{terminalId}");
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    Console.WriteLine($"LogIn StatusCode: {response.StatusCode}.");
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Создать новое подключение к серверу.
        /// </summary>
        /// <param name="obj">Id терминала</param>
        private static void NewConnection(object obj)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create($"http://localhost:8084/terminals/{(int)obj}");
                request.ContentType = "application/json";
                request.Method = "POST";

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        var ser = new DataContractJsonSerializer(typeof(TelemetryCollection));
                        ser.WriteObject(ms, Rnd.Next());
                        string json = Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);

                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        if (stream == null)
                            throw new NullReferenceException();

                        using (var streamReader = new StreamReader(stream))
                        {
                            string resp = streamReader.ReadToEnd();
                            if (resp == "1")
                                Console.WriteLine($"SendData StatusCode: {ServiceStatusCode.GoodLogin}.");
                            else if (resp == "2")
                                throw new Exception($"SendData StatusCode: {ServiceStatusCode.BadData}.");
                            else
                                throw new Exception($"SendData StatusCode: {ServiceStatusCode.BadLogin}.");
                        }
                        response.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // Останавить таймер.
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            int interval;

            Console.Write("Enter the value of re-treatment interval (in seconds): ");
            while (!int.TryParse(Console.ReadLine(), out interval))
                Console.Write("Error. Entered invalid value. Enter value of re-treatment interval (in milliseconds): ");
            interval = interval*1000;

            int id;
            Console.Write("Enter your id: ");
            while (!int.TryParse(Console.ReadLine(), out id))
                Console.Write("Error. Entered invalid value. Enter your id: ");

            try
            {
                LogIn(id);

                Console.Write("Next?");
                Console.ReadKey();

                var tm = new TimerCallback(NewConnection);
                _timer = new Timer(tm, id, 0, interval);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}

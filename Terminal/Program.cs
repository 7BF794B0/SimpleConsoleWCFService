using System;
using System.Device.Location;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

using Infrastructure;

namespace Terminal
{
    class Program
    {
        private static Timer _timer;

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
                var tel = new Telemetry
                {
                    Time = DateTime.Now,
                    Coordinates = new GeoCoordinate(55.45, 37.36),
                    Speed = 40,
                    Engine = true,
                    TotalMileage = 100
                };

                var ser = new DataContractJsonSerializer(typeof(Telemetry));
                var mem = new MemoryStream();
                ser.WriteObject(mem, tel);
                string data = Encoding.UTF8.GetString(mem.ToArray(), 0, (int)mem.Length);
                var webClient = new WebClient
                {
                    Headers = { ["Content-type"] = "application/json" },
                    Encoding = Encoding.UTF8
                };
                Console.WriteLine($"StatusCode: {webClient.UploadString($"http://localhost:8084/terminals/{(int)obj}", "POST", data)}.");
            }
            catch (Exception ex)
            {
                // Останавить таймер.
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                Console.WriteLine(ex);
            }
        }

        private static void Test1(int id)
        {
            try
            {
                var ser = new DataContractJsonSerializer(typeof(Telemetry));
                var mem = new MemoryStream();
                var webClient = new WebClient
                {
                    Headers = { ["Content-type"] = "application/json" },
                    Encoding = Encoding.UTF8
                };

                var tel = new Telemetry
                {
                    Time = new DateTime(2016, 12, 4, 12, 0, 0),
                    Coordinates = new GeoCoordinate(59.938048, 30.3141581), // Дворцовая площадь
                    Speed = 0.0d,
                    Engine = true,
                    TotalMileage = 0.0d
                };

                ser.WriteObject(mem, tel);
                string data = Encoding.UTF8.GetString(mem.ToArray(), 0, (int)mem.Length);

                Console.WriteLine($"StatusCode: {webClient.UploadString($"http://localhost:8084/terminals/{id}", "POST", data)}.");
            }
            catch (Exception ex)
            {
                // Останавить таймер.
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                Console.WriteLine(ex);
            }
        }

        private static void Test2(int id)
        {
            try
            {
                var ser = new DataContractJsonSerializer(typeof(Telemetry));
                var mem = new MemoryStream();
                var webClient = new WebClient
                {
                    Headers = { ["Content-type"] = "application/json" },
                    Encoding = Encoding.UTF8
                };

                var tel = new Telemetry
                {
                    Time = new DateTime(2016, 12, 4, 12, 30, 0), // За 30 минут
                    Coordinates = new GeoCoordinate(60.1384927, 30.2175813), // Сертолово
                    Speed = 46.0d,
                    Engine = true,
                    TotalMileage = 23.0d // Прохали примерно 23 км
                };

                ser.WriteObject(mem, tel);
                string data = Encoding.UTF8.GetString(mem.ToArray(), 0, (int)mem.Length);

                Console.WriteLine($"StatusCode: {webClient.UploadString($"http://localhost:8084/terminals/{id}", "POST", data)}.");
            }
            catch (Exception ex)
            {
                // Останавить таймер.
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                Console.WriteLine(ex);
            }
        }

        private static void Test3(int id)
        {
            try
            {
                var ser = new DataContractJsonSerializer(typeof(Telemetry));
                var mem = new MemoryStream();
                var webClient = new WebClient
                {
                    Headers = { ["Content-type"] = "application/json" },
                    Encoding = Encoding.UTF8
                };

                var tel = new Telemetry
                {
                    Time = new DateTime(2016, 12, 4, 13, 0, 0), // За 30 минут
                    Coordinates = new GeoCoordinate(60.2059187, 29.7110901), // Зеленогорск
                    Speed = 58.0d,
                    Engine = false,
                    TotalMileage = 52.0d // Прохали примерно 29 км
                };

                ser.WriteObject(mem, tel);
                string data = Encoding.UTF8.GetString(mem.ToArray(), 0, (int)mem.Length);

                Console.WriteLine($"StatusCode: {webClient.UploadString($"http://localhost:8084/terminals/{id}", "POST", data)}.");
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
        static void Main()
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

                // Настоящий функционал
                //var tm = new TimerCallback(NewConnection);
                //_timer = new Timer(tm, id, 0, interval);

                // Тестовый функционал
                Test1(id);
                Test2(id);
                Test3(id);
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

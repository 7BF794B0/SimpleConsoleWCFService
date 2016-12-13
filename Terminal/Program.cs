using System;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

using Contracts;

namespace Terminal
{
    class Program
    {
        private static Timer _timer;
        private static bool _firstFlag = true;

        private static double CalculateSpeed(double time, double mileage)
        {
            return (mileage / 1000) / time;
        }

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

        private static void Test(int id)
        {
            try
            {
                TelemetryCollection _dataSet = new TelemetryCollection();

                Random rnd = new Random();
                Telemetry tel1;

                // Старотвая позиция используется 1 раз (в первый).
                double startLatitude = rnd.Next(10, 70);
                double startLongitude = rnd.Next(10, 70);
                // Рандомное значение времени в пути.
                DateTime randomTime;
                // Новые рандомные координаты.
                double newLatitude;
                double newLongitude;
                // Новая дистанция, которую мы проехали за время randomMinutes.
                double newDistanse;

                GeoCoordinate tempCoord;

                // Первый элемент списка в формирующийся коллекции должен знать о последнем элементе в предыдущей коллекции.
                if (_firstFlag)
                {
                    tel1 = new Telemetry
                    {
                        Time = DateTime.Now,
                        Coordinates = new GeoCoordinate(startLatitude, startLongitude),
                        SpeedKmh = 0.0d,
                        Engine = true,
                        TotalMileageKm = 0.0d
                    };
                    _dataSet.Collection.Add(tel1);
                    _firstFlag = false;
                }
                else
                {
                    randomTime = _dataSet.Collection.Last().Time.AddMinutes(rnd.Next(30, 90));
                    newLatitude = _dataSet.Collection.Last().Coordinates.Latitude + rnd.NextDouble();
                    newLongitude = _dataSet.Collection.Last().Coordinates.Longitude + rnd.NextDouble();
                    tempCoord = new GeoCoordinate(newLatitude, newLongitude);
                    newDistanse = _dataSet.Collection.Last().Coordinates.GetDistanceTo(tempCoord);

                    tel1 = new Telemetry
                    {
                        Time = randomTime,
                        Coordinates = new GeoCoordinate(newLatitude, newLongitude),
                        SpeedKmh = CalculateSpeed((randomTime - _dataSet.Collection.Last().Time).TotalHours, newDistanse),
                        Engine = true,
                        TotalMileageKm = (_dataSet.Collection.Last().TotalMileageKm + newDistanse) / 1000
                    };

                    _dataSet.Collection.Clear();
                    _dataSet.Collection.Add(tel1);
                }

                randomTime = _dataSet.Collection.Last().Time.AddMinutes(rnd.Next(30, 90));
                newLatitude = _dataSet.Collection.Last().Coordinates.Latitude + rnd.NextDouble();
                newLongitude = _dataSet.Collection.Last().Coordinates.Longitude + rnd.NextDouble();
                tempCoord = new GeoCoordinate(newLatitude, newLongitude);
                newDistanse = _dataSet.Collection.Last().Coordinates.GetDistanceTo(tempCoord);

                var tel2 = new Telemetry
                {
                    Time = randomTime,
                    Coordinates = new GeoCoordinate(newLatitude, newLongitude),
                    SpeedKmh = CalculateSpeed((randomTime - _dataSet.Collection.Last().Time).TotalHours, newDistanse),
                    Engine = true,
                    TotalMileageKm = (_dataSet.Collection.Last().TotalMileageKm + newDistanse) / 1000
                };
                _dataSet.Collection.Add(tel2);

                randomTime = _dataSet.Collection.Last().Time.AddMinutes(rnd.Next(30, 90));
                newLatitude = _dataSet.Collection.Last().Coordinates.Latitude + rnd.NextDouble();
                newLongitude = _dataSet.Collection.Last().Coordinates.Longitude + rnd.NextDouble();
                tempCoord = new GeoCoordinate(newLatitude, newLongitude);
                newDistanse = _dataSet.Collection.Last().Coordinates.GetDistanceTo(tempCoord);

                var tel3 = new Telemetry
                {
                    Time = randomTime,
                    Coordinates = new GeoCoordinate(newLatitude, newLongitude),
                    SpeedKmh = CalculateSpeed((randomTime - _dataSet.Collection.Last().Time).TotalHours, newDistanse),
                    Engine = true,
                    TotalMileageKm = (_dataSet.Collection.Last().TotalMileageKm + newDistanse) / 1000
                };
                _dataSet.Collection.Add(tel3);

                var request = (HttpWebRequest)WebRequest.Create($"http://localhost:8084/terminals/{id}");
                request.ContentType = "application/json";
                request.Method = "POST";

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        var ser = new DataContractJsonSerializer(typeof(TelemetryCollection));
                        ser.WriteObject(ms, _dataSet);
                        string json = Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);

                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
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
                RandomGenerator rnd = new RandomGenerator();

                var request = (HttpWebRequest)WebRequest.Create($"http://localhost:8084/terminals/(int){obj}");
                request.ContentType = "application/json";
                request.Method = "POST";

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        var ser = new DataContractJsonSerializer(typeof(TelemetryCollection));
                        ser.WriteObject(ms, rnd.Next());
                        string json = Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);

                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
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

                //var tm = new TimerCallback(NewConnection);
                //_timer = new Timer(tm, id, 0, interval);
                Test(id);
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

using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class ValidData
    {
        [TestMethod]
        public void SendValidData()
        {
            try
            {
                var tel1 = new Telemetry
                {
                    Time = new DateTime(2016, 12, 4, 12, 0, 0),
                    // Дворцовая площадь
                    Latitude = 59.938048,
                    Longitude = 30.3141581,
                    SpeedKmh = 0.0d,
                    Engine = true,
                    TotalMileageKm = 0.0d
                };

                var tel2 = new Telemetry
                {
                    Time = new DateTime(2016, 12, 4, 12, 30, 0), // За 30 минут
                    // Сертолово
                    Latitude = 60.1384927,
                    Longitude = 30.2175813,
                    SpeedKmh = 46.0d,
                    Engine = true,
                    TotalMileageKm = 23.0d // Прохали примерно 23 км
                };

                var tel3 = new Telemetry
                {
                    Time = new DateTime(2016, 12, 4, 13, 0, 0), // За 30 минут
                    // Зеленогорск
                    Latitude = 60.2059187,
                    Longitude = 29.7110901,
                    SpeedKmh = 58.0d,
                    Engine = false,
                    TotalMileageKm = 52.0d // Прохали примерно 29 км
                };

                TelemetryCollection dataSet = new TelemetryCollection();
                dataSet.Collection.Add(tel1);
                dataSet.Collection.Add(tel2);
                dataSet.Collection.Add(tel3);

                var request = (HttpWebRequest)WebRequest.Create(@"http://localhost:8084/terminals/4");
                request.ContentType = "application/json";
                request.Method = "POST";

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        var ser = new DataContractJsonSerializer(typeof(TelemetryCollection));
                        ser.WriteObject(ms, dataSet);
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
                        Assert.AreEqual("1", streamReader.ReadToEnd());
                    }
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}

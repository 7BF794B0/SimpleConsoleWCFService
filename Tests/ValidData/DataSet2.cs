using System;
using System.Device.Location;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.ValidData
{
    [TestClass]
    public class DataSet2
    {
        private DataContractJsonSerializer _ser;
        private MemoryStream _mem;
        private WebClient _webClient;

        /// <summary>
        /// Проинициализировать все общие объекты.
        /// </summary>
        private void Init()
        {
            _ser = new DataContractJsonSerializer(typeof(Telemetry));
            _mem = new MemoryStream();
            _webClient = new WebClient
            {
                Headers = { ["Content-type"] = "application/json" },
                Encoding = Encoding.UTF8
            };
        }

        [TestMethod]
        public void SendValidDataSet2()
        {
            try
            {
                Init();

                var tel = new Telemetry
                {
                    Time = new DateTime(2016, 12, 4, 12, 30, 0), // За 30 минут
                    Coordinates = new GeoCoordinate(60.1384927, 30.2175813), // Сертолово
                    Speed = 46,
                    Engine = true,
                    TotalMileage = 23 // Прохали примерно 23 км
                };

                _ser.WriteObject(_mem, tel);
                string data = Encoding.UTF8.GetString(_mem.ToArray(), 0, (int)_mem.Length);

                Assert.AreEqual("200", _webClient.UploadString("http://localhost:8084/terminals/4}", "POST", data));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}

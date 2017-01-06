using System;
using System.Collections.Generic;
using System.Device.Location;

using Contracts;
using Contracts.Interfaces;

using NLog;
using Server.Entities;

namespace Server.Implementations
{
    public class Data : IData
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly List<int> LoggedTerminals = new List<int>();

        private static GeoCoordinate _coordinate1;
        private static GeoCoordinate _coordinate2;

        private static int _count;

        private static readonly DataBaseMaster DbMaster = new DataBaseMaster();

        public void Login(string terminalId)
        {
            if (!LoggedTerminals.Contains(Convert.ToInt32(terminalId)))
                LoggedTerminals.Add(Convert.ToInt32(terminalId));

            Logger.Info($"The client with id: {terminalId} logged");
        }

        public ServiceStatusCode SendData(string terminalId, TelemetryCollection lst)
        {
            if (LoggedTerminals.Contains(Convert.ToInt32(terminalId)))
            {
                for (int i = 0; i < lst.Collection.Count - 1; i++)
                {
                    _coordinate1 = new GeoCoordinate(lst.Collection[i].Latitude, lst.Collection[i].Longitude);
                    _coordinate2 = new GeoCoordinate(lst.Collection[i + 1].Latitude, lst.Collection[i + 1].Longitude);

                    if (Math.Abs(_coordinate1.GetDistanceTo(_coordinate2) - (lst.Collection[i + 1].TotalMileageKm - lst.Collection[i].TotalMileageKm) * 1000) < 500000000000000.0d) // Где 500 - это погрешность в измерениях [метры].
                                                                                                                                                                            // Хотя тесты показали, что точность можно ужесточить и уменьшить до ~70.
                    {
                        // Время в пути.
                        var time = (lst.Collection[i + 1].Time - lst.Collection[i].Time).TotalHours;
                        // Пройденное расстояние.
                        var distance = lst.Collection[i + 1].TotalMileageKm - lst.Collection[i].TotalMileageKm;

                        // Проверка на соответствие скоростей и времени.
                        if (!(Math.Abs(lst.Collection[i + 1].SpeedKmh - distance / time) < 5000000000000)) // Где 5 - это погрешность в измерениях [км/ч]
                        {
                            Logger.Error($"The client with the id: {terminalId} sent is not the correct data.");
                            return ServiceStatusCode.BadData;
                        }
                    }
                    else
                    {
                        Logger.Error($"The client with the id: {terminalId} sent is not the correct data.");
                        return ServiceStatusCode.BadData;
                    }
                }

                Logger.Info($"The client has connected with id: {terminalId}");

                var context = new TelemetryContext();
                var transactionScope = new System.Threading.Tasks.Task(() => DbMaster.TransactionScope(lst, context));
                transactionScope.Start();

                foreach (Telemetry t in lst.Collection)
                {
                    Logger.Info("");
                    Logger.Info($"Telemetry, DataSet#: {++_count}");
                    Logger.Info($"Telemetry, Time: {t.Time}");
                    Logger.Info($"Telemetry, Latitude: {t.Latitude}");
                    Logger.Info($"Telemetry, Longitude: {t.Longitude}");
                    Logger.Info($"Telemetry, Speed: {t.SpeedKmh}");
                    Logger.Info($"Telemetry, Engine: {t.Engine}");
                    Logger.Info($"Telemetry, TotalMileage: {t.TotalMileageKm}");
                }

                return ServiceStatusCode.GoodLogin;
            }

            Logger.Error($"The client with the id: {terminalId} could not send the data because it has not been logged.");
            return ServiceStatusCode.BadLogin;
        }
    }
}
using System;
using System.Collections.Generic;

using Contracts;
using Contracts.Interfaces;
using NLog;

namespace Server.Implementations
{
    public class Data : IData
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly List<int> LoggedTerminals = new List<int>();

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
                    if (
                        Math.Abs(lst.Collection[i].Coordinates.GetDistanceTo(lst.Collection[i + 1].Coordinates) -
                                 (lst.Collection[i + 1].TotalMileageKm - lst.Collection[i].TotalMileageKm)*1000) < 500.0d) // Где 500 - это погрешность в измерениях [метры].
                                                                                                                           // Хотя тесты показали, что точность можно ужесточить и уменьшить до ~70.
                    {
                        // Время в пути.
                        var time = (lst.Collection[i + 1].Time - lst.Collection[i].Time).TotalHours;
                        // Пройденное расстояние.
                        var distance = lst.Collection[i + 1].TotalMileageKm - lst.Collection[i].TotalMileageKm;

                        // Проверка на соответствие скоростей и времени.
                        if (!(Math.Abs(lst.Collection[i + 1].SpeedKmh - distance / time) < 5)) // Где 5 - это погрешность в измерениях [км/ч]
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
                for (int i = 0; i < lst.Collection.Count; i++)
                {
                    Logger.Info("");
                    Logger.Info($"Telemetry, DataSet#: {i + 1}");
                    Logger.Info($"Telemetry, Time: {lst.Collection[i].Time}");
                    Logger.Info($"Telemetry, Coordinates: {lst.Collection[i].Coordinates}");
                    Logger.Info($"Telemetry, Speed: {lst.Collection[i].SpeedKmh}");
                    Logger.Info($"Telemetry, Engine: {lst.Collection[i].Engine}");
                    Logger.Info($"Telemetry, TotalMileage: {lst.Collection[i].TotalMileageKm}");
                }

                return ServiceStatusCode.GoodLogin;
            }

            Logger.Error($"The client with the id: {terminalId} could not send the data because it has not been logged.");
            return ServiceStatusCode.BadLogin;
        }
    }
}
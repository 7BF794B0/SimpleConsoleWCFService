using System;
using System.Collections.Generic;
using System.Linq;
using Contracts.Interfaces;
using Infrastructure;
using NLog;

namespace Server.Implementations
{
    public class Data : IData
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly List<int> LoggedTerminals = new List<int>();
        private static readonly List<Telemetry> DataSet = new List<Telemetry>();

        public void Login(string terminalId)
        {
            if (!LoggedTerminals.Contains(Convert.ToInt32(terminalId)))
                LoggedTerminals.Add(Convert.ToInt32(terminalId));

            Logger.Info($"The client with id:{terminalId} logged.");
        }

        public int SendData(string terminalId, Telemetry data)
        {
            if (LoggedTerminals.Contains(Convert.ToInt32(terminalId)))
            {
                var tel = new Telemetry
                {
                    Time = data.Time,
                    Coordinates = data.Coordinates,
                    Speed = data.Speed,
                    Engine = data.Engine,
                    TotalMileage = data.TotalMileage
                };
                DataSet.Add(tel);

                if (DataSet.Count == 1)
                {
                    Logger.Info("");
                    Logger.Info($"The client has connected with id: {terminalId}");
                    Logger.Info($"Telemetry, Time: {data.Time}");
                    Logger.Info($"Telemetry, Coordinates: {data.Coordinates}");
                    Logger.Info($"Telemetry, Speed: {data.Speed}");
                    Logger.Info($"Telemetry, Engine: {data.Engine}");
                    Logger.Info($"Telemetry, TotalMileage: {data.TotalMileage}");

                    return 200;
                }

                // Проверка на соответствие расстояний.
                if (
                    Math.Abs(DataSet[DataSet.Count - 2].Coordinates.GetDistanceTo(DataSet.Last().Coordinates) -
                             (DataSet.Last().TotalMileage - DataSet[DataSet.Count - 2].TotalMileage) * 1000) < 500.0d) // Где 500 - это погрешность в измерениях [метры].
                                                                                                                       // Хотя тесты показали, что точность можно ужесточить и уменьшить до ~70.
                {
                    // Время в пути.
                    var time = (DataSet.Last().Time - DataSet[DataSet.Count - 2].Time).TotalHours;
                    // Пройденное расстояние.
                    var distance = DataSet.Last().TotalMileage - DataSet[DataSet.Count - 2].TotalMileage;

                    // Проверка на соответствие скоростей и времени.
                    if (Math.Abs(DataSet.Last().Speed - distance/time) < 5) // Где 5 - это погрешность в измерениях [км/ч]
                    {
                        Logger.Info("");
                        Logger.Info($"The client has connected with id: {terminalId}");
                        Logger.Info($"Telemetry, Time: {data.Time}");
                        Logger.Info($"Telemetry, Coordinates: {data.Coordinates}");
                        Logger.Info($"Telemetry, Speed: {data.Speed}");
                        Logger.Info($"Telemetry, Engine: {data.Engine}");
                        Logger.Info($"Telemetry, TotalMileage: {data.TotalMileage}");

                        return 200;
                    }
                }
            }

            Logger.Error($"The client with the id: {terminalId} could not send the data because it has not been logged.");
            return 500;
        }
    }
}
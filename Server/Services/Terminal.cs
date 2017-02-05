using System;
using System.Collections.Generic;

using Contracts;
using Contracts.Interfaces;

using NLog;

namespace Server.Services
{
    internal class Terminal : ITerminal
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly List<int> _loggedTerminals = new List<int>();

        private readonly DataBaseMaster _dbMaster = new DataBaseMaster();

        public void Login(string terminalId)
        {
            if (!_loggedTerminals.Contains(Convert.ToInt32(terminalId)))
                _loggedTerminals.Add(Convert.ToInt32(terminalId));

            _logger.Info($"The terminal with id: {terminalId} logged");
        }

        public ServiceStatusCode SendData(string terminalId, TelemetryCollection lst)
        {
            if (_loggedTerminals.Contains(Convert.ToInt32(terminalId)))
            {
                for (int i = 0; i < lst.Collection.Count - 1; i++)
                {
                    if (
                        Math.Abs(lst.Collection[i].Coordinates.GetDistanceTo(lst.Collection[i + 1].Coordinates) -
                                 (lst.Collection[i + 1].TotalMileageKm - lst.Collection[i].TotalMileageKm)*1000) < 500000000000000.0d) // Где 500 - это погрешность в измерениях [метры].
                                                                                                                           // Хотя тесты показали, что точность можно ужесточить и уменьшить до ~70.
                    {
                        // Время в пути.
                        var time = (lst.Collection[i + 1].Time - lst.Collection[i].Time).TotalHours;
                        // Пройденное расстояние.
                        var distance = lst.Collection[i + 1].TotalMileageKm - lst.Collection[i].TotalMileageKm;

                        // Проверка на соответствие скоростей и времени.
                        if (!(Math.Abs(lst.Collection[i + 1].SpeedKmh - distance / time) < 5000000000000)) // Где 5 - это погрешность в измерениях [км/ч]
                        {
                            _logger.Error($"The terminal with the id: {terminalId} sent is not the correct data.");
                            return ServiceStatusCode.BadData;
                        }
                    }
                    else
                    {
                        _logger.Error($"The terminal with the id: {terminalId} sent is not the correct data.");
                        return ServiceStatusCode.BadData;
                    }
                }

                _logger.Info($"The terminal has connected with id: {terminalId}");

                new System.Threading.Tasks.Task(() => _dbMaster.InsertData(lst.ToEntities())).Start();

                _logger.Info("");
                _logger.Info($"Telemetry, TerminalId: {lst.TerminalId}");
                foreach (Telemetry t in lst.Collection)
                {
                    _logger.Info($"Telemetry, Time: {t.Time}");
                    _logger.Info($"Telemetry, Coordinates: {t.Coordinates}");
                    _logger.Info($"Telemetry, Speed: {t.SpeedKmh}");
                    _logger.Info($"Telemetry, Engine: {t.Engine}");
                    _logger.Info($"Telemetry, TotalMileage: {t.TotalMileageKm}");
                }

                return ServiceStatusCode.SuccessSend;
            }

            _logger.Error($"The terminal with the id: {terminalId} could not send the data because it has not been logged.");
            return ServiceStatusCode.BadLogin;
        }
    }
}
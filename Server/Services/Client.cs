using Contracts;
using Contracts.Interfaces;
using NLog;

namespace Server.Services
{
    internal class Client : IClient
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly DataBaseMaster _dbMaster = new DataBaseMaster();

        public int[] GetTerminalsInfo()
        {
            _logger.Info("The client has requested details of all relevant terminals");
            return _dbMaster.GetTerminalsInfo();
        }

        public TelemetryCollection GetDataByTerminalId(string terminalId)
        {
            _logger.Info($"The client has requested all the telemetry data from the terminal #{terminalId}");
            return _dbMaster.GetDataByTerminalId(int.Parse(terminalId)).ToBaseFormat();
        }
    }
}

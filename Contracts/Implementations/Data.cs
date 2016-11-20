using Contracts.Interfaces;
using NLog;

namespace Contracts.Implementations
{
    public class Data : IData
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void SendData(int id)
        {
            Logger.Info($"The client has connected with id: {id}");
        }
    }
}

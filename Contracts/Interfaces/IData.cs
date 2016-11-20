using System.ServiceModel;

namespace Contracts.Interfaces
{
    [ServiceContract]
    public interface IData
    {
        /// <summary>
        /// Зафиксировать подключение нового пользователя.
        /// </summary>
        /// <param name="id">Id терминала</param>
        [OperationContract]
        void SendData(int id);
    }
}

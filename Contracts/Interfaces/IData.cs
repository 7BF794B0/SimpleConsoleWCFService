using System.ServiceModel;
using System.ServiceModel.Web;
using Infrastructure;

namespace Contracts.Interfaces
{
    [ServiceContract]
    public interface IData
    {
        /// <summary>
        /// Зафиксировать подключение нового терминала.
        /// </summary>
        /// <param name="terminalId">Id терминала.</param>
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped, Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "terminals/login/{terminalId}")]
        [OperationContract]
        void Login(string terminalId);

        /// <summary>
        /// Получить данные от терминала.
        /// </summary>
        /// <param name="terminalId">Id терминала.</param>
        /// <param name="data">Телеметрические данные.</param>
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "terminals/{terminalId}")]
        [OperationContract]
        int SendData(string terminalId, Telemetry data);
    }
}

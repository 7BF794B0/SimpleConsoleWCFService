using System.ServiceModel;
using System.ServiceModel.Web;

namespace Contracts.Interfaces
{
    [ServiceContract]
    public interface IClient
    {
        /// <summary>
        /// Вернуть информацию о всех актуальных терминалах.
        /// </summary>
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "getterminalinfo")]
        [OperationContract]
        int[] GetTerminalsInfo();

        /// <summary>
        /// Вернуть все телеметрические данные конкретного терминала.
        /// </summary>
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "getdata/{terminalId}")]
        [OperationContract]
        TelemetryCollection GetDataByTerminalId(string terminalId);
    }
}

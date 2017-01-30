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
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped, Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "client/getterminalinfo/")]
        [OperationContract]
        string GetTerminalsInfo();
    }
}

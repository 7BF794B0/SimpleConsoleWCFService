using System.ServiceModel;
using System.ServiceModel.Web;

namespace Contracts.Interfaces
{
    [ServiceContract]
    public interface IData
    {
        /// <summary>
        /// Зафиксировать подключение нового пользователя.
        /// </summary>
        /// <param name="id">Id терминала</param>
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped, Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "terminals/{id}")]
        [OperationContract]
        void SendData(string id);
    }
}

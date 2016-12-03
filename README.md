# SimpleConsoleWCFService

Хотелось бы нормально реализовать обработку ситуации, когда терминал пытается отправить данные будучи еще не залогининым. При такой ситуации запрос должен отваливаться с 500 кодом.

Как я это попытался реализовать:

1. Изменяем контракт: меняем тип возвращаемого значения с ```void ``` на ```HttpResponseMessage ```
```cs
/// <summary>
/// Получить данные от терминала.
/// </summary>
/// <param name="terminalId">Id терминала.</param>
[WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped, Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "terminals/{terminalId}")]
[OperationContract]
HttpResponseMessage SendData(string terminalId);
```
2. Реализуем
```cs
public HttpResponseMessage SendData(string terminalId)
{
    if (loggedTerminals.Contains(Convert.ToInt32(terminalId)))
    {
        Logger.Info($"The client has connected with id: {terminalId}");
        return new HttpResponseMessage(HttpStatusCode.OK);
    }
    else
    {
        Logger.Error($"The client with the id: {terminalId} could not send the data because it has not been logged.");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
    }
}
```
3. В ответе получаем:
```
{"SendDataResult":{"Content":null,"ReasonPhrase":"Internal Server Error","RequestMessage":null,"StatusCode":500,"Version":{"_Build":-1,"_Major":1,"_Minor":1,"_Revision":-1}}}
```
4. Пытаемся обработать на стороне терминала:
```cs
var request = (HttpWebRequest)WebRequest.Create($"http://localhost:8084/terminals/{(int)obj}");
using (var response = (HttpWebResponse)request.GetResponse())
{
    using (var sr = new System.IO.StreamReader(response.GetResponseStream(), System.Text.Encoding.ASCII))
    {
        Console.WriteLine("StatusCode: {0}", JsonConvert.DeserializeObject<HttpResponseMessage>(sr.ReadToEnd()).StatusCode); // Доп. зависимость это не очень (
    }
    response.Close();
}
```
5. ???
6. Оно не работает и все равно пишет, что код 200 (и это даже понятно ведь ответ получен). И тут либо надо реально ронять сервер, либо реализовывать это все по другому.

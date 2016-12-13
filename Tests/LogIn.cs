using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class LogIn
    {
        /// <summary>
        /// Авторизоваться на сервере.
        /// </summary>
        [TestMethod]
        public void Authorize()
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("http://localhost:8084/terminals/login/4");
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}

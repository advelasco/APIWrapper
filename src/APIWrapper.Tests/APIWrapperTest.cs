using APIWrapper.Util.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace APIWrapper.Tests
{
    [TestClass]
    public class APIWrapperTest
    {
        #region ExtractQueryString Tests
        [TestMethod]
        public void ExtractQueryString_must_return_empty_when_parameters_are_null()
        {
            APIWrapper target = new APIWrapper(new AuthState
            {
                Token = "token",
                APIUrl = new Uri("https://www.mystore.com/api/v1")
            });

            Assert.AreEqual(String.Empty, target.ExtractQueryString(null));
        }

        [TestMethod]
        public void ExtractQueryString_must_return_formatted_string_when_parameters_are_valid()
        {
            NameValueCollection param = new NameValueCollection();
            param.Add("attr1", "10");
            param.Add("attr2", "20");

            APIWrapper target = new APIWrapper(new AuthState
            {
                Token = "token",
                APIUrl = new Uri("https://www.mystore.com/api/v1")
            });

            Assert.AreEqual("?attr1=10&attr2=20", target.ExtractQueryString(param));
        }
        #endregion

        #region Call tests
        [TestMethod]
        public void Call_with_HttpMethod_must_return_used_httpmethod_and_request_headers_correctly()
        {
            var mock = new Mock<HttpHelper>("https://www.mystore.com/api/v1");
            mock.SetupProperty(m => m.HttpWebRequest.Method);
            mock.SetupProperty(m => m.HttpWebRequest.ContentType);
            mock.SetupProperty(m => m.HttpWebRequest.Headers, new WebHeaderCollection());
            mock.Setup(m => m.HttpWebRequest.GetRequestStream()).Returns(new MemoryStream());
            mock.Setup(m => m.HttpWebRequest.GetResponse().GetResponseStream()).Returns(new MemoryStream());

            APIWrapper target = new APIWrapper(new AuthState
            {
                Token = "token",
                APIUrl = new Uri("https://www.mystore.com/api/v1")
            });

            target.Call(HttpMethods.GET, "departments", String.Empty, mock.Object);

            mock.VerifySet(h => h.HttpWebRequest.Method = "GET");
            mock.VerifySet(h => h.HttpWebRequest.ContentType = "application/json");

            Assert.AreEqual(mock.Object.HttpWebRequest.Headers.GetValues("Authorization").First().ToString(), "Bearer token");
        }

        [TestMethod]
        public void Call_with_HttpMethod_Post_must_write_content_on_request_body()
        {
            var content = "{" +
                                "\"id\": \"1\"," +
                                "\"name\": \"Department\"" +
                            "}";

            string writedString = string.Empty;
            var mockMS = new Mock<MemoryStream>();
            mockMS.Setup(m => m.CanWrite).Returns(true);
            mockMS.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback(
                (byte[] b, int i1, int i2) =>
                {
                    writedString = new System.Text.ASCIIEncoding().GetString(b, i1, i2);
                });


            var mock = new Mock<HttpHelper>("https://www.mystore.com/api/v1");
            mock.SetupProperty(m => m.HttpWebRequest.Method);
            mock.SetupProperty(m => m.HttpWebRequest.ContentType);
            mock.SetupProperty(m => m.HttpWebRequest.Headers, new WebHeaderCollection());
            mock.Setup(m => m.HttpWebRequest.GetRequestStream()).Returns(mockMS.Object);
            mock.Setup(m => m.HttpWebRequest.GetResponse().GetResponseStream()).Returns(new MemoryStream());

            APIWrapper target = new APIWrapper(new AuthState
            {
                Token = "token",
                APIUrl = new Uri("https://www.mystore.com/api/v1")
            });

            target.Call(HttpMethods.POST, "departments", content, mock.Object);

            mock.VerifySet(h => h.HttpWebRequest.Method = "POST");
            Assert.AreEqual(content, writedString);
        }

        [TestMethod]
        public void Call_with_HttpMethod_Put_must_write_content_on_request_body()
        {
            var content = "{" +
                                "\"id\": \"1\"," +
                                "\"name\": \"Department\"" +
                            "}";

            string writedString = string.Empty;
            var mockMS = new Mock<MemoryStream>();
            mockMS.Setup(m => m.CanWrite).Returns(true);
            mockMS.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback(
                (byte[] b, int i1, int i2) =>
                {
                    writedString = new System.Text.ASCIIEncoding().GetString(b, i1, i2);
                });


            var mock = new Mock<HttpHelper>("https://www.mystore.com/api/v1");
            mock.SetupProperty(m => m.HttpWebRequest.Method);
            mock.SetupProperty(m => m.HttpWebRequest.ContentType);
            mock.SetupProperty(m => m.HttpWebRequest.Headers, new WebHeaderCollection());
            mock.Setup(m => m.HttpWebRequest.GetRequestStream()).Returns(mockMS.Object);
            mock.Setup(m => m.HttpWebRequest.GetResponse().GetResponseStream()).Returns(new MemoryStream());

            APIWrapper target = new APIWrapper(new AuthState
            {
                Token = "token",
                APIUrl = new Uri("https://www.mystore.com/api/v1")
            });

            target.Call(HttpMethods.PUT, "departments", content, mock.Object);

            mock.VerifySet(h => h.HttpWebRequest.Method = "PUT");
            Assert.AreEqual(content, writedString);
        }
        #endregion

        #region GetResponse tests
        [TestMethod]
        public void GetResponse_must_return_StatusCode_200_when_response_is_valid()
        {
            var mock = new Mock<HttpWebResponseWrapper>();
            mock.Setup(m => m.StatusCode).Returns(HttpStatusCode.OK);
            mock.Setup(m => m.Headers).Returns(new WebHeaderCollection());
            mock.Setup(m => m.GetResponseStream()).Returns(new MemoryStream());

            APIWrapper target = new APIWrapper(new AuthState
            {
                Token = "token",
                APIUrl = new Uri("https://www.mystore.com/api/v1")
            });

            APIResponse apiResponse = target.GetResponse(mock.Object);

            Assert.AreEqual(200, apiResponse.StatusCode);
        }

        [TestMethod]
        public void GetResponse_must_return_Content_when_response_is_valid()
        {
            var content = "{" +
                                "\"count\": \"1000\" " +
                            "}";

            var mock = new Mock<HttpWebResponseWrapper>();
            mock.Setup(m => m.StatusCode).Returns(HttpStatusCode.OK);
            mock.Setup(m => m.Headers).Returns(new WebHeaderCollection());
            mock.Setup(m => m.GetResponseStream()).Returns(new MemoryStream(Encoding.Default.GetBytes(content)));

            APIWrapper target = new APIWrapper(new AuthState
            {
                Token = "token",
                APIUrl = new Uri("https://www.mystore.com/api/v1")
            });

            APIResponse apiResponse = target.GetResponse(mock.Object);

            Assert.AreEqual("1000", apiResponse.Content.count.Value);
        }

        [TestMethod]
        public void GetResponse_must_return_error_object_with_404_error_when_response_produce_error_404()
        {
            var content = "{" +
                            "\"responseCode\": \"404\"," +
                            "\"message\": \"notfound\"," +
                            "\"errors\": " +
                            "[" +
                                "{" +
                                    "\"message\": \"The requested resource was not found\"," +
                                    "\"code\": \"not_found\"" +
                                "}" +
                            "]" +
                          "}";

            var mock = new Mock<HttpWebResponseWrapper>();
            mock.Setup(m => m.StatusCode).Returns(HttpStatusCode.NotFound);
            mock.Setup(m => m.Headers).Returns(new WebHeaderCollection());
            mock.Setup(m => m.GetResponseStream()).Returns(new MemoryStream(Encoding.Default.GetBytes(content)));

            APIWrapper target = new APIWrapper(new AuthState
            {
                Token = "token",
                APIUrl = new Uri("https://www.mystore.com/api/v1")
            });

            APIResponse apiResponse = target.GetResponse(mock.Object);

            Assert.AreEqual(404, apiResponse.StatusCode);
            Assert.AreEqual("notfound", apiResponse.Error.Message);
            Assert.AreEqual("The requested resource was not found", apiResponse.Error.ErrorDetails.First().Message);
            Assert.AreEqual("not_found", apiResponse.Error.ErrorDetails.First().Code);
        }

        [TestMethod]
        public void GetResponse_must_return_StatusCode_404_when_response_content_produce_error_404_and_response_StatusCode_equals_200()
        {
            var content = "{" +
                              "\"responseCode\": \"404\"" +
                          "}";


            var mock = new Mock<HttpWebResponseWrapper>();
            mock.Setup(m => m.StatusCode).Returns(HttpStatusCode.OK);
            mock.Setup(m => m.Headers).Returns(new WebHeaderCollection());
            mock.Setup(m => m.GetResponseStream()).Returns(new MemoryStream(Encoding.Default.GetBytes(content)));

            APIWrapper target = new APIWrapper(new AuthState
            {
                Token = "token",
                APIUrl = new Uri("https://www.mystore.com/api/v1")
            });

            APIResponse apiResponse = target.GetResponse(mock.Object);

            Assert.AreEqual(404, apiResponse.StatusCode);
        }
        #endregion

    }
}

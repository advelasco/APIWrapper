using APIWrapper.Util.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;

namespace APIWrapper.Tests
{
    [TestClass]
    public class APIAuthorizerTest
    {
        #region Ctor Tests
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_must_not_accept_empty_clientid()
        {
            new APIAuthorizer("", "key", new Uri("https://mystore.com/manager"));
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_must_not_accept_empty_Secretkey()
        {
            new APIAuthorizer("client", "", new Uri("https://mystore.com/manager"));
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_must_not_accept_null_Secretkey()
        {
            new APIAuthorizer("client", "key", null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Constructor_must_not_accept_unsafe_storeurl()
        {
            new APIAuthorizer("client", "key", new Uri("http://mystore.com/manager"));
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Constructor_must_not_accept_storeurl_without_manager_path()
        {
            new APIAuthorizer("client", "key", new Uri("http://mystore.com"));
        }
        #endregion

        #region GetAuthorizationUrl Tests
        [TestMethod]
        public void GetAuthorizationUrl_must_return_url_with_scope_when_called_with_valid_scope()
        {
            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/manager"));

            Assert.AreEqual("https://www.mystore.com/manager/oauth/authorization?client_id=client&scope=readDepartment,writeOrder",
                target.GetAuthorizationUrl(new string[] { "readDepartment", "writeOrder" }));
        }

        [TestMethod]
        public void GetAuthorizationUrl_must_return_url_without_scope_when_called_without_scope_()
        {
            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/manager"));

            Assert.AreEqual("https://www.mystore.com/manager/oauth/authorization?client_id=client", target.GetAuthorizationUrl(null));
        }

        [TestMethod]
        public void GetAuthorizationUrl_must_return_url_without_additional_slash_when_storeurl_ends_with_slash()
        {
            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/manager/"));

            Assert.AreEqual("https://www.mystore.com/manager/oauth/authorization?client_id=client", target.GetAuthorizationUrl(null));
        }
        #endregion

        #region AuthorizationState Tests
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void AuthorizationState_must_not_accept_empty_code()
        {
            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/manager"));

            target.AuthorizationState("");
        }

        [TestMethod]
        public void AuthorizationState_must_define_post_method_and_content_type()
        {
            var mock = new Mock<HttpHelper>("https://www.mystore.com/manager")
                .SetupProperty(m => m.HttpWebRequest.Method);
            mock.Setup(m => m.HttpWebRequest.GetRequestStream()).Returns(new MemoryStream());
            mock.Setup(m => m.HttpWebRequest.GetResponse().GetResponseStream()).Returns(new MemoryStream());


            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/manager"));
            target.AuthorizationState("code", mock.Object);


            mock.VerifySet(h => h.HttpWebRequest.Method = "POST");
            mock.VerifySet(h => h.HttpWebRequest.ContentType = "application/x-www-form-urlencoded");
        }

        [TestMethod]
        public void AuthorizationState_must_define_parameters_on_request_body()
        {
            string writedString = string.Empty;
            var mockMS = new Mock<MemoryStream>();
            mockMS.Setup(m => m.CanWrite).Returns(true);
            mockMS.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback(
                (byte[] b, int i1, int i2) => {
                    writedString = new System.Text.ASCIIEncoding().GetString(b, i1, i2);
                });

            var mock = new Mock<HttpHelper>("https://www.mystore.com/manager")
                .SetupProperty(m => m.HttpWebRequest.Method);
            mock.Setup(m => m.HttpWebRequest.GetRequestStream()).Returns(mockMS.Object);
            mock.Setup(m => m.HttpWebRequest.GetResponse().GetResponseStream()).Returns(new MemoryStream());

            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/manager"));
            target.AuthorizationState("code", mock.Object);

            Assert.AreEqual("identification=client&secretKey=key&secretCode=code", writedString);
        }
        #endregion

        #region GetAccessToken
        [TestMethod]
        public void GetAccessToken_must_return_AcessToken_when_parameters_are_valid()
        {
            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/mystore"));
            var token = "{" +
                            "\"access_token\": \"token\" " +
                        "}";

            var result = target.GetAccessToken(token);
            Assert.AreEqual("token", result.Token);
            Assert.AreEqual("https://www.mystore.com/mystore/api/v1", result.APIUrl.ToString());
        }

        [TestMethod]
        public void GetAccessToken_must_return_AcessToken_null_when_parameters_are_invalid()
        {
            var target = new APIAuthorizer("client", "key", new Uri("https://www.mystore.com/manager"));

            Assert.IsNull(target.GetAccessToken(null));
            Assert.IsNull(target.GetAccessToken(""));
            Assert.IsNull(target.GetAccessToken(" { \"token\" : \"value\" }"));
        }
        #endregion
    }
}

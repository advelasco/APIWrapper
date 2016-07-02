using APIWrapper.Util.Http;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace APIWrapper
{
    /// <summary>
    /// This class is used to obtain the authorization
    /// from the shop customer to make api calls on their behalf
    /// </summary>
    public class APIAuthorizer
    {
        private readonly string _clientId;
        private readonly string _secretKey;
        private readonly Uri _clientUrl;

        /// <summary>
        /// Creates an instance of this class in order to obtain the authorization
        /// from the customer to make api calls on their behalf
        /// </summary>
        /// <param name="clientId">the unique api key of your client</param>
        /// <param name="secretKey">the unique secret key of your client</param>
        /// <param name="clientUrl">url of your client</param>
        public APIAuthorizer(string clientId, string secretKey, Uri clientUrl)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException("clientId can't be null or empty string");
            if (string.IsNullOrEmpty(secretKey))
                throw new ArgumentNullException("secretKey can't be null or empty string");
            if (clientUrl == null)
                throw new ArgumentNullException("clientUrl can't be a null string");
            if (clientUrl.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("clientUrl must have https");

            this._clientId = clientId;
            this._secretKey = secretKey;
            this._clientUrl = clientUrl;
        }

        /// <summary>
        /// Get the URL required by you to redirect the User to in which they will be
        /// presented with the ability to grant access to app with the specified scope
        /// </summary>
        /// <param name="scope">scopes to set permission</param>
        /// <returns>url to make a a redirect call</returns>
        public string GetAuthorizationUrl(string[] scope)
        {
            var authURL = new StringBuilder();

            authURL.AppendFormat("{0}/oauth/authorization", _clientUrl.AbsoluteUri.TrimEnd('/'));
            authURL.AppendFormat("?client_id={0}", _clientId);

            if (scope != null && scope.Length > 0)
            {
                authURL.AppendFormat("&scope={0}", string.Join(",", scope));
            }

            return authURL.ToString();
        }

        /// <summary>
        /// After the shop owner has authorized your app, shop will give you a code.
        /// Use this code to get your authorization state that you will use to make API calls
        /// </summary>
        /// <param name="code">a code given to you by shop</param>
        /// <returns>Authorization state needed by the API client to make API calls</returns>
        public AuthState AuthorizationState(string code)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentNullException("code can't be null or empty string");

            string url = String.Format("{0}/api/v1/oauth", _clientUrl.AbsoluteUri.TrimEnd('/'));

            return AuthorizationState(code, new HttpHelper(url));
        }

        public AuthState AuthorizationState(string code, HttpHelper httpHelper)
        {
            string postBody = String.Format("identification={0}&secretKey={1}&secretCode={2}",
                this._clientId,     // {0}
                this._secretKey,    // {1}
                code);              // {2}

            var request = httpHelper.HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(postBody);
                writer.Close();
            }

            string result = null;
            var response = (HttpWebResponseWrapper)request.GetResponse();

            using (Stream stream = response.GetResponseStream())
            {
                StreamReader sr = new StreamReader(stream);
                result = sr.ReadToEnd();
                sr.Close();
            }

            return GetAccessToken(result);
        }

        public AuthState GetAccessToken(string result)
        {
            if (!String.IsNullOrEmpty(result) && result.Contains("access_token"))
            {
                JObject jsonResult = JObject.Parse(result);
                return new AuthState
                {
                    Token = (string)jsonResult["access_token"],
                    APIUrl = new Uri(String.Format("{0}/api/v1", _clientUrl.AbsoluteUri.TrimEnd('/')))
                };
            }

            return null;
        }
    }
}

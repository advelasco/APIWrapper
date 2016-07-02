using APIWrapper.Util.Http;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace APIWrapper
{
    /// <summary>
    /// This class is your response message of your API
    /// </summary>
    public class APIResponse
    {
        public APIResponse()
        {
            this.Header = new NameValueCollection();
            this.Error = new ErrorResponse();
            this.Error.ErrorDetails = new List<ErrorDetail>();
        }

        public dynamic Content { get; set; }

        public NameValueCollection Header { get; set; }

        public ErrorResponse Error { get; set; }

        public int StatusCode { get; set; }
    }
}

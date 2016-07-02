using System.Collections.Generic;

namespace APIWrapper.Util.Http
{
    /// <summary>
    /// Class that represents a structure for error
    /// </summary>
    public class ErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<ErrorDetail> ErrorDetails { get; set; }
    }

    /// <summary>
    /// Class that represents a structure for error details
    /// </summary>
    public class ErrorDetail
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}

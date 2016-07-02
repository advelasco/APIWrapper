using Newtonsoft.Json;

namespace APIWrapper.Util.Translator
{
    /// <summary>
    /// This class is used to translate to and from C# object and JSON strings 
    /// </summary>
    public class JsonTranslator : ITranslator
    {
        /// <summary>
        /// The content type used by JSON
        /// </summary>
        public static readonly string ContentType = "application/json";

        /// <summary>
        /// Given a C# object, return a JSON string that can be used by the Shopify API
        /// </summary>
        /// <param name="data">a c# object that maps to a JSON object</param>
        /// <returns>JSON string</returns>
        public string Encode(object data)
        {
            return JsonConvert.SerializeObject(data);
        }

        /// <summary>
        /// Given a JSON String, return a corresponding C# object
        /// </summary>
        /// <param name="encodedData">JSON string return from the Shopify API</param>
        /// <returns>c# object corresponding to the JSON data return from the Shopify API</returns>
        public dynamic Decode(string encodedData)
        {
            return JsonConvert.DeserializeObject(encodedData);
        }

        /// <summary>
        /// The content type used by JSON
        /// </summary>
        /// <returns></returns>
        public string GetContentType()
        {
            return ContentType;
        }
    }
}

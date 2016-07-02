namespace APIWrapper.Util.Translator
{
    public interface ITranslator
    {
        dynamic Decode(string encodedData);
        string Encode(object data);
        string GetContentType();
    }
}
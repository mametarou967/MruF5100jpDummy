namespace MruF5100jpDummy.Model.Logging
{
    public interface ILogWriteRequester
    {
        void WriteRequest(LogLevel logLevel, string message);
    }
}

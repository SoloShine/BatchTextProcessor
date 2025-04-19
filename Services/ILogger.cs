
namespace BatchTextProcessor.Services
{
    public interface ILogger
    {
        void Log(string message);
        void LogError(string errorMessage);
    }
}


namespace BatchTextProcessor.Services
{
    public interface ILogger
    {
        void Log(string message);
        void LogWarning(string warningMessage);
        void LogError(string errorMessage);
    }
}

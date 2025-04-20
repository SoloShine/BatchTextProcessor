
namespace BatchTextProcessor.Services
{
    public class LoggerService : ILogger
    {
        public void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[INFO] {message}");
        }

        public void LogWarning(string warningMessage)
        {
            System.Diagnostics.Debug.WriteLine($"[WARN] {warningMessage}");
        }

        public void LogError(string errorMessage)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] {errorMessage}");
        }
    }
}

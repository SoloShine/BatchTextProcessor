
namespace BatchTextProcessor.Services
{
    public class LoggerService : ILogger
    {
        public void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[INFO] {message}");
        }

        public void LogError(string errorMessage)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] {errorMessage}");
        }
    }
}

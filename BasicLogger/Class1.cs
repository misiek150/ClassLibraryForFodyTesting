using System.IO;

namespace BasicLogger
{
    public class Logger
    {
        public string LogPath { get; private set; }

        public Logger(string logPath)
        {
            LogPath = logPath;
        }

        public void LogMessage(string message)
        {
            File.WriteAllText(LogPath, message);
        }
    }
}

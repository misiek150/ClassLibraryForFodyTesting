using System.IO;

namespace BasicLogger
{
    public class Logger
    {
        public string LogPath { get; private set; }

        public Logger(string logPath)
        {
            LogPath = logPath;
            File.WriteAllText(LogPath, "Logger initialized!");
        }

        public void LogMessage(string message)
        {
            File.WriteAllText(LogPath, message);
        }
    }
}

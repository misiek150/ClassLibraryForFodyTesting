using System;
using System.IO;

namespace BasicLogger
{
    public class Logger
    {
        public string LogPath { get; private set; }

        public Logger(string logPath)
        {
            LogPath = logPath;
            File.WriteAllText(LogPath, "Logger initialized! " + DateTime.Now.ToShortTimeString());
            //Console.WriteLine(LogPath + "Logger initialized! DDDy " + DateTime.Now.ToShortTimeString());
        }

        public void LogMessage(string message)
        {
            File.WriteAllText(LogPath, string.Format("{0} {1}", message, DateTime.Now.ToShortTimeString()));
        }
    }
}

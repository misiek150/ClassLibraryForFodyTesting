﻿using System;
using System.IO;

namespace BasicLogger
{
    public class Logger
    {
        public string LogPath { get; private set; }

        public Logger(string logPath)
        {
            LogPath = logPath;
            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine("Logger initialized! " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        public void LogMessage(string message)
        {

            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message));
            }
        }
    }
}

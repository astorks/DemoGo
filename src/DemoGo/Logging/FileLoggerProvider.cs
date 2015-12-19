using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DemoGo.Logging
{
    public class FileLoggerProvider : ILoggerProvider
    {
        string LogFilePath { get; set; }

        public FileLoggerProvider(string logFilePath)
        {
            LogFilePath = logFilePath;
        }

        public FileLoggerProvider() : this("./.log.txt") { }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, LogFilePath);
        }

        public void Dispose()
        {

        }
    }
}

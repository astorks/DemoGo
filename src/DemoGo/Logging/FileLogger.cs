using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DemoGo.Logging
{
    public class FileLogger : ILogger
    {
        public string CategoryName { get; }
        private string LogFilePath { get; }

        public FileLogger(string categoryName, string logFilePath)
        {
            CategoryName = categoryName;
            LogFilePath = logFilePath;
        }

        public IDisposable BeginScopeImpl(object state)
        {
            return new NoopDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.Verbose;
        }

        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            string message;
            var values = state as ILogValues;
            if (formatter != null)
            {
                message = formatter(state, exception);
            }
            else if (values != null)
            {
                message = LogFormatter.FormatLogValues(values);
                if (exception != null)
                {
                    message += Environment.NewLine + exception;
                }
            }
            else
            {
                message = LogFormatter.Formatter(state, exception);
            }

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            message = $"{ logLevel }: {message}";
            File.AppendAllText(LogFilePath, message + "\r\n");
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {

            }
        }
    }
}

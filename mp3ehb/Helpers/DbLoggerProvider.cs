using System;
using Microsoft.Extensions.Logging;

namespace mp3ehb.Helpers
{
    /// <summary>
    /// DbLoggerProvider class
    /// </summary>
    public class DbLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new DbLogger();
        }

        public void Dispose()
        {
        }

        private class DbLogger : ILogger
        {
            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Console.WriteLine(formatter(state, exception));
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
    
}

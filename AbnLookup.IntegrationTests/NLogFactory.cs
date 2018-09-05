using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using System;

namespace AbnLookup
{
    /// <summary>
    /// This loads up NLog so we can create loggers for our tests.
    /// </summary>
    public static class NLogFactory 
    {
        private static LoggerFactory _loggerFactory;

        private static bool _loggerLoaded;
        private static readonly Object _thisLock = new Object();

        // I am really not sure about this approach. I am begining to think that we should just use a singleton.
        public static ILogger<T> GetLogger<T>()
        {
            // Threading is evil.
            lock (_thisLock)
            {
                // Critical code section
                if (!_loggerLoaded)
                {
                    _loggerFactory = new LoggerFactory();

                    // Configure NLog
                    _loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });

                    LogManager.LoadConfiguration("nlog.config");
                }

                ILogger<T> logger = _loggerFactory.CreateLogger<T>();
                _loggerLoaded = true;

                return logger;
            }
        }

    }
}

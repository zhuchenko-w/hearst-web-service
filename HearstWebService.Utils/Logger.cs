using NLog;
using System;

namespace HearstWebService.Common
{
    public class Logger : Interfaces.ILogger
    {
        private static NLog.Logger _errorLogger = LogManager.GetLogger("ErrorLogger");
        private static NLog.Logger _infoLogger = LogManager.GetLogger("InfoLogger");

        public void Error(string message, Exception exception = null, string prefix = null)
        {
            message = ApplyPrefix(message, prefix);

            if (exception == null)
            {
                _errorLogger.Error(message);
            }
            else
            {
                _errorLogger.Error(exception, message);
            }
        }
        public void Info(string message, string prefix = null)
        {
            _infoLogger.Info(ApplyPrefix(message, prefix));
        }

        private string ApplyPrefix(string message, string prefix)
        {
            return string.IsNullOrEmpty(prefix) ? message : $"{prefix} {message}";
        }
    }
}

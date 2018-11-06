using HearstWebService.Common;
using HearstWebService.Interfaces;
using System;

namespace HearstWebService.BusinessLogic
{
    public abstract class BaseLogic
    {
        protected abstract string LogPrefix { get; }

        protected readonly Lazy<IDbAccessor> _dbAccessor;
        protected readonly Lazy<ILogger> _logger;

        public BaseLogic(Lazy<IDbAccessor> dbAccessor, Lazy<ILogger> logger)
        {
            _dbAccessor = dbAccessor;
            _logger = logger;
        }

        protected void LogAndThrow(string message, bool invalidParameter = false)
        {
            _logger.Value.Error(message, null, LogPrefix);
            throw invalidParameter ? new InvalidParameterException(message) : new Exception(message);
        }
    }
}

using HearstWebService.Interfaces;
using System;
using System.Collections.Generic;
using System.Web.Configuration;

namespace HearstWebService.Common.Helpers
{
    //TODO: replace with class with generic appsetting reading
    public class ConfigHelper
    {
        private const string ConnectionStringKey = "DbConnection";
        private const string ReportValidParamValuesCacheExpirationHoursKey = "ReportValidParamValuesCacheExpirationHours";
        private const string SettingValuesCacheExpirationHoursKey = "SettingValuesCacheExpirationHours";
        private const string DownloadFileMaxAttemptsKey = "DownloadFileMaxAttempts";
        private const string DownloadRetryWaitTimeMsKey = "DownloadRetryWaitTimeMs";
        private const string KindVgoValidValuesKey = "KindVGOValidValues";
        private const string FileStoragePathKey = "FileStoragePath";

        private string _connectionString; 
        private int? _reportValidParamValuesCacheExpirationHours;
        private int? _settingValuesCacheExpirationHours;
        private int? _downloadFileMaxAttempts;
        private int? _downloadRetryWaitTimeMs;
        private IList<int> _kindVgoValidValues;
        private string _fileStoragePath; 

        private readonly Lazy<ILogger> _logger;

        public string Name { get; private set; }

        public string ConnectionString => _connectionString ?? (_connectionString = ReadConfigValue(ConnectionStringKey, false));
        public int? ReportValidParamValuesCacheExpirationHours => _reportValidParamValuesCacheExpirationHours ?? (_reportValidParamValuesCacheExpirationHours = ReadIntegerSetting(ReportValidParamValuesCacheExpirationHoursKey));
        public int? SettingValuesCacheExpirationHours => _settingValuesCacheExpirationHours ?? (_settingValuesCacheExpirationHours = ReadIntegerSetting(SettingValuesCacheExpirationHoursKey));
        public int? DownloadFileMaxAttempts => _downloadFileMaxAttempts ?? (_downloadFileMaxAttempts = ReadIntegerSetting(DownloadFileMaxAttemptsKey));
        public int? DownloadRetryWaitTimeMs => _downloadRetryWaitTimeMs ?? (_downloadRetryWaitTimeMs = ReadIntegerSetting(DownloadRetryWaitTimeMsKey));
        public IList<int> KindVgoValidValues => _kindVgoValidValues ?? (_kindVgoValidValues = ReadListSetting<int>(KindVgoValidValuesKey));
        public string FileStoragePath => _fileStoragePath ?? (_fileStoragePath = ReadConfigValue(FileStoragePathKey));

        private ConfigHelper(Lazy<ILogger> logger)
        {
            _logger = logger;
            Name = Guid.NewGuid().ToString();
        }

        public static ConfigHelper Instance => Nested.instance;

        private class Nested
        {
            internal static readonly ConfigHelper instance = new ConfigHelper(new Lazy<ILogger>(()=>new Logger()));
        }

        private string ReadConfigValue(string key, bool isSetting = true)
        {
            var value = isSetting ? WebConfigurationManager.AppSettings[key] : WebConfigurationManager.ConnectionStrings[key]?.ConnectionString;

            if (value == null)
            {
                _logger.Value.Error($"{(isSetting ? "Setting" : "Connection string")} \"{key}\" not found");
            }

            return value;
        }
        private int? ReadIntegerSetting(string key)
        {
            var stringValue = ReadConfigValue(key);

            if (!string.IsNullOrEmpty(stringValue) && int.TryParse(stringValue, out int value))
            {
                return value;
            }
            else
            {
                _logger.Value.Error($"Value of setting \"{key}\" is not set or can not be parsed as integer value");
                return null;
            }
        }
        private bool? ReadBoolSetting(string key)
        {
            var stringValue = ReadConfigValue(key);

            if (!string.IsNullOrEmpty(stringValue) && bool.TryParse(stringValue, out bool value))
            {
                return value;
            }
            else
            {
                _logger.Value.Error($"Value of setting \"{key}\" is not set or can not be parsed as bool value");
                return null;
            }
        }
        private IList<T> ReadListSetting<T>(string key)
        {
            var stringValue = ReadConfigValue(key);

            if (!string.IsNullOrEmpty(stringValue))
            {
                var split = stringValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if(split.Length > 0)
                {
                    var list = new List<T>();
                    foreach (var item in split)
                    {
                        try
                        {
                            list.Add(ConvertValue<T>(item));
                        }
                        catch(Exception)
                        {
                            _logger.Value.Error($"Failed to parse \"{item}\" as {typeof(T).Name}");
                        }
                    }
                    return list;
                }
            }

            _logger.Value.Error($"Value of setting \"{key}\" is not set or can not be parsed as a list of {typeof(T).Name} values");
            return new List<T>();
        }
        private T ConvertValue<T>(string value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
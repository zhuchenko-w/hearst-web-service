using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System;
using HearstWebService.Data.Models;
using HearstWebService.Interfaces;
using HearstWebService.Data.Model;

namespace HearstWebService.Data.Accessor
{
    public class DbAccessor : IDbAccessor
    {
        private const string CachePrefix = "DbAccessor";
        private const int DefaultReportValidParamValuesCacheExpirationHours = 0;
        private const int DefaultSettingsCacheExpirationHours = 0;

        private readonly string _connectionString;
        private readonly int _reportValidParamValuesCacheExpirationHours;
        private readonly int _settingsCacheExpirationHours;
        private readonly Lazy<ICache> _cache;
        private readonly Lazy<ILogger> _logger;

        public DbAccessor(string connectionString, 
            int? reportValidParamValuesCacheExpirationHours, 
            int? settingsCacheExpirationHours, 
            Lazy<ICache> cache, 
            Lazy<ILogger> logger)
        {
            _connectionString = connectionString;
            _reportValidParamValuesCacheExpirationHours = reportValidParamValuesCacheExpirationHours ?? DefaultReportValidParamValuesCacheExpirationHours;
            _settingsCacheExpirationHours = settingsCacheExpirationHours ?? DefaultSettingsCacheExpirationHours;
            _cache = cache;
            _logger = logger;
        }

        public async Task<HashSet<string>> GetDistinctValidReportEntitiesAsync()
        {
            return await GetDistinctValidReportParamValues<string>("[dbo].[DimEntity]", "[EntityDesc]");
        }
        public async Task<HashSet<string>> GetDistinctValidReportYearsAsync()
        {
            return await GetDistinctValidReportParamValues<string>("[dbo].[DimYear]", "[YearCode]");
        }
        public async Task<HashSet<string>> GetDistinctValidReportScenariosAsync()
        {
            return await GetDistinctValidReportParamValues<string>("[dbo].[DimScenario]", "[ScenarioCode]");
        }

        public async Task<HashSet<string>> GetDistinctValidActualToPmScenariosAsync()
        {
            return await GetDistinctValues<string>("[dbo].[DimScenario]", "[ScenarioCode]", "[ScenarioID] NOT IN (1,2)");
        }
        public async Task<HashSet<string>> GetDistinctValidActualToPmYearsAsync()
        {
            return await GetDistinctValues<string>("[dbo].[DimYear]", "[YearCode]");
        }

        public async Task<List<DbSetting>> GetSettings()
        {
            var cacheKey = CachePrefix + "settings";
            try
            {
                var cacheValues = _cache.Value.GetItem<List<DbSetting>>(cacheKey);
                if (cacheValues != null)
                {
                    return cacheValues;
                }

                var settings = await GetValues(
                    "SELECT [SettingName], [SettingValue] FROM [dbo].[vSettings]",
                    async (reader) => new DbSetting
                    {
                        Name = await reader.GetFieldValueAsync<string>(0),
                        Value = await reader.GetFieldValueAsync<string>(1)
                    });
                _cache.Value.SetOrUpdateItem(cacheKey, TimeSpan.FromHours(_settingsCacheExpirationHours), settings);
                return settings;
            }
            catch (Exception ex)
            {
                _logger.Value.Error("Failed to get settings", ex);
                throw;
            }
        }
        public async Task<int> ExecuteDataTransferProcedureAsync(int batchNumber)
        {
            var query = $@"
                DECLARE @BatchNumber INT
                DECLARE @user NVARCHAR(32)

                SET @BatchNumber = {batchNumber}
                SET @user = SUSER_NAME()

                EXEC pTransferData @BatchNumber, @user    
            ";

            return await ExecuteQueryAsync(query);
        }

        public async Task<int> ExecuteStoredProcedureNonQueryAsync(string storedProcedureName, params StoredProcedureParameter[] parameters)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = storedProcedureName;

                        foreach (var parameter in parameters)
                        {
                            command.Parameters.Add(parameter.Name, parameter.Type).Value = parameter.Value;
                        }

                        return await command.ExecuteNonQueryAsync();
                    }                    
                }
            }
            catch (Exception ex)
            {
                _logger.Value.Error("An error occured while executing stored procedure", ex);
                throw;
            }
        }
        public async Task<DataTable> ExecuteStoredProcedureFillDataTableAsync(string storedProcedureName, params StoredProcedureParameter[] parameters)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var dataTable = new DataTable();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = storedProcedureName;

                        foreach (var parameter in parameters)
                        {
                            command.Parameters.Add(parameter.Name, parameter.Type).Value = parameter.Value;
                        }

                        using (var dataReader = await command.ExecuteReaderAsync())
                        {
                            dataTable.Load(dataReader);
                        }
                    }
                    
                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                _logger.Value.Error("An error occured while filling data table from stored procedure", ex);
                throw;
            }
        }

        private async Task<int> ExecuteQueryAsync(string query)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = query;

                        return await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Value.Error("An error occured while executing database query", ex);
                throw;
            }
        }
        private async Task<HashSet<T>> GetDistinctValidReportParamValues<T>(string tableName, string columnName)
        {
            var cacheKey = CachePrefix + tableName + columnName;
            var cacheValues = _cache.Value.GetItem<HashSet<T>>(cacheKey);

            if (cacheValues != null)
            {
                return cacheValues;
            }

            var values = await GetDistinctValues<T>(tableName, columnName);
            _cache.Value.SetOrUpdateItem(cacheKey, TimeSpan.FromHours(_reportValidParamValuesCacheExpirationHours), values);

            return values;
        }
        private async Task<HashSet<T>> GetDistinctValues<T>(string tableName, string columnName, string whereCondition = null)
        {
            try
            {
                var data = await GetValues(
                    $"SELECT DISTINCT {columnName} FROM {tableName}{(string.IsNullOrEmpty(whereCondition) ? "" : $" WHERE {whereCondition}")}", 
                    (reader) => reader.GetFieldValueAsync<T>(0));

                return new HashSet<T>(data);
            }
            catch (Exception ex)
            {
                _logger.Value.Error($"Failed to get distinct values. Table: {tableName}, column: {columnName}", ex);
                throw;
            }
        }
        private async Task<List<T>> GetValues<T>(string query, Func<SqlDataReader, Task<T>> readFunc)
        {
            var values = new List<T>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = query;

                        values.AddRange(await ReadValues(command, readFunc));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Value.Error("An error occured while accessing database", ex);
                throw;
            }

            return values;
        }
        private async Task<List<T>> ReadValues<T>(SqlCommand command, Func<SqlDataReader, Task<T>> readFunc)
        {
            var values = new List<T>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    values.Add(await readFunc(reader));
                }
            }

            return values;
        }
    }
}

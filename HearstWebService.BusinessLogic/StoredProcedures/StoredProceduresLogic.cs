using HearstWebService.Data.Models;
using HearstWebService.Interfaces;
using System;
using System.Data;
using System.Threading.Tasks;

namespace HearstWebService.BusinessLogic.StoredProcedures
{
    public class StoredProceduresLogic : BaseLogic, IStoredProceduresLogic
    {
        protected override string LogPrefix => "[Stored procedures log]";

        public StoredProceduresLogic(Lazy<IDbAccessor> dbAccessor, Lazy<ILogger> logger) 
            : base(dbAccessor, logger)
        {
        }

        public async Task<bool> ApproveData(int batchNumber)
        {
            var batchNumberParameter = new StoredProcedureParameter
            {
                Name = "@BatchNumber",
                Type = SqlDbType.Int,
                Value = batchNumber
            };
            return await ExecuteStoredProcedure("pApproveData", batchNumberParameter);
        }
        public async Task<bool> TransferData(int batchNumber)
        {
            try
            {
                var affectedRows = await _dbAccessor.Value.ExecuteDataTransferProcedureAsync(batchNumber);

                _logger.Value.Info($"pTransferData: {affectedRows} rows affected");

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> ActualToPm(string scenario, string year)
        {
            bool? paramsAreValid = null;
            try
            {
                paramsAreValid = (await _dbAccessor.Value.GetDistinctValidActualToPmScenariosAsync()).Contains(scenario) 
                    && (await _dbAccessor.Value.GetDistinctValidActualToPmYearsAsync()).Contains(year);
            }
            catch
            {
            }

            if (!paramsAreValid.HasValue)
            {
                LogAndThrow("Failed to get valid ActualToPm parameters");
            }
            else if (!paramsAreValid.Value)
            {
                LogAndThrow($"Invalid pActualToPm procedure parameters", true);
            }
            else
            {
                var scenarioParameter = new StoredProcedureParameter
                {
                    Name = "@Scenario",
                    Type = SqlDbType.NVarChar,
                    Value = scenario
                };
                var yearParameter = new StoredProcedureParameter
                {
                    Name = "@YearPL",
                    Type = SqlDbType.NVarChar,
                    Value = year
                };
                return await ExecuteStoredProcedure("pActualToPM", scenarioParameter, yearParameter);
            }

            return false;
        }
        public async Task<bool> LockScenario(string year, string scenario, int? actionValue)
        {
            var yearParameter = new StoredProcedureParameter
            {
                Name = "@Year",
                Type = SqlDbType.VarChar,
                Value = year
            };
            var scenarioParameter = new StoredProcedureParameter
            {
                Name = "@Scenario",
                Type = SqlDbType.VarChar,
                Value = scenario
            };
            var actionParameter = new StoredProcedureParameter
            {
                Name = "@Action",
                Type = SqlDbType.Int,
                Value = actionValue
            };

            return await ExecuteStoredProcedure("pLockScenario", yearParameter, scenarioParameter, actionParameter);
        }

        public async Task<DataTable> GetReportDataTable(ReportParameters parameters)
        {
            var spParameters = new[]
            {
                new StoredProcedureParameter
                {
                    Name = "@Entity",
                    Type = SqlDbType.NVarChar,
                    Value = parameters.Entity
                },new StoredProcedureParameter
                {
                    Name = "@Year",
                    Type = SqlDbType.NVarChar,
                    Value = parameters.Year
                },new StoredProcedureParameter
                {
                    Name = "@Scenario",
                    Type = SqlDbType.NVarChar,
                    Value = parameters.Scenario
                },new StoredProcedureParameter
                {
                    Name = "@KindVGO",
                    Type = SqlDbType.Int,
                    Value = parameters.KindVgo ?? 0
                }
            };

            try
            {
                return await _dbAccessor.Value.ExecuteStoredProcedureFillDataTableAsync("pHearstReport", spParameters);
            }
            catch
            {
                return null;
            }
        }

        private async Task<bool> ExecuteStoredProcedure(string storedProcedureName, params StoredProcedureParameter[] parameters)
        {
            try
            {
                var affectedRows = await _dbAccessor.Value.ExecuteStoredProcedureNonQueryAsync(storedProcedureName, parameters);

                _logger.Value.Info($"{storedProcedureName}: {affectedRows} rows affected");

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

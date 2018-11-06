using HearstWebService.Data.Model;
using HearstWebService.Data.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace HearstWebService.Interfaces
{
    public interface IDbAccessor
    {
        Task<List<DbSetting>> GetSettings();
        Task<int> ExecuteStoredProcedureNonQueryAsync(string storedProcedureName, params StoredProcedureParameter[] parameters);
        Task<DataTable> ExecuteStoredProcedureFillDataTableAsync(string storedProcedureName, params StoredProcedureParameter[] parameters);
        Task<HashSet<string>> GetDistinctValidReportEntitiesAsync();
        Task<HashSet<string>> GetDistinctValidReportYearsAsync();
        Task<HashSet<string>> GetDistinctValidReportScenariosAsync();
        Task<HashSet<string>> GetDistinctValidActualToPmScenariosAsync();
        Task<HashSet<string>> GetDistinctValidActualToPmYearsAsync();
        Task<int> ExecuteDataTransferProcedureAsync(int batchNumber);
    }
}

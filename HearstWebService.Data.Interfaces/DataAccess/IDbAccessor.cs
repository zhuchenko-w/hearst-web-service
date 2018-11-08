using HearstWebService.Data.Model;
using HearstWebService.Data.Models;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace HearstWebService.Interfaces
{
    public interface IDbAccessor
    {
        Task<List<DbSetting>> GetSettings(SafeAccessTokenHandle accessToken);
        Task<int> ExecuteStoredProcedureNonQueryAsync(SafeAccessTokenHandle accessToken, string storedProcedureName, params StoredProcedureParameter[] parameters);
        Task<DataTable> ExecuteStoredProcedureFillDataTableAsync(SafeAccessTokenHandle accessToken, string storedProcedureName, params StoredProcedureParameter[] parameters);
        Task<HashSet<string>> GetDistinctValidReportEntitiesAsync(SafeAccessTokenHandle accessToken);
        Task<HashSet<string>> GetDistinctValidReportYearsAsync(SafeAccessTokenHandle accessToken);
        Task<HashSet<string>> GetDistinctValidReportScenariosAsync(SafeAccessTokenHandle accessToken);
        Task<HashSet<string>> GetDistinctValidActualToPmScenariosAsync(SafeAccessTokenHandle accessToken);
        Task<HashSet<string>> GetDistinctValidActualToPmYearsAsync(SafeAccessTokenHandle accessToken);
        Task<int> ExecuteDataTransferProcedureAsync(SafeAccessTokenHandle accessToken, int batchNumber);
    }
}

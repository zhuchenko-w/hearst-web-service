using HearstWebService.Data.Models;
using Microsoft.Win32.SafeHandles;
using System.Data;
using System.Threading.Tasks;

namespace HearstWebService.Interfaces
{
    public interface IStoredProceduresLogic
    {
        Task<bool> ApproveData(SafeAccessTokenHandle accessToken, int batchNumber);
        Task<bool> TransferData(SafeAccessTokenHandle accessToken, int batchNumber);
        Task<bool> ActualToPm(SafeAccessTokenHandle accessToken, string scenario, string year);
        Task<bool> LockScenario(SafeAccessTokenHandle accessToken, string year, string scenario, int? actionValue);
        Task<DataTable> GetReportDataTable(SafeAccessTokenHandle accessToken, ReportParameters parameters);
    }
}

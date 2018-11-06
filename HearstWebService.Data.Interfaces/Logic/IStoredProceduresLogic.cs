using HearstWebService.Data.Models;
using System.Data;
using System.Threading.Tasks;

namespace HearstWebService.Interfaces
{
    public interface IStoredProceduresLogic
    {
        Task<bool> ApproveData(int batchNumber);
        Task<bool> TransferData(int batchNumber);
        Task<bool> ActualToPm(string scenario, string year);
        Task<bool> LockScenario(string year, string scenario, int? actionValue);
        Task<DataTable> GetReportDataTable(ReportParameters parameters);
    }
}

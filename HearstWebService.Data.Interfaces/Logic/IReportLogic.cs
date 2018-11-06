using HearstWebService.Data.Models;
using System.Threading.Tasks;

namespace HearstWebService.Interfaces
{
    public interface IReportLogic
    {
        Task<string> CreateReport(ReportParameters parameters);
    }
}

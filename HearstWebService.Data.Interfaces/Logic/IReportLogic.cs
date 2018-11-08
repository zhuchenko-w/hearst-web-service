using HearstWebService.Data.Models;
using Microsoft.Win32.SafeHandles;
using System.Threading.Tasks;

namespace HearstWebService.Interfaces
{
    public interface IReportLogic
    {
        Task<string> CreateReport(SafeAccessTokenHandle accessToken, ReportParameters parameters);
    }
}

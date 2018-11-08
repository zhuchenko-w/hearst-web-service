using HearstWebService.Common;
using HearstWebService.Common.Helpers;
using HearstWebService.Data.Models;
using HearstWebService.Interfaces;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace HearstWebService.Controllers
{
    [Authorize]
    public class ReportController : BaseApiController
    {
        private const string ExternalIcPostfix = "_External_IC";

        private readonly Lazy<IReportLogic> _reportLogic;

        public ReportController(Lazy<IReportLogic> reportLogic, Lazy<ILogger> logger)
            : base(logger)
        {
            _reportLogic = reportLogic;
        }

        [Route("ExecutePackage/{entity?}/{year?}/{scenario?}/{kindvgo?}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetReport([FromUri]ReportParameters parameters)
        {
            try
            {
                var filePath = await _reportLogic.Value.CreateReport(((WindowsIdentity)RequestContext.Principal.Identity).AccessToken, parameters);
                return GetOutputFileAsResponse(filePath, GetOutputReportFilename(filePath, parameters.Scenario, parameters.Year, parameters.KindVgo));
            }
            catch (InvalidParameterException ex)
            {
                return HandleExceptionResult(HttpStatusCode.BadRequest, "Invalid parameter value", ex);
            }
            catch (Exception ex)
            {
                return HandleExceptionResult(HttpStatusCode.InternalServerError, "Failed to create report", ex);
            }
        }

        [Route("CreateReport/{entity?}/{year?}/{scenario?}/{kindvgo?}", Name = "CreateReport")]
        [HttpGet]
        public async Task<HttpResponseMessage> CreateReport([FromUri]ReportParameters parameters)
        {
            try
            {
                var filePath = await _reportLogic.Value.CreateReport(((WindowsIdentity)RequestContext.Principal.Identity).AccessToken, parameters);
                var fileId = FileStorageHelper.CopyFileToStorage(filePath, GetOutputReportFilename(filePath, parameters.Scenario, parameters.Year, parameters.KindVgo));

                return Request.CreateResponse(HttpStatusCode.OK, fileId);
            }
            catch (InvalidParameterException ex)
            {
                return HandleExceptionMessage(HttpStatusCode.BadRequest, "Invalid parameter value", ex);
            }
            catch (Exception ex)
            {
                return HandleExceptionMessage(HttpStatusCode.InternalServerError, "Failed to create report", ex);
            }
        }

        private string GetOutputReportFilename(string filePath, string scenario, string year, int? kindVgo)
        {
            var fixedYear = Regex.Replace(year, "^FY", "20");
            var filename = $"{scenario}_{fixedYear}_{Regex.Replace(Path.GetFileNameWithoutExtension(filePath), "^Template_V4_", "")}";
            var postfix = kindVgo == 2 ? ExternalIcPostfix : "";

            return filename + postfix + Path.GetExtension(filePath);
        }
    }
}
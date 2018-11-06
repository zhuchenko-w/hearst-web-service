using HearstWebService.Common.Helpers;
using HearstWebService.Interfaces;
using System;
using System.IO;
using System.Net;
using System.Web.Http;

namespace HearstWebService.Controllers
{
    [Authorize]
    public class DownloadsController : BaseApiController
    {
        public DownloadsController(Lazy<ILogger> logger)
            : base(logger)
        {
        }

        [Route("DownloadFile/{fileId?}")]
        [HttpGet]
        public IHttpActionResult DownloadFile(Guid fileId)
        {
            try
            {
                var filePath = FileStorageHelper.FindFile(fileId);
                return GetOutputFileAsResponse(filePath, FileStorageHelper.GetClearFileName(Path.GetFileName(filePath), fileId));
            }
            catch (Exception ex)
            {
                return HandleExceptionResult(HttpStatusCode.InternalServerError, "Failed to download file", ex);
            }
        }
    }
}
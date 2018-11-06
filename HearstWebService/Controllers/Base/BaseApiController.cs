using HearstWebService.Common.Helpers;
using HearstWebService.Interfaces;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace HearstWebService.Controllers
{
    public class BaseApiController : ApiController
    {
        protected const string InvalidParameterPageUrl = "/InvalidParameter";
        protected const string ForbiddenPageUrl = "/Forbidden";
        protected const string NotFoundPageUrl = "/NotFoundPage";
        protected const string ErrorPageUrl = "/Error";

        private const int DownloadFileMaxAttemptsDefault = 10;
        private const int DownloadRetryWaitTimeMsDefault = 1000;

        protected readonly Lazy<ILogger> _logger;

        public BaseApiController(Lazy<ILogger> logger)
        {
            _logger = logger;
        }

        protected async Task<T> RunImpersonated<T>(Func<Task<T>> func)
        {
            //using (var identityContext = ((WindowsIdentity)RequestContext.Principal.Identity).Impersonate())
            //{
            //    return await func();
            //}
            return await WindowsIdentity.RunImpersonated(new Microsoft.Win32.SafeHandles.SafeAccessTokenHandle(WindowsIdentity.GetCurrent().Token), func);
        }

        protected IHttpActionResult GetOutputFileAsResponse(string filePath, string outputFileName = null)
        {
            var attempts = ConfigHelper.Instance.DownloadFileMaxAttempts ?? DownloadFileMaxAttemptsDefault;

            while (attempts-- > 0)
            {
                try
                {
                    using (var fileStream = File.OpenRead(filePath))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            memoryStream.SetLength(fileStream.Length);
                            fileStream.Read(memoryStream.GetBuffer(), 0, (int)fileStream.Length);

                            var result = new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new ByteArrayContent(memoryStream.GetBuffer())
                            };
                            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                            {
                                FileName = string.IsNullOrEmpty(outputFileName) ? Path.GetFileName(filePath) : outputFileName
                            };
                            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                            var response = ResponseMessage(result);

                            return response;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Value.Error($"Exception occured while trying to send file in response [{attempts} attempt(s) left]", ex);
                    Thread.Sleep(ConfigHelper.Instance.DownloadRetryWaitTimeMs ?? DownloadRetryWaitTimeMsDefault);
                }
            }

            return HandleExceptionResult(HttpStatusCode.InternalServerError, "Failed to send file in response");
        }

        protected IHttpActionResult HandleExceptionResult(HttpStatusCode statusCode = HttpStatusCode.InternalServerError, string message = null, Exception ex = null)
        {
            if (!string.IsNullOrEmpty(message) || ex != null)
            {
                _logger.Value.Error(message ?? "", ex);
            }

            return statusCode == HttpStatusCode.BadRequest 
                ? (IHttpActionResult)BadRequest(message) 
                : (IHttpActionResult)InternalServerError(new Exception(message, ex));
        }

        protected HttpResponseMessage HandleExceptionMessage(HttpStatusCode statusCode = HttpStatusCode.InternalServerError, string message = null, Exception ex = null)
        {
            if (!string.IsNullOrEmpty(message) || ex != null)
            {
                _logger.Value.Error(message ?? "", ex);
            }

            return new HttpResponseMessage(statusCode);
        }
    }
}
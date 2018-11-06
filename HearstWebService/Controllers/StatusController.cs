using HearstWebService.Data.Models;
using HearstWebService.Models;
using System;
using System.IO;
using System.Web.Mvc;

namespace HearstWebService.Controllers
{
    [Authorize]
    [RoutePrefix("Status")]
    public class StatusController : Controller
    {
        [Route("ExecutePackage/{entity?}/{year?}/{scenario?}/{kindvgo?}")]
        [HttpGet]
        public ActionResult CreateReportStatus(ReportParameters parameters)
        {
            return GetView(new StatusModel {
                RequestType = RequestTypes.CreateReport,
                Url = GetUrlFromRoot(Url.HttpRouteUrl("CreateReport", new { entity = parameters.Entity, year = parameters.Year, scenario = parameters.Scenario, kindvgo = parameters.KindVgo })),
            });
        }

        [Route("ExecuteSP/ApproveData/{batchNumber:int}")]
        [HttpGet]
        public ActionResult ApproveDataStatus(int batchNumber)
        {
            return GetView(new StatusModel
            {
                RequestType = RequestTypes.ApproveData,
                Url = GetUrlFromRoot(Url.HttpRouteUrl("ApproveData", new { batchNumber })),
            });
        }

        [Route("ExecuteSP/ActualToPm/{scenario:maxlength(10)?}/{year:maxlength(10)?}")]
        [HttpGet]
        public ActionResult ActualToPmStatus(string scenario, string year)
        {
            return GetView(new StatusModel
            {
                RequestType = RequestTypes.ActualToPm,
                Url = GetUrlFromRoot(Url.HttpRouteUrl("ActualToPm", new { scenario, year })),
            });
        }

        [Route("ExecuteSP/TransferData/{batchNumber:int}")]
        [HttpGet]
        public ActionResult TransferDataStatus(int batchNumber)
        {
            return GetView(new StatusModel
            {
                RequestType = RequestTypes.TransferData,
                Url = GetUrlFromRoot(Url.HttpRouteUrl("TransferData", new { batchNumber })),
            });
        }

        [Route("ExecuteSP/LockScenario/{year:maxlength(10)?}/{scenario:maxlength(10)?}/{actionValue:int?}")]
        [HttpGet]
        public ActionResult LockScenarioStatus(string year, string scenario, int? actionValue)
        {
            return GetView(new StatusModel
            {
                RequestType = RequestTypes.LockScenario,
                Url = GetUrlFromRoot(Url.HttpRouteUrl("LockScenario", new { year, scenario, actionValue })),
            });
        }

        private ActionResult GetView(StatusModel model)
        {
            return View("Status", model);
        }

        private string GetUrlFromRoot(string localUrl) {
            return Path.Combine(Request.Url.GetLeftPart(UriPartial.Authority) + Request.ApplicationPath, localUrl);
        }
    }
}
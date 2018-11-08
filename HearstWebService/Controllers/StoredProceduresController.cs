﻿using System.Web.Http;
using System.Threading.Tasks;
using System;
using HearstWebService.Interfaces;
using HearstWebService.Common;
using System.Net;
using System.Security.Principal;

namespace HearstWebService.Controllers
{
    [Authorize]
    [RoutePrefix("ExecuteSP")]
    public class StoredProceduresController : BaseApiController
    {
        private readonly Lazy<IStoredProceduresLogic> _storedProceduresLogic;

        public StoredProceduresController(Lazy<IStoredProceduresLogic> storedProceduresLogic, Lazy<ILogger> logger)
            : base(logger)
        {
            _storedProceduresLogic = storedProceduresLogic;
        }

        [Route("ApproveData/{batchNumber:int}", Name = "ApproveData")]
        [HttpGet]
        public async Task<IHttpActionResult> ApproveData(int batchNumber)
        {
            return await _storedProceduresLogic.Value.ApproveData(((WindowsIdentity)RequestContext.Principal.Identity).AccessToken, batchNumber) ? Ok() : HandleExceptionResult();
        }

        [Route("ActualToPm/{scenario:maxlength(10)?}/{year:maxlength(10)?}", Name = "ActualToPm")]
        [HttpGet]
        public async Task<IHttpActionResult> ActualToPm([FromUri]string scenario = null, [FromUri]string year = null)
        {
            try
            {
                return await _storedProceduresLogic.Value.ActualToPm(((WindowsIdentity)RequestContext.Principal.Identity).AccessToken, scenario, year) ? Ok() : HandleExceptionResult();
            }
            catch (InvalidParameterException ex)
            {
                return HandleExceptionResult(HttpStatusCode.BadRequest, "Invalid parameter value", ex);
            }
            catch (Exception ex)
            {
                return HandleExceptionResult(HttpStatusCode.InternalServerError, "Failed to call procedure", ex);
            }
        }

        [Route("TransferData/{batchNumber:int}", Name = "TransferData")]
        [HttpGet]
        public async Task<IHttpActionResult> TransferData(int batchNumber)
        {
            return await _storedProceduresLogic.Value.TransferData(((WindowsIdentity)RequestContext.Principal.Identity).AccessToken, batchNumber) ? Ok() : HandleExceptionResult();
        }

        [Route("LockScenario/{year:maxlength(10)?}/{scenario:maxlength(10)?}/{actionValue:int?}", Name = "LockScenario")]
        [HttpGet]
        public async Task<IHttpActionResult> LockScenario(string year, string scenario, int? actionValue)
        {
            return await _storedProceduresLogic.Value.LockScenario(((WindowsIdentity)RequestContext.Principal.Identity).AccessToken, year, scenario, actionValue) ? Ok() : HandleExceptionResult();
        }
    }
}
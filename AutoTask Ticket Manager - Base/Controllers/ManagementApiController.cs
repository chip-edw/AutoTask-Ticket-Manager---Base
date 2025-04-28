using Asp.Versioning;
using AutoTaskTicketManager.Common.Models;
using AutoTaskTicketManager.Services;
using AutoTaskTicketManager_Base.ManagementAPI;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AutoTaskTicketManager_Base.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ManagementApiController : ControllerBase
    {
        private readonly IManagementService _managementService;

        public ManagementApiController(IManagementService managementService)
        {
            _managementService = managementService;
        }


        #region AutoTaskCompanies/CountInSql
        /// <summary>
        /// Returns the number of companies stored in the ATTMS CustomerSettings table.
        /// </summary>
        [HttpGet("AutoTaskCompanies/CountInSql")]
        public async Task<ActionResult<ApiResponse<object>>> GetCompanyCountInSql()
        {
            Log.Debug("Inbound Management API: GetCompanyCountInSql");

            try
            {
                int count = await _managementService.GetCompanyCountAsync();

                var response = new { CompanyCount = count };
                return Ok(ApiResponse<object>.Ok(response));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving Company Count from SQL.");
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while retrieving the Company Count."));
            }
        }
        #endregion


        #region TicketProcessing/SenderExclusionList
        /// <summary>
        /// Returns the list of Sender Exclusions used to prevent automatic ticket creation.
        /// </summary>
        /// <returns>JSON array of sender exclusion email addresses</returns>
        [HttpGet("TicketProcessing/SenderExclusionList")]
        public ActionResult<ApiResponse<IEnumerable<string>>> GetSenderExclusionList()
        {
            Log.Debug("Inbound Management API (Controller): Return Sender Exclusion List");

            try
            {
                var senderExclusionList = ExclusionService.GetSenderExclusions();

                if (senderExclusionList == null || senderExclusionList.Count == 0)
                {
                    return NotFound(ApiResponse<string>.Fail("No Sender Exclusions found."));
                }

                return Ok(ApiResponse<IEnumerable<string>>.Ok(senderExclusionList));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Sender Exclusions.");
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while retrieving Sender Exclusions."));
            }
        }
        #endregion


        #region TicketProcessing/SubjectExclusionList
        /// <summary>
        /// Gets the list of email Subject Keywords that result in bypassing ticket creation for that specific email
        /// </summary>
        /// <returns></returns>
        [HttpGet("TicketProcessing/SubjectExclusionList")]
        public ActionResult<ApiResponse<IEnumerable<string>>> GetSubjectExclusionList()
        {
            Log.Debug("Inbound Management API (Controller): Return Subject Exclusion Keyword List for Ticket Processing");

            try
            {
                var exclusionList = ExclusionService.GetSubjectExclusions(); //StartupConfiguration.subjectExclusionKeyWordList;

                if (exclusionList == null || exclusionList.Count == 0)
                {
                    return NotFound(ApiResponse<string>.Fail("No Subject Exclusion Keywords found."));
                }

                return Ok(ApiResponse<IEnumerable<string>>.Ok(exclusionList));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Subject Exclusion Keywords.");
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while retrieving Subject Exclusion Keywords."));
            }
        }
        #endregion
    }
}

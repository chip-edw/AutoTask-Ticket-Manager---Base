using Asp.Versioning;
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

        /// <summary>
        /// Returns the number of companies stored in the ATTMS CustomerSettings table.
        /// </summary>
        [HttpGet("AutoTaskCompanies/CountInSql")]
        public async Task<ActionResult<object>> GetCompanyCountInSql()
        {
            Log.Debug("Inbound Management API: GetCompanyCountInSql");

            try
            {
                int count = await _managementService.GetCompanyCountAsync();
                return Ok(new { CompanyCount = count });
            }
            catch (Exception ex)
            {
                Log.Error("Error retrieving Company Count from SQL. Exception: {ExceptionMessage}", ex.Message);
                return StatusCode(500, "An error occurred while retrieving the Company Count.");
            }
        }

        /// <summary>
        /// Returns the list of Sender Exclusions used to prevent automatic ticket creation.
        /// </summary>
        /// <returns>JSON array of sender exclusion email addresses</returns>
        [HttpGet("TicketProcessing/SenderExclusionList")]
        public ActionResult<IEnumerable<string>> GetSenderExclusionList()
        {
            Log.Debug("\nInbound Management API (Controller): Return Sender Exclusion List");

            try
            {
                var senderExclusionList = StartupConfiguration.senderExclusionsList;

                if (senderExclusionList == null || senderExclusionList.Count == 0)
                {
                    return NotFound("No Sender Exclusions found.");
                }

                return Ok(senderExclusionList);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to retrieve Sender Exclusions. Exception: {ExceptionMessage}", ex.Message);
                return StatusCode(500, "An error occurred while retrieving Sender Exclusions.");
            }
        }

        [HttpGet("TicketProcessing/SubjectExclusionList")]
        public async Task<ActionResult<IEnumerable<string>>> GetSubjectExclusionList()
        {
            Log.Debug("\nInbound Management API (Controller): Return Subject Exclusion Keyword List for Ticket Processing");

            try
            {
                var exclusionList = StartupConfiguration.subjectExclusionKeyWordList;

                if (exclusionList == null || exclusionList.Count == 0)
                {
                    return NotFound("No Subject Exclusion Keywords found.");
                }

                return Ok(exclusionList);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to retrieve Subject Exclusion Keywords. Exception: {ExceptionMessage}", ex.Message);
                return StatusCode(500, "An error occurred while retrieving Subject Exclusion Keywords.");
            }
        }


    }
}

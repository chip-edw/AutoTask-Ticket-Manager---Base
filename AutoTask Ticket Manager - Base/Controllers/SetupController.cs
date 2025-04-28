using Asp.Versioning;
using AutoTaskTicketManager.Common.Models;
using AutoTaskTicketManager_Base.Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AutoTaskTicketManager_Base.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/setup")]
    public class SetupController : ControllerBase
    {

        #region generate-encryption-key
        /// <summary>
        /// Used to generate the API Key. 
        /// For security it is managed by middleware to ensure it can only be accesses locally and not remotely.
        /// </summary>
        /// <param name="masterSecret"></param>
        /// <returns></returns>
        [HttpPost("generate-encryption-key")]
        public ActionResult<ApiResponse<object>> GenerateEncryptionKey([FromBody] string masterSecret)
        {
            Log.Debug("Inbound Setup API (Controller): Generate Encryption Key");

            if (string.IsNullOrWhiteSpace(masterSecret))
            {
                return BadRequest(ApiResponse<string>.Fail("Master secret must not be empty."));
            }

            try
            {
                var encryptedKey = EncryptionUtility.Encrypt(masterSecret);
                var response = new { EncryptedKey = encryptedKey };

                return Ok(ApiResponse<object>.Ok(response));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating encryption key.");
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while generating the encryption key."));
            }
        }

        #endregion


        #region test-localhost-access
        /// <summary>
        /// Basic endpoint for testing the maintenance API
        /// </summary>
        /// <returns></returns>
        [HttpGet("test-localhost-access")]
        public ActionResult<ApiResponse<object>> TestLocalhostAccess()
        {
            var response = new { Status = "OK", Message = "Localhost access confirmed" };
            return Ok(ApiResponse<object>.Ok(response));
        }
        #endregion

    }
}

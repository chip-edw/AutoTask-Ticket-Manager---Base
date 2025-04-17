using AutoTaskTicketManager_Base.Common.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace AutoTaskTicketManager_Base.Controllers
{
    [ApiController]
    [Route("api/setup")]
    public class SetupController : ControllerBase
    {
        [HttpPost("generate-encryption-key")]
        public IActionResult GenerateEncryptionKey([FromBody] string masterSecret)
        {
            if (string.IsNullOrWhiteSpace(masterSecret))
            {
                return BadRequest("Master secret must not be empty.");
            }

            var encryptedKey = EncryptionUtility.Encrypt(masterSecret);
            return Ok(new { EncryptedKey = encryptedKey });
        }

        [HttpGet("test-localhost-access")]
        public IActionResult TestLocalhostAccess()
        {
            return Ok(new { status = "OK", message = "Localhost access confirmed" });
        }

    }
}

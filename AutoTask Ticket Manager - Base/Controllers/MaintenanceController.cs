using AutoTaskTicketManager_Base;
using AutoTaskTicketManager_Base.AutoTaskAPI;
using AutoTaskTicketManager_Base.Models;
using AutoTaskTicketManager_Base.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[ApiController]
[Route("api/v1/maintenance")]
public class MaintenanceController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICompanySettingsService _companySettingsService;


    public MaintenanceController(ApplicationDbContext dbContext, ICompanySettingsService companySettingsService)
    {
        _dbContext = dbContext;
        _companySettingsService = companySettingsService;
    }

    #region Reload Startup Lists and Dictionaries
    [HttpPost("reload")]
    public async Task<IActionResult> ReloadData([FromBody] ReloadRequest request)
    {
        Log.Information("Maintenance reload triggered.");

        if (request.MasterReload)
        {
            StartupConfiguration.LoadSenderExclusionListFromSQL(_dbContext);
            StartupConfiguration.LoadSubjectExclusionKeyWordsFromSQL(_dbContext);
            StartupConfiguration.LoadSupportDistros(_dbContext);
            StartupConfiguration.LoadAutoAssignCompanies(_dbContext);
            StartupConfiguration.LoadAutoAssignSenders(_dbContext);

            ////Loads all active Autotask Companies from the AutoTask API into Companies.companies Dictionary
            AutotaskAPIGet.GetAutoTaskCompanies();

            //Load Open tickets into Dictionary
            AutotaskAPIGet.GetNotCompletedTickets();

            //Load Active AutoTask Resources into Dictionary
            AutotaskAPIGet.GetAutoTaskActiveResources();
            // Add any other reloads here...
        }
        else
        {
            if (request.ReloadExclusions)
            {
                StartupConfiguration.LoadSenderExclusionListFromSQL(_dbContext);
                StartupConfiguration.LoadSubjectExclusionKeyWordsFromSQL(_dbContext);
            }

            if (request.ReloadSupportDistros)
            {
                StartupConfiguration.LoadSupportDistros(_dbContext);
            }

            if (request.ReloadCompanies)
            {
                StartupConfiguration.LoadAutoAssignCompanies(_dbContext);
            }

            if (request.ReloadResources)
            {
                StartupConfiguration.LoadAutoAssignSenders(_dbContext);
            }

            // Add more sections as needed...
        }

        return Ok(new { Message = "Reload completed successfully" });
    }
    #endregion


    #region CompanySettings
    /// <summary>
    /// Get all company settings from SQL.
    /// </summary>
    [HttpGet("companysettings")]
    public async Task<ActionResult<IEnumerable<CustomerSettings>>> GetCompanySettings()
    {
        var companies = await _companySettingsService.GetAllCompanySettingsFromSqlAsync();
        return Ok(companies);
    }
    #endregion

    [HttpPut("companysettings/update")]
    public async Task<ActionResult<List<CustomerSettings>>> UpdateCompanySettingsBatch([FromBody] List<CustomerSettings> companies)
    {
        if (companies == null || companies.Count == 0)
        {
            return BadRequest("No company settings provided.");
        }

        await _companySettingsService.UpdateCompanySettingsBatchAsync(companies);

        return Ok(companies);
    }


    [HttpPost("companysettings/refreshcompanymemory")]
    public async Task<IActionResult> RefreshMemory()
    {
        AutoTaskTicketManager_Base.AutoTaskAPI.AutotaskAPIGet.GetAutoTaskCompanies();
        return Ok("Memory refreshed successfully.");
    }



}

public class ReloadRequest
{
    public bool MasterReload { get; set; }
    public bool ReloadExclusions { get; set; }
    public bool ReloadSupportDistros { get; set; }
    public bool ReloadCompanies { get; set; }
    public bool ReloadResources { get; set; }
}

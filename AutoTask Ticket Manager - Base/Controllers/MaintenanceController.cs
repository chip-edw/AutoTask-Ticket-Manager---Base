using AutoTaskTicketManager_Base;
using AutoTaskTicketManager_Base.AutoTaskAPI;
using AutoTaskTicketManager_Base.Dtos;
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
    private readonly ISenderExclusionService _senderService;
    private readonly ISubjectExclusionService _subjectService;

    public MaintenanceController(
        ApplicationDbContext dbContext,
        ICompanySettingsService companySettingsService,
        ISenderExclusionService senderService,
        ISubjectExclusionService subjectService)
    {
        _dbContext = dbContext;
        _companySettingsService = companySettingsService;
        _senderService = senderService;
        _subjectService = subjectService;
    }

    #region Reload Startup Lists and Dictionaries

    /// <summary>
    /// Reloads memory-based configuration from the database and external sources.
    /// Performs full or partial reload based on request flags.
    /// </summary>
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

            AutotaskAPIGet.GetAutoTaskCompanies();
            AutotaskAPIGet.GetNotCompletedTickets();
            AutotaskAPIGet.GetAutoTaskActiveResources();
        }
        else
        {
            if (request.ReloadExclusions)
            {
                StartupConfiguration.LoadSenderExclusionListFromSQL(_dbContext);
                StartupConfiguration.LoadSubjectExclusionKeyWordsFromSQL(_dbContext);
            }

            if (request.ReloadSupportDistros)
                StartupConfiguration.LoadSupportDistros(_dbContext);

            if (request.ReloadCompanies)
                StartupConfiguration.LoadAutoAssignCompanies(_dbContext);

            if (request.ReloadResources)
                StartupConfiguration.LoadAutoAssignSenders(_dbContext);
        }

        return Ok(new { Message = "Reload completed successfully" });
    }

    #endregion

    #region CompanySettings

    /// <summary>
    /// Retrieves all company settings from the database.
    /// </summary>
    [HttpGet("companysettings")]
    public async Task<ActionResult<IEnumerable<CustomerSettings>>> GetCompanySettings()
    {
        var companies = await _companySettingsService.GetAllCompanySettingsFromSqlAsync();
        return Ok(companies);
    }

    /// <summary>
    /// Updates company settings for one or more records in the database.
    /// </summary>
    [HttpPut("companysettings/update")]
    public async Task<ActionResult<List<CustomerSettings>>> UpdateCompanySettingsBatch([FromBody] List<CustomerSettings> companies)
    {
        if (companies == null || companies.Count == 0)
            return BadRequest("No company settings provided.");

        await _companySettingsService.UpdateCompanySettingsBatchAsync(companies);
        return Ok(companies);
    }

    /// <summary>
    /// Refreshes the in-memory dictionary of AutoTask companies from the AutoTask API.
    /// </summary>
    [HttpPost("companysettings/refreshcompanymemory")]
    public async Task<IActionResult> RefreshMemory()
    {
        AutotaskAPIGet.GetAutoTaskCompanies();
        return Ok("Memory refreshed successfully.");
    }

    #endregion

    #region SenderExclusions

    /// <summary>
    /// Retrieves all sender email addresses from the SenderExclusion table in the database.
    /// </summary>
    [HttpGet("sender-exclusions")]
    public async Task<IActionResult> GetSenderExclusions() =>
        Ok(await _senderService.GetAllAsync());

    /// <summary>
    /// Adds a new sender email address to the SenderExclusion table in the database.
    /// </summary>
    [HttpPost("sender-exclusions")]
    public async Task<IActionResult> AddSenderExclusion([FromBody] SenderExclusionDto dto) =>
        Ok(await _senderService.AddAsync(dto));

    /// <summary>
    /// Deletes a sender email exclusion entry by ID from the database.
    /// </summary>
    [HttpDelete("sender-exclusions/{id}")]
    public async Task<IActionResult> DeleteSenderExclusion(int id) =>
        Ok(await _senderService.DeleteAsync(id));

    #endregion

    #region SubjectExclusions

    /// <summary>
    /// Retrieves all subject exclusion keywords from the SubjectExclusionKeyword table in the database.
    /// </summary>
    [HttpGet("subject-exclusions")]
    public async Task<IActionResult> GetSubjectExclusions() =>
        Ok(await _subjectService.GetAllAsync());

    /// <summary>
    /// Adds a new subject keyword to the SubjectExclusionKeyword table in the database.
    /// </summary>
    [HttpPost("subject-exclusions")]
    public async Task<IActionResult> AddSubjectExclusion([FromBody] SubjectExclusionKeywordDto dto) =>
        Ok(await _subjectService.AddAsync(dto));

    /// <summary>
    /// Deletes a subject exclusion keyword by ID from the SubjectExclusionKeyword table.
    /// </summary>
    [HttpDelete("subject-exclusions/{id}")]
    public async Task<IActionResult> DeleteSubjectExclusion(int id) =>
        Ok(await _subjectService.DeleteAsync(id));

    #endregion
}

/// <summary>
/// Defines reload options for the maintenance reload endpoint.
/// </summary>
public class ReloadRequest
{
    public bool MasterReload { get; set; }
    public bool ReloadExclusions { get; set; }
    public bool ReloadSupportDistros { get; set; }
    public bool ReloadCompanies { get; set; }
    public bool ReloadResources { get; set; }
}

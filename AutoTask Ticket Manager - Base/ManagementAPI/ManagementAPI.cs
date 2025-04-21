using AutoTaskTicketManager_Base.AutoTaskAPI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Serilog;
using System.Text.Json;

//using AutoTaskTicketManager_Base.Models;
//using using AutoTaskTicketManager_Base.MSGraphAPI;
//using System.Net;
//using Microsoft.EntityFrameworkCore;


namespace AutoTaskTicketManager_Base.ManagementAPI
{
    public class ManagementAPI
    {
        private readonly Worker _workerService;

        //Using Dependancy Injection to get an instance of the Worker Service so I can manage the cancellation token
        public ManagementAPI(Worker workerService)
        {
            _workerService = workerService;
        }

        public async Task TriggerDoWorkAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            await _workerService.DoWorkAsync(cancellationTokenSource.Token);
        }

        public static void Map(IEndpointRouteBuilder endpoints)
        {
            #region ShutDown Application Remotely
            endpoints.MapGet("/ATTMS/ShutDownApp", async context =>
            {
                try
                {
                    Log.Information("API call to Shutdown the application received.");

                    // Resolve the application lifetime service
                    var lifetime = context.RequestServices.GetService<IHostApplicationLifetime>();

                    lifetime?.StopApplication();

                    await context.Response.WriteAsync("Application restart initiated.");
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to ShutDown the application: {ex}");
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Failed to ShutDown the application.");
                }
            });

            #endregion

            #region Restart Application Remotely

            endpoints.MapGet("/ATTMS/RestartApp", async context =>
            {
                try
                {
                    Log.Information("API call to restart the application received.");

                    // Get the path to the current executable
                    var executablePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;

                    if (!string.IsNullOrEmpty(executablePath))
                    {
                        Log.Information($"Restarting application: {executablePath}");

                        // Start a new process to restart the application
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = executablePath,
                            UseShellExecute = true,
                        });

                        // Stop the current application
                        var lifetime = context.RequestServices.GetService<IHostApplicationLifetime>();
                        lifetime?.StopApplication();

                        await context.Response.WriteAsync("Application restart initiated.");
                    }
                    else
                    {
                        Log.Error("Executable path not found.");
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Executable path not found. Restart failed.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to restart the application: {ex}");
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Failed to restart the application.");
                }
            });


            #endregion

            #region Company Count from companies dictionary
            //Return the current list of companies contained in the Companies.companies dictionary
            endpoints.MapGet("/AutoTaskCompanies/CompanyList", async context =>
            {
                string maintApiMessage = "In Bound Maint. API to: Return the current list of companies contained in the Companies.companies dictionary";
                Log.Debug(maintApiMessage);

                List<KeyValuePair<Int64, object[]>> companiesList = Companies.GetCompaniesListforManagementAPI();
                var response = new { GetCompaniesListforManagementAPI = companiesList };
                var json = JsonSerializer.Serialize(response);
                context.Response.ContentType = "application/json";

                try
                {
                    await context.Response.WriteAsync(json);
                    Log.Debug("Transfer to Client Successful - Management API - /AutoTaskCompanies/CompanyList -" +
                        "Return the current list of companies contained in the Companies.companies dictionary");
                }

                catch (Exception ex)
                {
                    Log.Debug($"Transfer to Client Errored - Management API - /AutoTaskCompanies/CompanyList - " +
                        $"Return the current list of companies contained in the Companies.companies dictionary -{ex}");
                }

            });
            #endregion

            #region Return Company Count from DataBase
            //Return Company Count from the SQL DB
            endpoints.MapGet("/AutoTaskCompanies/CountInSql", static async context =>
            {
                string maintApiMessage = "In Bound Maint. API to: Return Company Count from the SQL DB";
                Log.Debug("\n");
                Log.Debug(maintApiMessage);

                var countInSql = ManagementApiHelper.GetCompanyCountFromSql();
                var response = new { CompanyCount = countInSql };
                var json = JsonSerializer.Serialize(response);
                context.Response.ContentType = "application/json";

                try
                {
                    await context.Response.WriteAsync(json);
                    Log.Debug("Transfer to Client Successful - Management API - /AutoTaskCompanies/CountInSql");
                }

                catch (Exception ex)
                {
                    Log.Debug($"Transfer to Client Errored - Management API - /AutoTaskCompanies/CountInSql - {ex}");
                }

            });
            #endregion

            #region Return Active Company Count from AutoTask API
            ////Return Company Count from the AutoTask API
            //endpoints.MapGet("/AutoTaskCompanies/Count", async context =>
            //{
            //    string maintApiMessage = "In Bound Maint. API to: Return Company Count from the AutoTask API";
            //    Log.Debug("\n");
            //    Log.Debug(maintApiMessage);

            //    var count = AutotaskAPIGet.GetCompanyCount();
            //    var response = new { CompanyCount = count };
            //    var json = JsonSerializer.Serialize(response);
            //    context.Response.ContentType = "application/json";

            //    try
            //    {
            //        await context.Response.WriteAsync(json);
            //        Log.Debug("Transfer to Client Successful - Management API - /AutoTaskCompanies/Count from AT API");
            //    }

            //    catch (Exception ex)
            //    {
            //        Log.Debug($"Transfer to Client Errored - Management API - /AutoTaskCompanies/Count from AT API - {ex}");
            //    }

            //});
            #endregion

            #region Return Company information from the AutoTask Company ID
            ////Return Company information from the AutoTask Company ID
            //endpoints.MapPost("/AutoTaskCompanies/CompanyInfo", async context =>
            //{
            //    string maintApiMessage = "In Bound Maint. API to: Return Company Info";
            //    Log.Debug(maintApiMessage);

            //    try
            //    {
            //        using var streamReader = new StreamReader(context.Request.Body);
            //        var requestBody = await streamReader.ReadToEndAsync();

            //        var requestObject = JsonSerializer.Deserialize<Dictionary<string, string>>(requestBody);
            //        var companyId = int.Parse(requestObject!["CompanyId"]);


            //        List<List<KeyValuePair<string, string>>> companiesList = EmailHelper.GetCustomerSettings(companyId);
            //        var response = new { GetCustomerSettings = companiesList };
            //        var json = JsonSerializer.Serialize(response);
            //        context.Response.ContentType = "application/json";

            //        await context.Response.WriteAsync(json);
            //        Log.Debug("Transfer to Client Successful - Management API - /AutoTaskCompanies/CompanyInfo - " +
            //            "Return Company information from the AutoTask Company ID");

            //    }
            //    catch (Exception ex)
            //    {
            //        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            //        context.Response.ContentType = "application/json";
            //        var errorMessage = new { Message = $"Invalid request body! Must use a string for CompanyId:  {ex}" };
            //        var jsonResponse = JsonSerializer.Serialize(errorMessage);
            //        await context.Response.WriteAsync(jsonResponse);
            //        Log.Debug($"Transfer to Client Errored - Management API - /AutoTaskCompanies/CompanyInfo - " +
            //            $"{ex}");
            //    }
            //});
            #endregion

            #region Reload all active Autotask Companies from Autotask API into the Companies Dictionary
            ////Reloads all active Autotask Companies from the AutoTask API into Companies.companies Dictionary
            ////Compares the results to what is in SQL and updates SQL with any missing companies. ie. Sync the active companies to SQL. This just adds any new companies to SQL from AT.
            ////Finally reports back when complete with the status in the response
            //endpoints.MapGet("/AutoTaskCompanies/Sync", async context =>
            //{
            //    try
            //    {
            //        string maintApiMessage = "In Bound Maint. API to: Sync Companies from AT API to SQL DB";
            //        Log.Debug(maintApiMessage);

            //        //Get AT Companies from AutoTask API and fill Companies.companies dictionary
            //        AutotaskAPIGet.GetAutoTaskCompanies();

            //        //Compares the SQL DB with what was loaded into memory and if anything is missing in SQL it gets added
            //        AppConfig.UpdateDataBaseWithMissingCompanies();

            //        var count = Companies.GetCompanyCountFromMemory();

            //        var response = $"Company Sync task complete: {count} companies stored in Dictionary";
            //        var json = JsonSerializer.Serialize(response);
            //        context.Response.ContentType = "application/json";
            //        await context.Response.WriteAsync(json);
            //        Log.Debug("API AT Company Sync from AT API to SQL DB Successful - Management API - /AutoTaskCompanies/Sync");

            //    }
            //    catch (Exception ex)
            //    {
            //        var response = $"Error ManagementAPI /AutoTaskCompanies/Sync: {ex}";
            //        Log.Warning(response);

            //        var json = JsonSerializer.Serialize(response);
            //        context.Response.ContentType = "application/json";
            //        await context.Response.WriteAsync(json);
            //        Log.Debug($"API AT Company Syncfrom AT API to SQL DB Successful Failed  - Management API - /AutoTaskCompanies/Sync {ex}");
            //    }

            //});
            #endregion

            #region Get All Active AT companies and return in List of Arrays
            //endpoints.MapGet("/AutoTaskCompanies/GetCompanies", async context =>
            //{
            //    //Get All Active AT companies and return in List of Arrays
            //    try
            //    {
            //        string maintApiMessage = "In Bound Maint. API to: Get All Active Companies from Memory";
            //        Log.Debug("\n");
            //        Log.Debug(maintApiMessage);

            //        List<List<KeyValuePair<string, string>>> companiesList = EmailHelper.GetCustomerSettings();
            //        var response = new { GetCustomerSettings = companiesList };
            //        var json = JsonSerializer.Serialize(response);
            //        context.Response.ContentType = "application/json";
            //        await context.Response.WriteAsync(json);

            //        Log.Debug("\nTransfer to Client Successful - Management API - /AutoTaskCompanies/GetCompanies - " +
            //            "Return the current list of companies contained in the Companies.companies dictionary\n");
            //    }
            //    catch (Exception ex)
            //    {
            //        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            //        context.Response.ContentType = "application/json";
            //        var errorMessage = new { Message = $"Request Failed:   {ex}" };
            //        var jsonResponse = JsonSerializer.Serialize(errorMessage);
            //        await context.Response.WriteAsync(jsonResponse);
            //        Log.Debug($"Transfer to Client Failed - Management API - /AutoTaskCompanies/GetCompanies - {ex}");
            //    }
            //});
            #endregion

            #region Update DataBase Companies that have been modified
            //endpoints.MapPost("/AutoTaskCompanies/UpdateCompanies", async context =>
            //{
            //    try
            //    {
            //        Log.Debug("In Bound Maint. API to: Update Companies that have been modified");

            //        // Read the incoming stream as JSON
            //        using (StreamReader reader = new StreamReader(context.Request.Body))
            //        {
            //            string json = await reader.ReadToEndAsync();
            //            // Deserialize the JSON into a list of Company objects
            //            List<Company> companiesList = JsonSerializer.Deserialize<List<Company>>(json);

            //            using (var dbContext = new ATTMSContext()) // Using ATTMSContext
            //            {
            //                foreach (var company in companiesList)
            //                {
            //                    // Find the existing CustomerSetting record
            //                    var customerSetting = await dbContext.CustomerSettings
            //                        .FirstOrDefaultAsync(c => c.AutotaskId == company.AutotaskId);

            //                    if (customerSetting != null)
            //                    {
            //                        // Update the properties
            //                        customerSetting.SupportEmail = company.SupportEmail;
            //                        customerSetting.EnableEmail = bool.Parse(company.EnableEmail);
            //                        customerSetting.AutoAssign = bool.Parse(company.AutoAssign);

            //                        // Save the changes
            //                        await dbContext.SaveChangesAsync();
            //                    }
            //                }
            //            }

            //            await context.Response.WriteAsync(JsonSerializer.Serialize(new { Message = "Companies updated successfully" }));
            //        }

            //        // Additional logic like reloading support distributions...
            //        AppConfig.LoadSupportDistros();

            //        Log.Debug("Companies updated in the SQL database");
            //    }
            //    catch (Exception ex)
            //    {
            //        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            //        context.Response.ContentType = "application/json";
            //        await context.Response.WriteAsync(JsonSerializer.Serialize(new { Message = $"Request Failed: {ex.Message}" }));
            //        Log.Debug($"Transfer to Client Failed - {ex}");
            //    }
            //});
            #endregion

            #region Return Subject Exclusion Keywords from in memory list
            //endpoints.MapGet("/SubjectExclusionKeyWord/Get", async context =>
            //{

            //    try
            //    {
            //        string maintApiMessage = "In Bound Maint. API to: Return Subject Exclusion Keywords from in memory list";
            //        Log.Debug(maintApiMessage);

            //        var subjectExclusionKeyWordsList = ManagementApiHelper.GetSubjectExclusionKeyWordsFromList();
            //        var response = new { GetSubjectExclusionKeyWordsFromList = subjectExclusionKeyWordsList };
            //        var json = JsonSerializer.Serialize(response);
            //        context.Response.ContentType = "application/json";
            //        await context.Response.WriteAsync(json);

            //        Log.Debug("Transfer to Client Successful - Management API - /SubjectExclusionKeyWords/Get - " +
            //            "Return the current list of Subject Exclusion Key Words from memory");

            //    }
            //    catch (Exception ex)
            //    {
            //        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            //        context.Response.ContentType = "application/json";
            //        var errorMessage = new { Message = $"Request Failed:   {ex}" };
            //        var jsonResponse = JsonSerializer.Serialize(errorMessage);
            //        await context.Response.WriteAsync(jsonResponse);

            //        Log.Debug($"Transfer to Client Failed - Management API - /SubjectExclusionKeyWords/Get - {ex}");
            //    }

            //});
            #endregion

            #region Update Subject Exclusion Keywords from Management Console in SQL and reload in memory list
            //endpoints.MapPost("/SubjectExclusionKeyWord/Update", async context =>
            //{
            //    try
            //    {
            //        // Receive new list of strings from remote API client
            //        List<string> updatedKeywords = await context.Request.ReadFromJsonAsync<List<string>>();
            //        if (updatedKeywords != null)
            //        {
            //            // Clear existing list of Subject Exclusion Keyword strings
            //            AppConfig.subjectExclusionKeyWordList.Clear();

            //            // Copy new list of strings to existing Subject Exclusion Keywords List
            //            AppConfig.subjectExclusionKeyWordList.AddRange(updatedKeywords);

            //            using (var dbContext = new ATTMSContext())
            //            {
            //                // Clear the existing data in the database table
            //                dbContext.SubjectExclusionKeywords.RemoveRange(dbContext.SubjectExclusionKeywords);

            //                // Insert the modified data into the database table
            //                foreach (var item in updatedKeywords)
            //                {
            //                    dbContext.SubjectExclusionKeywords.Add(new SubjectExclusionKeyword { SubjectKeyWord = item });
            //                }

            //                await dbContext.SaveChangesAsync();
            //            }

            //            await context.Response.WriteAsync("OK");
            //            Log.Debug("SQL Subject Exclusion Keywords Updated");

            //            //Reload the 'subjectExclusionKeyWordList' from SQL
            //            AppConfig.LoadSubjectExclusionKeyWordsFromSQL();
            //            Log.Debug("'subjectExclusionKeyWordList' in Memory has been reloaded");
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            //        context.Response.ContentType = "application/json";
            //        var errorMessage = new { Message = $"Request Failed:   {ex}" };
            //        var jsonResponse = JsonSerializer.Serialize(errorMessage);
            //        await context.Response.WriteAsync(jsonResponse);

            //        Log.Debug($"Transfer to Client Failed - Management API - /SubjectExclusionKeyWords/Get - {ex}");
            //    }
            //});
            #endregion

            #region Return Sender Exclusions from in memory list - AppConfig.senderExclusionsList
            //endpoints.MapGet("/SenderExclusions/Get", async context =>
            //{

            //    try
            //    {
            //        string maintApiMessage = "In Bound Maint. API to: Return Ssender Exclusions from in memory list - AppConfig.senderExclusionsList";
            //        Log.Debug(maintApiMessage);

            //        var senderExclusionsList = ManagementApiHelper.GetSenderExclusionsFromList();
            //        var response = new { GetSenderExclusionsFromList = senderExclusionsList };
            //        var json = JsonSerializer.Serialize(response);
            //        context.Response.ContentType = "application/json";
            //        await context.Response.WriteAsync(json);

            //        Log.Debug("Transfer to Client Successful - Management API - /SenderExclusionsList/Get - " +
            //            "Return the current list of Sender Exclusions from memory");

            //    }
            //    catch (Exception ex)
            //    {
            //        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            //        context.Response.ContentType = "application/json";
            //        var errorMessage = new { Message = $"Request Failed:   {ex}" };
            //        var jsonResponse = JsonSerializer.Serialize(errorMessage);
            //        await context.Response.WriteAsync(jsonResponse);

            //        Log.Debug($"Transfer to Client Failed - Management API - /SenderExclusionsList/Get - {ex}");
            //    }

            //});
            #endregion



            //Initiate service Cancellation Token
            endpoints.MapGet("/ATTMS/StopWorker", async context =>
            {
                try
                {
                    Log.Information("API call to stop service received - Management API - /ATTMS/StopTokenIssued from AT API");

                    // Access the service provider
                    var serviceProvider = context.RequestServices;

                    // Resolve an instance of IWorkerService
                    var workerService = serviceProvider.GetService<IWorkerService>();

                    // Ensure the service is resolved
                    if (workerService == null)
                    {
                        Log.Error("Worker service not found.");
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Worker service not found.");
                        return;
                    }

                    // Call StopService to stop the worker
                    workerService.StopService();

                    await context.Response.WriteAsync("Worker service stop initiated.");
                }
                catch (Exception ex)
                {
                    Log.Debug($"API Call to Stop Token Failed - Management API - /ATTMS/StopTokenIssued from AT API - {ex}");
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Failed to stop the worker service.");
                }
            });



        }
    }
}

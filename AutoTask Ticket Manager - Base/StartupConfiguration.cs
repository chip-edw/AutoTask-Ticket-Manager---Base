using AutoTaskTicketManager_Base.AutoTaskAPI;
using AutoTaskTicketManager_Base.Models;
using Microsoft.Data.Sqlite;
using Serilog;
using System.Numerics;
using System.Runtime.InteropServices;

namespace AutoTaskTicketManager_Base
{
    public static class StartupConfiguration
    {

        #region "Dictionaries and Lists"
        //Dictionary holds all protected app configs loaded from SQLite DB or in future azure vault.
        public static Dictionary<string, string> protectedSettings = new Dictionary<string, string>();

        //Dictionary holds config values specific to MS Graph API - This dictionary loaded from AppSettings
        public static Dictionary<string, string> graphConfigs = new Dictionary<string, string>();

        //Dictionary holds all the AutoTask Company IDs and companyNames loaded from the DB.
        //Is used to compare against the company list downloaded from Auto Task.
        public static Dictionary<string, string> companies = new Dictionary<string, string>();

        //Dictionary Holds all the AUtotask IDs for the companies in the local SQLite table "CustomerSettings"
        //which have the AutoAsign bit set to "1". Will be used to determine if we invoke AutoAssign logic.
        public static Dictionary<BigInteger, bool> autoAssignCompanies = new Dictionary<BigInteger, bool>();

        //Dictionary holds Autotask Company IDs that have the Autoassign flag set
        //Will be used to determine when the ticket creation autoassignment logic is invoked
        public static Dictionary<Int64, object[]> autoAssignMembers = new Dictionary<Int64, object[]>();

        //Dictionary holds Autotask Resource IDs that should be assigned any ticket they raise through email
        public static Dictionary<string, object[]> autoAssignSenders = new Dictionary<string, object[]>();

        //Dictionary holds all the Active Support Distros. It is loaded from DB. Used when a e-mail is parsed to determine if we should create an Autotask Ticket
        public static Dictionary<string, Int64> supportDistros = new Dictionary<string, Int64>(StringComparer.OrdinalIgnoreCase);

        //List holds all the Email Subject Exclusion Key Words loaded from the Database at Startup
        public static List<string> subjectExclusionKeyWordList = new List<string>();

        //List holds all the Email Sender Exclusion e-mail addresses loaded from the Database at Startup.
        //This prevents un authorized e-mail senders from raising tickets
        public static List<string> senderExclusionsList = new List<string>();

        //Dictionary holds all the AT Ticket numbers and Ticket Titles not in a completed state.
        //Used to compare to the subjects of inbound e-mails to prevent creation of duplicate tickets
        public static Dictionary<string, string> companiesTicketsNotCompleted = new Dictionary<string, string>();

        //Dictionary holds Autotask Resources that are Active
        //ResourceID as Key (firstName, email) as Value in an Object.
        public static Dictionary<Int64, object[]> autoTaskResources = new Dictionary<Int64, object[]>();


        #endregion

        #region " Determine OS "
        /// <summary>
        /// Determine which OS we are operating in. Needed since this is a cross platform App
        /// </summary>
        /// <returns>A string containing one of "Windows", "Linux", "macOS", or "Unknown" </returns>
        public static string DetermineOS()
        {
            string os = string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                os = "Windows";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                os = "Linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                os = "macOS";
            }
            else
            {
                Console.WriteLine("Operating system not recognized");
                os = "Unknown";
            }

            return os;

        }
        #endregion

        #region "Startup and Configuration Methods"
        public static string GetProtectedSetting(string Tkey)
        {
            return protectedSettings[Tkey];
        }

        public static void SetConfig(string Tkey, string Tvalue)
        {
            protectedSettings.Add(Tkey, Tvalue);
        }

        public static object[] GetAutoAssignMembers(long Tkey)
        {
            return autoAssignMembers[Tkey];
        }

        public static object[] GetAutoAssignSenders(string Tkey)
        {
            return autoAssignSenders[Tkey];
        }

        public static bool LoadProtectedSettings(ApplicationDbContext dbContext)
        {
            Log.Debug("Loading Protected Settings Method Fired from StartupConfiguration");
            Log.Debug("Clearing Dictionary");

            protectedSettings.Clear();

            try
            {
                Log.Debug("Populating dictionary with Protected Settings Key, Value pairs");

                foreach (var setting in dbContext.ConfigStore)
                {
                    protectedSettings[setting.ValueName] = setting.Value;
                }

                Log.Debug($"Protected Settings loaded. Count: {protectedSettings.Count}");
                return true;
            }
            catch (SqliteException ex)
            {
                Log.Error("{0} Unable to read from database: {1}", nameof(LoadProtectedSettings), ex);
                return false;
            }
        }


        public static void LoadMsGraphConfig()
        {
            string[] items = { "ClientID", "ClientSecret", "Tenant" };
            foreach (var item in items)
            {
                try
                {
                    graphConfigs[item] = protectedSettings[item];
                }
                catch (Exception ex)
                {
                    Log.Error($"StartupConfiguration Error loading graphConfigs dictionary  {ex}");
                }

            }
            ;
            Log.Debug("MS Graph Configs Loaded\n");
        }

        internal static string GetMsGraphConfig(string Tkey)
        {
            return graphConfigs[Tkey];
        }


        /// <summary>
        /// Loads the Active Support e-mail distros into the dictionary named "supportDistros"
        /// </summary>
        /// <returns></returns>
        public static bool LoadSupportDistros(ApplicationDbContext dbContext)
        {
            //###############     Load active Support Distros from SQL DB   ###############
            try
            {
                var filteredSettings = dbContext.CustomerSettings
                    .Where(setting => setting.EnableEmail == true &&
                                      !string.IsNullOrEmpty(setting.SupportEmail) &&
                                      setting.SupportEmail.Length > 5);

                foreach (var setting in filteredSettings)
                {
                    supportDistros[setting.SupportEmail] = setting.AutotaskId;
                }

                Log.Debug($"Support Distros Loaded. Count: {supportDistros.Count()}\n");
                return true;
            }
            catch (SqliteException ex)
            {
                Log.Error("{0} Unable to read from database: {1}", nameof(LoadSupportDistros), ex);
                return false;
            }
        }


        public static void LoadAutoAssignCompanies(ApplicationDbContext dbContext)
        {
            try
            {
                // Flush in-memory Company list before repopulating
                autoAssignCompanies.Clear();

                var filteredSettings = dbContext.CustomerSettings
                    .Where(setting => setting.AutoAssign == true);

                foreach (var setting in filteredSettings)
                {
                    autoAssignCompanies[setting.AutotaskId] = setting.AutoAssign;
                }

                Log.Debug($"Auto Assign Companies loaded. Count: {autoAssignCompanies.Count}\n");
            }
            catch (SqliteException ex)
            {
                Log.Error("{0} Unable to read from database: {1}", nameof(LoadAutoAssignCompanies), ex);
            }
        }

        public static string GetConfig(string Tkey)
        {
            if (protectedSettings.TryGetValue(Tkey, out string value))
            {
                return value;
            }

            string message = $"[StartupConfiguration] MISSING CONFIG KEY: '{Tkey}' — this is required and the app will now shut down.";
            Log.Fatal(message);

            Environment.FailFast(message);  // Immediately terminates the process the application and any threads such as Schedulers

            return null!;
        }
        public static void LoadAutoAssignSenders(ApplicationDbContext dbContext)
        {
            try
            {
                var senderAssignments = dbContext.SenderAssignments.ToList();

                if (autoAssignSenders.Count > 0)
                {
                    autoAssignSenders.Clear();
                }

                foreach (var assignment in senderAssignments)
                {
                    string resourceId = assignment.AT_Resource_Id;
                    string resourceName = assignment.Resource_Name;
                    string resourceEmail = assignment.Resource_Email;
                    string resourceRole = assignment.Resource_Role;
                    bool resourceActive = assignment.Resource_Active;

                    autoAssignSenders.Add(resourceId, new object[] {
                resourceName, resourceEmail, resourceRole, resourceActive
            });
                }

                Log.Debug($"Sender Assign Members loaded. Count: {autoAssignSenders.Count}\n");
            }
            catch (Exception ex)
            {
                Log.Error($"AppConfig.cs LoadAutoAssignSenders() encountered an error: {ex}");
            }
        }
        public static bool LoadSubjectExclusionKeyWordsFromSQL(ApplicationDbContext dbContext)
        {
            try
            {
                // Reload List from DB
                var subjectExclusionKeyWords = dbContext.SubjectExclusionKeywords.ToList();
                if (subjectExclusionKeyWordList.Count > 0)
                {
                    // Clear the list if not empty
                    subjectExclusionKeyWordList.Clear();
                }

                foreach (var exclusion in subjectExclusionKeyWords)
                {
                    subjectExclusionKeyWordList.Add(exclusion.SubjectKeyWord);
                }


                Log.Debug($"Subject Exclusion KeyWords Loaded. Count: {StartupConfiguration.subjectExclusionKeyWordList.Count}\n");

                return true;
            }
            catch (Exception ex) // Catching a more general exception
            {
                Log.Error("{0} Unable to read from database: {1}", nameof(LoadSubjectExclusionKeyWordsFromSQL), ex);
                return false;
            }
        }
        public static bool LoadSenderExclusionListFromSQL(ApplicationDbContext dbContext)
        {
            try
            {
                var senderExclusions = dbContext.SenderExclusions.ToList();

                if (senderExclusionsList.Count > 0)
                {
                    // Clear the list if not empty
                    senderExclusionsList.Clear();
                }

                foreach (var address in senderExclusions)
                {
                    senderExclusionsList.Add(address.SenderAddress);
                }

                Log.Debug($"Sender Exclusions Loaded. Count: {senderExclusionsList.Count}\n");

                return true;
            }
            catch (Exception ex) // Catching a more general exception
            {
                Log.Error("{0} Unable to read from database: {1}", nameof(LoadSenderExclusionListFromSQL), ex);
                return false;
            }
        }

        public static void UpdateDataBaseWithMissingCompanies(ApplicationDbContext dbContext)
        {
            Log.Information("Starting update of CustomerSettings with missing AutoTask companies...");

            // Retrieve existing company IDs from the SQL table
            var existingCompanyIds = dbContext.CustomerSettings
                                            .Select(cs => cs.AutotaskId)
                                            .ToHashSet();

            int addedCompaniesCount = 0; // Counter for the number of added companies

            // Iterate through Companies.companies
            foreach (var entry in Companies.companies)
            {
                int companyId = (int)entry.Key; // Assuming the key is the company ID
                object[] companyData = entry.Value;

                // Special handling: Skip Owner Company (ID 0)
                if (companyId == 0)
                {
                    Log.Information("Skipping Owner Company (ID 0) during database update process.");
                    continue;
                }

                // Check if the company ID does not exist in the database
                if (!existingCompanyIds.Contains(companyId))
                {
                    // Assuming companyData[0] is AccountName, companyData[1] is Enabled, etc.
                    var newCustomerSetting = new CustomerSettings
                    {
                        AutotaskId = companyId,
                        AccountName = companyData.Length > 0 ? (string)companyData[0] : null,
                        Enabled = companyData.Length > 1 ? Convert.ToBoolean(companyData[1]) : false,
                        // Set the following fields to default values
                        AutoAssign = false,
                        EnableEmail = false,
                        SupportEmail = ""
                    };

                    dbContext.CustomerSettings.Add(newCustomerSetting);
                    addedCompaniesCount++;

                    // Log the addition of a new company
                    Log.Information("Added new company to database: {AutotaskId}, {AccountName}", newCustomerSetting.AutotaskId, newCustomerSetting.AccountName);
                }
            }

            // Save changes to the database
            if (addedCompaniesCount > 0)
            {
                dbContext.SaveChanges();
                Log.Information("Saved {Count} new companies to database.", addedCompaniesCount);
            }
            else
            {
                Log.Information("No new companies needed to be added.");
            }

        }


        #endregion
    }
}
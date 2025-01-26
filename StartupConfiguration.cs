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


        public static bool LoadProtectedSettings()
        {
            Log.Debug("Loading Protected Settings Method Fired from StartupConfiguration");
            Log.Debug("Clearing Dictionary");
            protectedSettings.Clear();
            try
            {
                Log.Debug("Populating dictionary with Protected Settings Key, Value pairs");
                using (var context = new ApplicationDbContext())
                {
                    foreach (var setting in context.ConfigStore)
                    {
                        protectedSettings.Add(setting.ValueName, setting.Value);
                    }
                }

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

            };
            Log.Debug("MS Graph Configs Loaded\n");
        }

        /// <summary>
        /// Loads the Active Support e-mail distros into the dictionary named "supportDistros"
        /// </summary>
        /// <returns></returns>
        public static bool LoadSupportDistros()
        {
            //###############     Load active Support Distros from SQL DB   ###############
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var filteredSettings = context.CustomerSettings
                        .Where(setting => setting.EnableEmail == true && !string.IsNullOrEmpty(setting.SupportEmail) && setting.SupportEmail.Length > 5);

                    foreach (var setting in filteredSettings)
                    {
                        supportDistros[setting.SupportEmail] = setting.AutotaskId;
                    }
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

        public static void LoadAutoAssignCompanies()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    // Flush In Memory Company list so we can repopulate
                    autoAssignCompanies.Clear();

                    var filteredSettings = context.CustomerSettings
                        .Where(setting => setting.AutoAssign == true);

                    foreach (var setting in filteredSettings)
                    {
                        autoAssignCompanies[setting.AutotaskId] = setting.AutoAssign;
                    }
                }

                Log.Debug($"Auto Assign Companies loaded. Count: {autoAssignCompanies.Count()}\n");

            }
            catch (SqliteException ex)
            {
                Log.Error("{0} Unable to read from database: {1}", nameof(LoadSupportDistros), ex);

            }

        }

        public static string GetConfig(string Tkey)
        {
            return protectedSettings[Tkey];
        }



        #endregion
    }
}
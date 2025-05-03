using AutoTaskTicketManager_Base;

namespace AutoTaskTicketManager.Services
{
    /// <summary>
    /// ExclusionService provides fast access to exclusion keyword lists (subject/sender) 
    /// via static methods. These lists are loaded into memory during startup and used 
    /// during runtime by the Worker and EmailManager components.
    ///
    /// NOTE: This is a TEMPORARY design. This service will be refactored into a DI-based
    /// implementation once EmailManager and related components are decoupled from static usage.
    ///
    /// See: ATTMS Product Roadmap [Sprint Refactor - ExclusionService -> DI]
    /// </summary>
    public class ExclusionService
    {
        /// <summary>
        /// Get the list of sender email addresses to exclude from ticket creation.
        /// Uses memory loaded at startup.
        /// </summary>
        public static List<string> GetSenderExclusions()
        {
            return StartupConfiguration.senderExclusionsList ?? new List<string>();
        }

        /// <summary>
        /// Get the list of Subject Keywords to exclude from ticket creation.
        /// Uses memory loaded at startup.
        /// </summary>
        public static List<string> GetSubjectExclusions()
        {
            return StartupConfiguration.subjectExclusionKeyWordList ?? new List<string>();
        }
    }
}

using AutoTaskTicketManager_Base;

namespace AutoTaskTicketManager.Services
{
    public class ExclusionService
    {
        public static List<string> GetSenderExclusions()
        {
            return StartupConfiguration.senderExclusionsList ?? new List<string>();
        }

        public static List<string> GetSubjectExclusions()
        {
            return StartupConfiguration.subjectExclusionKeyWordList ?? new List<string>();
        }
    }
}

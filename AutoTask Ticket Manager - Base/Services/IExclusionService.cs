namespace AutoTaskTicketManager_Base.Services
{
    public interface IExclusionService
    {
        List<string> GetSenderExclusions();
        List<string> GetSubjectExclusions();
    }
}

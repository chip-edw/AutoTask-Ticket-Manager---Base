namespace AutoTaskTicketManager_Base.AutoTaskAPI
{
    public static class QueueCache
    {
        public static Dictionary<string, string> Queues = new();

        public static void Clear() => Queues.Clear();

        public static string GetQueueName(string? id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "Unassigned";
            return Queues.TryGetValue(id, out var name)
                ? name
                : $"QueueID: {id}";
        }
    }

}

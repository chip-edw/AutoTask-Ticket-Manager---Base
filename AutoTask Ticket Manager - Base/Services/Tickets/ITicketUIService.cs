using AutoTaskTicketManager_Base.Dtos.Tickets;

namespace AutoTaskTicketManager_Base.Services.Tickets
{
    public interface ITicketUIService
    {
        /// <summary>
        /// Retrieves a paginated list of open tickets (excluding status = 5).
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of tickets per page</param>
        /// <returns>List of TicketUI DTOs</returns>
        Task<List<TicketUI>> GetOpenTicketsAsync(int page, int pageSize);

        /// <summary>
        /// Retrieves a single ticket by ticket number.
        /// </summary>
        /// <param name="ticketNumber">The user-facing ticket number</param>
        /// <returns>TicketUI DTO or null if not found</returns>
        Task<TicketUI?> GetTicketByNumberAsync(string ticketNumber);
    }
}

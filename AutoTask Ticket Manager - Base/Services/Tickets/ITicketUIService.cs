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

        /// <summary>
        /// Creates a new ticket in AutoTask with the provided details.
        /// </summary>
        /// <param name="newTicket">The ticket details to create</param>
        /// <returns>The created TicketUI DTO, or null if creation failed</returns>
        Task<TicketUI?> CreateTicketAsync(TicketCreateDto newTicket);

        /// <summary>
        /// Updates an existing ticket in AutoTask with the specified changes.
        /// </summary>
        /// <param name="ticketId">The internal AutoTask ticket ID</param>
        /// <param name="updatedTicket">The updated ticket field values</param>
        /// <returns>The updated TicketUI DTO, or null if the update failed</returns>
        Task<TicketUI?> UpdateTicketAsync(long ticketId, TicketUpdateDto updatedTicket);
    }
}

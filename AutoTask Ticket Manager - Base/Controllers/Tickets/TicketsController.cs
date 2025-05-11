using AutoTaskTicketManager_Base.Dtos.Tickets;
using AutoTaskTicketManager_Base.Services.Tickets;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AutoTaskTicketManager_Base.Controllers.Tickets
{
    [ApiController]
    [Route("api/v1/tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketUIService _ticketService;

        public TicketsController(ITicketUIService ticketService)
        {
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
        }

        /// <summary>
        /// Gets a paginated list of open tickets.
        /// </summary>
        /// <param name="page">Page number (default = 1)</param>
        /// <param name="pageSize">Number of records per page (default = 25)</param>
        /// <returns>List of open TicketUI objects</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<TicketUI>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<TicketUI>>> GetOpenTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
        {
            Log.Debug("Request received: GET /api/v1/tickets?page={Page}&pageSize={PageSize}", page, pageSize);

            try
            {
                var result = await _ticketService.GetOpenTicketsAsync(page, pageSize);
                Log.Debug("Returning {Count} tickets", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while retrieving open tickets.");
                return StatusCode(500, "An error occurred while retrieving tickets.");
            }
        }

        /// <summary>
        /// Gets a single ticket by its ticket number.
        /// </summary>
        /// <param name="ticketNumber">The ticket number to search for</param>
        /// <returns>The ticket matching the number or 404</returns>
        [HttpGet("{ticketNumber}")]
        [ProducesResponseType(typeof(TicketUI), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<TicketUI>> GetTicketByNumber(string ticketNumber)
        {
            Log.Debug("Request received: GET /api/v1/tickets/{TicketNumber}", ticketNumber);

            try
            {
                var ticket = await _ticketService.GetTicketByNumberAsync(ticketNumber);
                if (ticket is null)
                {
                    Log.Information("Ticket not found: {TicketNumber}", ticketNumber);
                    return NotFound();
                }

                Log.Debug("Ticket found: {TicketNumber}", ticket.TicketNumber);
                return Ok(ticket);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while retrieving ticket #{TicketNumber}", ticketNumber);
                return StatusCode(500, "An error occurred while retrieving the ticket.");
            }
        }
    }
}

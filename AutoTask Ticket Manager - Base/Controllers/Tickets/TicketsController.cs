using AutoTaskTicketManager_Base.AutoTaskAPI;
using AutoTaskTicketManager_Base.Dtos.Tickets;
using AutoTaskTicketManager_Base.Services.Tickets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

        #region GET: /api/v1/tickets

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

        #endregion

        #region GET: /api/v1/tickets/{ticketNumber}
        /// <summary>
        /// Gets a single ticket by its user-facing ticket number.
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

        #endregion

        #region POST: /api/v1/tickets
        /// <summary>
        /// Creates a new ticket in AutoTask with the specified details.
        /// </summary>
        /// <param name="newTicket">The DTO containing ticket creation details</param>
        /// <returns>
        /// A 201 Created response with the created ticket, or an error if creation fails.
        /// On success, the response includes a Location header pointing to the ticket retrieval endpoint.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(TicketUI), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateTicket([FromBody] TicketCreateDto newTicket)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _ticketService.CreateTicketAsync(newTicket);
            if (result == null)
                return StatusCode(500, "Error creating ticket.");

            return CreatedAtAction(nameof(GetTicketByNumber), new { ticketNumber = result.TicketNumber }, result);
        }
        #endregion

        #region PATCH: /api/v1/tickets/{ticketId}
        /// <summary>
        /// Updates an existing ticket in AutoTask with the provided field changes.
        /// </summary>
        /// <param name="ticketId">The AutoTask ticket ID to update</param>
        /// <param name="updatedTicket">The updated ticket fields</param>
        /// <returns>The updated ticket or 404 if not found</returns>
        [HttpPatch("{ticketId:long}")]
        [ProducesResponseType(typeof(TicketUI), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<TicketUI>> UpdateTicket(long ticketId, [FromBody] TicketUpdateDto updatedTicket)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _ticketService.UpdateTicketAsync(ticketId, updatedTicket);
                if (result == null)
                {
                    Log.Information("Ticket update failed or ticket not found: {TicketId}", ticketId);
                    return NotFound($"Ticket with ID {ticketId} not found or could not be updated.");
                }

                Log.Debug("Ticket updated: {TicketId}", ticketId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while updating ticket #{TicketId}", ticketId);
                return StatusCode(500, "An error occurred while updating the ticket.");
            }
        }

        #endregion

        #region GET: /api/v1/tickets/metadata
        [HttpGet("metadata")]
        [ProducesResponseType(typeof(TicketMetadataDto), 200)]
        public IActionResult GetTicketMetadata()
        {
            try
            {
                var metadata = new TicketMetadataDto
                {
                    Statuses = ((IEnumerable<object>)Ticket.GetPickLists("status"))
                        .Select(p => JsonConvert.DeserializeObject<Dictionary<string, string>>(p.ToString()))
                        .Where(dict => dict != null && dict.ContainsKey("value") && dict.ContainsKey("label"))
                        .Select(dict => new PicklistOptionDto
                        {
                            Value = dict["value"],
                            Label = dict["label"]
                        }).ToList(),

                    Queues = ((IEnumerable<object>)Ticket.GetPickLists("queueID"))
                        .Select(p => JsonConvert.DeserializeObject<Dictionary<string, string>>(p.ToString()))
                        .Where(dict => dict != null && dict.ContainsKey("value") && dict.ContainsKey("label"))
                        .Select(dict => new PicklistOptionDto
                        {
                            Value = dict["value"],
                            Label = dict["label"]
                        }).ToList(),

                    Priorities = ((IEnumerable<object>)Ticket.GetPickLists("priority"))
                        .Select(p => JsonConvert.DeserializeObject<Dictionary<string, string>>(p.ToString()))
                        .Where(dict => dict != null && dict.ContainsKey("value") && dict.ContainsKey("label"))
                        .Select(dict => new PicklistOptionDto
                        {
                            Value = dict["value"],
                            Label = dict["label"]
                        }).ToList(),

                    Resources = Companies.companies
                        .Select(kvp => new PicklistOptionDto
                        {
                            Value = kvp.Key.ToString(),
                            Label = kvp.Value.FirstOrDefault()?.ToString() ?? $"ResourceID: {kvp.Key}"
                        }).ToList()
                };

                return Ok(metadata);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load ticket metadata");
                return StatusCode(500, "Failed to load ticket metadata");
            }
        }
        #endregion

    }
}

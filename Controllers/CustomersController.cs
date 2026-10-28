using CustomerOperations.Api.Dtos;
using CustomerOperations.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOperations.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customers;

    public CustomersController(ICustomerService customers)
    {
        _customers = customers;
    }

    /// <summary>Search, filter, sort and page customers. Search "error" simulates a failed request.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll([FromQuery] CustomerQuery query, CancellationToken ct) =>
        Ok(await _customers.GetPageAsync(query, ct));

    /// <summary>Dashboard counts for the whole dataset (independent of table filters) plus the list of cities.</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(CustomerSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CustomerSummaryDto>> GetSummary(CancellationToken ct) =>
        Ok(await _customers.GetSummaryAsync(ct));

    /// <summary>Duplicate-email check used by the form (case-insensitive, trimmed). Pass excludeId when editing.</summary>
    [HttpGet("email-exists")]
    [ProducesResponseType(typeof(EmailExistsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmailExistsResponse>> EmailExists(
        [FromQuery] string? email, [FromQuery] string? excludeId, CancellationToken ct) =>
        Ok(new EmailExistsResponse(await _customers.IsEmailTakenAsync(email, excludeId, ct)));

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetById(string id, CancellationToken ct) =>
        Ok(await _customers.GetByIdAsync(id, ct));

    /// <summary>Create a customer. Email fail@test.com simulates a failed save.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CustomerRequest request, CancellationToken ct)
    {
        var created = await _customers.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerDto>> Update(string id, [FromBody] CustomerRequest request, CancellationToken ct) =>
        Ok(await _customers.UpdateAsync(id, request, ct));

    /// <summary>Activate or deactivate. Customer CUS-1008 simulates a failed status change.</summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> UpdateStatus(string id, [FromBody] UpdateStatusRequest request, CancellationToken ct) =>
        Ok(await _customers.UpdateStatusAsync(id, request.Status, ct));
}

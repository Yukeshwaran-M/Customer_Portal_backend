using CustomerOperations.Api.Dtos;
using CustomerOperations.Api.Models;

namespace CustomerOperations.Api.Services;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetPageAsync(CustomerQuery query, CancellationToken ct);
    Task<CustomerSummaryDto> GetSummaryAsync(CancellationToken ct);
    Task<CustomerDto> GetByIdAsync(string id, CancellationToken ct);
    Task<bool> IsEmailTakenAsync(string? email, string? excludeId, CancellationToken ct);
    Task<CustomerDto> CreateAsync(CustomerRequest request, CancellationToken ct);
    Task<CustomerDto> UpdateAsync(string id, CustomerRequest request, CancellationToken ct);
    Task<CustomerDto> UpdateStatusAsync(string id, CustomerStatus status, CancellationToken ct);
}

using CustomerOperations.Api.Data;
using CustomerOperations.Api.Dtos;
using CustomerOperations.Api.Exceptions;
using CustomerOperations.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CustomerOperations.Api.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;
    private readonly MockBehaviorOptions _mock;

    public CustomerService(AppDbContext db, IOptions<MockBehaviorOptions> mock)
    {
        _db = db;
        _mock = mock.Value;
    }

    public async Task<PagedResult<CustomerDto>> GetPageAsync(CustomerQuery q, CancellationToken ct)
    {
        await SimulateLatencyAsync(ct);

        var term = (q.Search ?? string.Empty).Trim();
        if (_mock.Enabled && term.Equals(_mock.FailSearchTerm, StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status503ServiceUnavailable,
                "The customer service is currently unavailable.", ErrorCodes.ServerError);
        }

        IQueryable<Customer> query = _db.Customers.AsNoTracking();

        // All filters are applied together (AND) so they compose and never clear each other.
        if (!IsAll(q.Status))
        {
            if (!Enum.TryParse<CustomerStatus>(q.Status, ignoreCase: true, out var status) || !Enum.IsDefined(status))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Status must be All, Active or Inactive.", ErrorCodes.Validation);
            }

            query = query.Where(c => c.Status == status);
        }

        if (!IsAll(q.City))
        {
            var city = q.City!.Trim();
            query = query.Where(c => c.City == city);
        }

        if (term.Length > 0)
        {
            var pattern = $"%{EscapeLike(term)}%";
            query = query.Where(c =>
                EF.Functions.ILike(c.FullName, pattern) ||
                EF.Functions.ILike(c.Email, pattern) ||
                EF.Functions.ILike(c.Id, pattern));
        }

        var total = await query.CountAsync(ct);
        var items = await Sort(query, q)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(ct);

        return new PagedResult<CustomerDto>(items.Select(c => c.ToDto()).ToList(), total);
    }

    public async Task<CustomerSummaryDto> GetSummaryAsync(CancellationToken ct)
    {
        await SimulateLatencyAsync(ct);

        var customers = _db.Customers.AsNoTracking();
        var total = await customers.CountAsync(ct);
        var active = await customers.CountAsync(c => c.Status == CustomerStatus.Active, ct);
        var inactive = await customers.CountAsync(c => c.Status == CustomerStatus.Inactive, ct);
        var needsReview = await customers.CountAsync(c => c.ReviewRequired, ct);
        var cities = await customers.Select(c => c.City).Distinct().OrderBy(c => c).ToListAsync(ct);

        return new CustomerSummaryDto(total, active, inactive, needsReview, cities);
    }

    public async Task<CustomerDto> GetByIdAsync(string id, CancellationToken ct)
    {
        await SimulateLatencyAsync(ct);

        var customer = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        return (customer ?? throw NotFound(id)).ToDto();
    }

    public async Task<bool> IsEmailTakenAsync(string? email, string? excludeId, CancellationToken ct)
    {
        if (_mock.Enabled)
        {
            await Task.Delay(150, ct); // small, fixed delay: this is called while the user types
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var normalized = NormalizeEmail(email);
        return await _db.Customers.AsNoTracking()
            .AnyAsync(c => c.EmailNormalized == normalized && (excludeId == null || c.Id != excludeId), ct);
    }

    public async Task<CustomerDto> CreateAsync(CustomerRequest request, CancellationToken ct)
    {
        await SimulateLatencyAsync(ct);

        var data = Clean(request);
        AssertSavable(data.Email);
        var normalized = NormalizeEmail(data.Email);

        if (await _db.Customers.AnyAsync(c => c.EmailNormalized == normalized, ct))
        {
            throw DuplicateEmail();
        }

        var number = await _db.Database
            .SqlQuery<long>($"SELECT nextval('customer_number_seq') AS \"Value\"")
            .SingleAsync(ct);

        var now = DateTime.UtcNow;
        var customer = new Customer
        {
            Id = $"CUS-{number}",
            FullName = data.FullName,
            Email = data.Email,
            EmailNormalized = normalized,
            Phone = data.Phone,
            City = data.City,
            Status = data.Status,
            RiskLevel = data.RiskLevel,
            ReviewRequired = data.ReviewRequired,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Customers.Add(customer);
        await SaveAsync(ct);
        return customer.ToDto();
    }

    public async Task<CustomerDto> UpdateAsync(string id, CustomerRequest request, CancellationToken ct)
    {
        await SimulateLatencyAsync(ct);

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct) ?? throw NotFound(id);
        var data = Clean(request);
        AssertSavable(data.Email);
        var normalized = NormalizeEmail(data.Email);

        if (await _db.Customers.AnyAsync(c => c.Id != id && c.EmailNormalized == normalized, ct))
        {
            throw DuplicateEmail();
        }

        customer.FullName = data.FullName;
        customer.Email = data.Email;
        customer.EmailNormalized = normalized;
        customer.Phone = data.Phone;
        customer.City = data.City;
        customer.Status = data.Status;
        customer.RiskLevel = data.RiskLevel;
        customer.ReviewRequired = data.ReviewRequired;
        customer.UpdatedAt = DateTime.UtcNow;

        await SaveAsync(ct);
        return customer.ToDto();
    }

    public async Task<CustomerDto> UpdateStatusAsync(string id, CustomerStatus status, CancellationToken ct)
    {
        await SimulateLatencyAsync(ct);

        if (!Enum.IsDefined(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Status must be Active or Inactive.", ErrorCodes.Validation);
        }

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct) ?? throw NotFound(id);

        if (_mock.Enabled && id.Equals(_mock.FailStatusId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status500InternalServerError,
                "Unable to change the customer status. Please try again.", ErrorCodes.ServerError);
        }

        customer.Status = status;
        customer.UpdatedAt = DateTime.UtcNow;
        await SaveAsync(ct);
        return customer.ToDto();
    }

    // ---------------------------------------------------------------- helpers

    private async Task SimulateLatencyAsync(CancellationToken ct)
    {
        if (!_mock.Enabled)
        {
            return;
        }

        var min = Math.Max(0, _mock.MinDelayMs);
        var max = Math.Max(min, _mock.MaxDelayMs);
        await Task.Delay(Random.Shared.Next(min, max + 1), ct);
    }

    private void AssertSavable(string email)
    {
        if (_mock.Enabled && NormalizeEmail(email) == NormalizeEmail(_mock.FailEmail))
        {
            throw new ApiException(StatusCodes.Status500InternalServerError,
                "Unable to save the customer. Please try again.", ErrorCodes.ServerError);
        }
    }

    /// <summary>The unique index is the last line of defence against two simultaneous saves of the same email.</summary>
    private async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw DuplicateEmail();
        }
    }

    private static CustomerRequest Clean(CustomerRequest request)
    {
        var cleaned = new CustomerRequest
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            City = request.City.Trim(),
            Status = request.Status,
            RiskLevel = request.RiskLevel,
            ReviewRequired = request.ReviewRequired,
        };

        // Attribute validation ran on the untrimmed values, so re-check what is actually stored.
        if (cleaned.FullName.Length is < 3 or > 80)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Full name must be between 3 and 80 characters.", ErrorCodes.Validation);
        }

        if (cleaned.City.Length == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "City is required.", ErrorCodes.Validation);
        }

        if (!Enum.IsDefined(cleaned.Status) || !Enum.IsDefined(cleaned.RiskLevel))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Status or risk level is not valid.", ErrorCodes.Validation);
        }

        return cleaned;
    }

    private static IOrderedQueryable<Customer> Sort(IQueryable<Customer> query, CustomerQuery q)
    {
        var byName = string.Equals(q.SortBy, "fullName", StringComparison.OrdinalIgnoreCase);
        var byDate = string.IsNullOrWhiteSpace(q.SortBy) || string.Equals(q.SortBy, "updatedAt", StringComparison.OrdinalIgnoreCase);
        if (!byName && !byDate)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Sort field must be fullName or updatedAt.", ErrorCodes.Validation);
        }

        var descending = string.IsNullOrWhiteSpace(q.SortDirection)
            ? !byName // default: newest first for dates, A-Z for names
            : string.Equals(q.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        // ThenBy(Id) makes paging stable when the primary values are equal.
        return (byName, descending) switch
        {
            (true, false) => query.OrderBy(c => c.FullName).ThenBy(c => c.Id),
            (true, true) => query.OrderByDescending(c => c.FullName).ThenBy(c => c.Id),
            (false, false) => query.OrderBy(c => c.UpdatedAt).ThenBy(c => c.Id),
            _ => query.OrderByDescending(c => c.UpdatedAt).ThenBy(c => c.Id),
        };
    }

    private static bool IsAll(string? value) =>
        string.IsNullOrWhiteSpace(value) || value.Trim().Equals("All", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    private static ApiException NotFound(string id) =>
        new(StatusCodes.Status404NotFound, $"Customer {id} was not found.", ErrorCodes.NotFound);

    private static ApiException DuplicateEmail() =>
        new(StatusCodes.Status409Conflict, "A customer with this email already exists.", ErrorCodes.DuplicateEmail);
}

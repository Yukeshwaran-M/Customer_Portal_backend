using System.ComponentModel.DataAnnotations;
using CustomerOperations.Api.Models;

namespace CustomerOperations.Api.Dtos;

public record CustomerDto(
    string Id,
    string FullName,
    string Email,
    string Phone,
    string City,
    CustomerStatus Status,
    RiskLevel RiskLevel,
    bool ReviewRequired,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);

public record CustomerSummaryDto(int Total, int Active, int Inactive, int NeedsReview, IReadOnlyList<string> Cities);

public record EmailExistsResponse(bool Taken);

public record ApiErrorResponse(string Message, string? Code = null);

/// <summary>Body of POST /api/customers and PUT /api/customers/{id}.</summary>
public class CustomerRequest
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 80 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [StringLength(254, ErrorMessage = "Email is too long.")]
    [RegularExpression(@"^\s*[^\s@]+@[^\s@]+\.[^\s@]{2,}\s*$", ErrorMessage = "Email is not a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be exactly 10 digits.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required.")]
    [StringLength(60, ErrorMessage = "City cannot be longer than 60 characters.")]
    public string City { get; set; } = string.Empty;

    public CustomerStatus Status { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public bool ReviewRequired { get; set; }
}

public class UpdateStatusRequest
{
    public CustomerStatus Status { get; set; }
}

/// <summary>Query string of GET /api/customers. Values mirror the Angular CustomerFilters model.</summary>
public class CustomerQuery
{
    public string? Search { get; set; }

    /// <summary>All | Active | Inactive</summary>
    public string? Status { get; set; }

    /// <summary>"All" or a city name</summary>
    public string? City { get; set; }

    /// <summary>fullName | updatedAt</summary>
    public string? SortBy { get; set; }

    /// <summary>asc | desc</summary>
    public string? SortDirection { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Page must be 1 or greater.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;
}

public static class CustomerMappings
{
    public static CustomerDto ToDto(this Customer c) =>
        new(c.Id, c.FullName, c.Email, c.Phone, c.City, c.Status, c.RiskLevel, c.ReviewRequired, c.CreatedAt, c.UpdatedAt);
}

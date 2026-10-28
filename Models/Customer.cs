namespace CustomerOperations.Api.Models;

public enum CustomerStatus
{
    Active,
    Inactive
}

public enum RiskLevel
{
    Low,
    Medium,
    High
}

public class Customer
{
    /// <summary>Business id such as CUS-1001.</summary>
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Trimmed + lower-cased email; unique index makes duplicates impossible, even under races.</summary>
    public string EmailNormalized { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public CustomerStatus Status { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public bool ReviewRequired { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

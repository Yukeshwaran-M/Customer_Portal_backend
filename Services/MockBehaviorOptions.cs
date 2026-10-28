namespace CustomerOperations.Api.Services;

/// <summary>
/// Reproducible failures and latency required by the assignment. Configure in appsettings.json ("MockBehavior").
/// Set Enabled to false to switch everything off.
/// </summary>
public class MockBehaviorOptions
{
    public bool Enabled { get; set; } = true;
    public int MinDelayMs { get; set; } = 300;
    public int MaxDelayMs { get; set; } = 1000;

    /// <summary>Searching for this term makes GET /api/customers fail with 503.</summary>
    public string FailSearchTerm { get; set; } = "error";

    /// <summary>Saving a customer with this email fails with 500.</summary>
    public string FailEmail { get; set; } = "fail@test.com";

    /// <summary>Changing the status of this customer fails with 500.</summary>
    public string FailStatusId { get; set; } = "CUS-1008";
}

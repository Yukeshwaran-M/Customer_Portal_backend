using CustomerOperations.Api.Models;

namespace CustomerOperations.Api.Data;

public static class SeedData
{
    public static readonly Customer[] Customers =
    {
        Make("CUS-1001", "Arun Kumar", "arun.kumar@example.com", "9876543210", "Chennai", CustomerStatus.Active, RiskLevel.Low, false, Utc(2026, 1, 12, 9, 30), Utc(2026, 10, 1, 11, 15)),
        Make("CUS-1002", "Priya Sharma", "priya.sharma@example.com", "9876543211", "Coimbatore", CustomerStatus.Active, RiskLevel.Medium, true, Utc(2026, 2, 3, 10, 0), Utc(2026, 10, 5, 14, 20)),
        Make("CUS-1003", "Karthik Raj", "karthik.raj@example.com", "9876543212", "Salem", CustomerStatus.Inactive, RiskLevel.High, true, Utc(2026, 3, 15, 8, 45), Utc(2026, 9, 22, 16, 10)),
        Make("CUS-1004", "Divya Suresh", "divya.suresh@example.com", "9876543213", "Chennai", CustomerStatus.Active, RiskLevel.Low, false, Utc(2026, 4, 8, 12, 0), Utc(2026, 10, 10, 9, 0)),
        Make("CUS-1005", "Vijay Anand", "vijay.anand@example.com", "9876543214", "Madurai", CustomerStatus.Inactive, RiskLevel.Medium, false, Utc(2026, 5, 11, 7, 20), Utc(2026, 8, 30, 13, 40)),
        Make("CUS-1006", "Meena Lakshmi", "meena.lakshmi@example.com", "9876543215", "Coimbatore", CustomerStatus.Active, RiskLevel.High, true, Utc(2026, 6, 2, 11, 30), Utc(2026, 10, 12, 15, 25)),
        Make("CUS-1007", "Sathish Babu", "sathish.babu@example.com", "9876543216", "Salem", CustomerStatus.Active, RiskLevel.Low, false, Utc(2026, 6, 19, 6, 50), Utc(2026, 10, 8, 10, 5)),
        Make("CUS-1008", "Nandhini Raj", "nandhini.raj@example.com", "9876543217", "Madurai", CustomerStatus.Inactive, RiskLevel.High, true, Utc(2026, 7, 7, 14, 15), Utc(2026, 9, 15, 12, 30)),
        Make("CUS-1009", "Praveen Kumar", "praveen.kumar@example.com", "9876543218", "Chennai", CustomerStatus.Active, RiskLevel.Medium, false, Utc(2026, 8, 21, 9, 10), Utc(2026, 10, 14, 8, 35)),
        Make("CUS-1010", "Anitha Devi", "anitha.devi@example.com", "9876543219", "Coimbatore", CustomerStatus.Inactive, RiskLevel.Low, false, Utc(2026, 9, 2, 13, 25), Utc(2026, 10, 16, 17, 0)),
    };

    private static DateTime Utc(int year, int month, int day, int hour, int minute) =>
        new(year, month, day, hour, minute, 0, DateTimeKind.Utc);

    private static Customer Make(
        string id, string name, string email, string phone, string city,
        CustomerStatus status, RiskLevel risk, bool review, DateTime created, DateTime updated) =>
        new()
        {
            Id = id,
            FullName = name,
            Email = email,
            EmailNormalized = email.Trim().ToLowerInvariant(),
            Phone = phone,
            City = city,
            Status = status,
            RiskLevel = risk,
            ReviewRequired = review,
            CreatedAt = created,
            UpdatedAt = updated,
        };
}

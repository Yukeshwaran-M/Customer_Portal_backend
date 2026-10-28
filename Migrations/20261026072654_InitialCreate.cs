using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CustomerOperations.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "customer_number_seq",
                startValue: 1011L);

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    EmailNormalized = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Phone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    City = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ReviewRequired = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "customers",
                columns: new[] { "Id", "City", "CreatedAt", "Email", "EmailNormalized", "FullName", "Phone", "ReviewRequired", "RiskLevel", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { "CUS-1001", "Chennai", new DateTime(2026, 1, 12, 9, 30, 0, 0, DateTimeKind.Utc), "arun.kumar@example.com", "arun.kumar@example.com", "Arun Kumar", "9876543210", false, "Low", "Active", new DateTime(2026, 10, 1, 11, 15, 0, 0, DateTimeKind.Utc) },
                    { "CUS-1002", "Coimbatore", new DateTime(2026, 2, 3, 10, 0, 0, 0, DateTimeKind.Utc), "priya.sharma@example.com", "priya.sharma@example.com", "Priya Sharma", "9876543211", true, "Medium", "Active", new DateTime(2026, 10, 5, 14, 20, 0, 0, DateTimeKind.Utc) },
                    { "CUS-1003", "Salem", new DateTime(2026, 3, 15, 8, 45, 0, 0, DateTimeKind.Utc), "karthik.raj@example.com", "karthik.raj@example.com", "Karthik Raj", "9876543212", true, "High", "Inactive", new DateTime(2026, 9, 22, 16, 10, 0, 0, DateTimeKind.Utc) },
                    { "CUS-1004", "Chennai", new DateTime(2026, 4, 8, 12, 0, 0, 0, DateTimeKind.Utc), "divya.suresh@example.com", "divya.suresh@example.com", "Divya Suresh", "9876543213", false, "Low", "Active", new DateTime(2026, 10, 10, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { "CUS-1005", "Madurai", new DateTime(2026, 5, 11, 7, 20, 0, 0, DateTimeKind.Utc), "vijay.anand@example.com", "vijay.anand@example.com", "Vijay Anand", "9876543214", false, "Medium", "Inactive", new DateTime(2026, 8, 30, 13, 40, 0, 0, DateTimeKind.Utc) },
                    { "CUS-1006", "Coimbatore", new DateTime(2026, 6, 2, 11, 30, 0, 0, DateTimeKind.Utc), "meena.lakshmi@example.com", "meena.lakshmi@example.com", "Meena Lakshmi", "9876543215", true, "High", "Active", new DateTime(2026, 10, 12, 15, 25, 0, 0, DateTimeKind.Utc) },
                    { "CUS-1007", "Salem", new DateTime(2026, 6, 19, 6, 50, 0, 0, DateTimeKind.Utc), "sathish.babu@example.com", "sathish.babu@example.com", "Sathish Babu", "9876543216", false, "Low", "Active", new DateTime(2026, 10, 8, 10, 5, 0, 0, DateTimeKind.Utc) },
                    { "CUS-1008", "Madurai", new DateTime(2026, 7, 7, 14, 15, 0, 0, DateTimeKind.Utc), "nandhini.raj@example.com", "nandhini.raj@example.com", "Nandhini Raj", "9876543217", true, "High", "Inactive", new DateTime(2026, 9, 15, 12, 30, 0, 0, DateTimeKind.Utc) },
                    { "CUS-1009", "Chennai", new DateTime(2026, 8, 21, 9, 10, 0, 0, DateTimeKind.Utc), "praveen.kumar@example.com", "praveen.kumar@example.com", "Praveen Kumar", "9876543218", false, "Medium", "Active", new DateTime(2026, 10, 14, 8, 35, 0, 0, DateTimeKind.Utc) },
                    { "CUS-1010", "Coimbatore", new DateTime(2026, 9, 2, 13, 25, 0, 0, DateTimeKind.Utc), "anitha.devi@example.com", "anitha.devi@example.com", "Anitha Devi", "9876543219", false, "Low", "Inactive", new DateTime(2026, 10, 16, 17, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_customers_City",
                table: "customers",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_customers_EmailNormalized",
                table: "customers",
                column: "EmailNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_Status",
                table: "customers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_customers_UpdatedAt",
                table: "customers",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropSequence(
                name: "customer_number_seq");
        }
    }
}

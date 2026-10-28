using System.Text.Json.Serialization;
using CustomerOperations.Api.Data;
using CustomerOperations.Api.Dtos;
using CustomerOperations.Api.Exceptions;
using CustomerOperations.Api.Middleware;
using CustomerOperations.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

const string CorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // "Active" / "High" instead of 0 / 2, matching the Angular model
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Model-validation failures use the same { message, code } shape as every other error
        options.InvalidModelStateResponseFactory = context =>
        {
            var messages = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct();
            var message = string.Join(" ", messages);
            return new BadRequestObjectResult(new ApiErrorResponse(
                string.IsNullOrWhiteSpace(message) ? "The request is not valid." : message,
                ErrorCodes.Validation));
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.Configure<MockBehaviorOptions>(builder.Configuration.GetSection("MockBehavior"));
builder.Services.AddScoped<ICustomerService, CustomerService>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Applies pending migrations (creates the tables and seeds CUS-1001..CUS-1010)
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseCors(CorsPolicy);
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();

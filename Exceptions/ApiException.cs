namespace CustomerOperations.Api.Exceptions;

public static class ErrorCodes
{
    public const string DuplicateEmail = "DUPLICATE_EMAIL";
    public const string NotFound = "NOT_FOUND";
    public const string ServerError = "SERVER_ERROR";
    public const string Validation = "VALIDATION";
}

/// <summary>A business error that maps directly to an HTTP status code and a stable error code for the UI.</summary>
public class ApiException : Exception
{
    public ApiException(int statusCode, string message, string? code = null) : base(message)
    {
        StatusCode = statusCode;
        Code = code;
    }

    public int StatusCode { get; }
    public string? Code { get; }
}

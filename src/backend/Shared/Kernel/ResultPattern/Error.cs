using System.Text.Json.Serialization;

namespace Shared.Kernel.ResultPattern;

public sealed record Error(string Code, string Description, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
    public static readonly Error NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure
    );

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Unauthorized(string code, string description) =>
        new(code, description, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string description) =>
        new(code, description, ErrorType.Forbidden);

    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);

    public static Error BadRequest(string code, string description) =>
        new(code, description, ErrorType.BadRequest);

    public static Error InternalServerError(string code, string description) =>
        new(code, description, ErrorType.InternalServerError);
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType
{
    None = 1,
    Failure = 2,
    NotFound = 3,
    Validation = 4,
    BadRequest = 5,
    Conflict = 6,
    Unauthorized = 7,
    Forbidden = 8,
    InternalServerError = 9,
}

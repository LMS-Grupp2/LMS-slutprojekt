using System.Net;

namespace LMS.Blazor.Client.Services.ApiProxy;

public sealed class ApiValidationException : HttpRequestException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ApiValidationException(IReadOnlyDictionary<string, string[]> errors, string? detail)
        : base(detail ?? "The request was invalid.", null, HttpStatusCode.BadRequest)
    {
        Errors = errors;
    }
}

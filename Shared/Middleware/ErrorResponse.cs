namespace Shared.Middleware;

public class ErrorResponse
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Errors { get; set; }
}

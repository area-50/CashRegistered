namespace Shared.Response;

public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public T? Data { get; set; }
    public IEnumerable<object>? Errors { get; set; }
}

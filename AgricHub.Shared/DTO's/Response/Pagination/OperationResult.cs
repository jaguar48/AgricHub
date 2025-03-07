namespace AgricHub.Shared.DTO_s.Response;


public record OperationResult<T>
{
    public bool Success { get; }
    public T? Result { get; }
    public string? ErrorMessage { get; }

    private OperationResult(bool success, T? result, string? errorMessage)
    {
        Success = success;
        Result = result;
        ErrorMessage = errorMessage;
    }

    public static OperationResult<T> SuccessResult(T result)

    {
        return new OperationResult<T>(true, result, null);
    }

    public static OperationResult<T> Failure(string errorMessage)
    {
        return new OperationResult<T>(false, default, errorMessage);
    }

    // Common error helpers
    public static OperationResult<T> ValidationFailed(string message) 
        => Failure($"Validation error: {message}");

    public static OperationResult<T> NotFound(string entity) 
        => Failure($"{entity} not found");

    public static OperationResult<T> Forbidden() 
        => Failure("Access denied");

    // Implicit conversion for success cases
    public static implicit operator OperationResult<T>(T result) 
        => SuccessResult(result);
}


// Extended OperationResult for void returns
public record OperationResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }

    private OperationResult(bool success, string? errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public static OperationResult SuccessResult() => new(true, null);
    public static OperationResult Failure(string errorMessage) => new(false, errorMessage);
    public static OperationResult NotFound(string entity) => 
        new(false, $"{entity} not found");
}

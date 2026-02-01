namespace ControleFinanceiro.Application.Services;

public class Result<T>
{
    public bool Success { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }

    private Result(bool success, T? value, string? errorMessage)
    {
        Success = success;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(string errorMessage) => new(false, default, errorMessage);
}

public class Result
{
    public bool Success { get; }
    public string? ErrorMessage { get; }

    private Result(bool success, string? errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public static Result Ok() => new(true, null);
    public static Result Fail(string errorMessage) => new(false, errorMessage);
}

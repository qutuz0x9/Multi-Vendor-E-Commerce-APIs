using MultiVendorECommerce.Shared.Helpers;
using System.Runtime;

namespace MultiVendorECommerce.Shared.Results;

/// <summary>
/// Represents the result of an operation, which can be either a success or a failure with associated errors.
/// </summary>
public class Result
{
  
    protected readonly List<Error> _errors = new();
    public int StatusCode { get; set; } = 200;
    public bool IsSuccess => !_errors.Any();
    public bool IsFailure => _errors.Any();
    public IReadOnlyList<Error> Errors => _errors.AsReadOnly();

    protected Result() { }
    protected Result(int statusCode)
    {
        StatusCode = statusCode;
    }
    protected Result(Error error, int statusCode)
    {
        _errors.Add(error);
        StatusCode = statusCode;
    }
    protected Result(IEnumerable<Error> errors, int statusCode)
    {
        _errors.AddRange(errors);
        StatusCode = statusCode;
    }

    public static Result Success() => new();
    public static Result Success(int statusCode = 200) => new(statusCode);
    public static Result Failure(Error error, int statusCode = 404) => new(error, statusCode);
    public static Result Failure(IEnumerable<Error> errors, int statusCode = 404) => new(errors, statusCode);
}


/// <summary>
/// Represents the result of an operation that can either be a success with a value of type T or a failure with associated errors.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Result<T> : Result
{
    public T? Value { get; }
    private Result(T value)
    {
        Value = value;
    }
    private Result(T value, int statusCode) : base(statusCode)
    {
        Value = value;
    }
    private Result(Error error, int statusCode) : base(error, statusCode) { }
    private Result(IEnumerable<Error> errors, int statusCode) : base(errors, statusCode) { }
    public static Result<T> Success(T value, int statusCode = 200) => new(value, statusCode);
    public static new Result<T> Failure(Error error, int statusCode = 404) => new(error, statusCode);
    public static new Result<T> Failure(IEnumerable<Error> errors, int statusCode = 404) => new(errors, statusCode);
}
using System.Diagnostics.CodeAnalysis;
using Common.Enums;

namespace Common.Result;

public record Result
{
    public bool IsSuccess { get; }
    public ErrorType? Error { get; }

    protected Result(bool isSuccess, ErrorType? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(ErrorType error) => new(false, error);

    public static implicit operator Result(ErrorType error) => Failure(error);
}

public record Result<T1> : Result
{
    public T1? Value { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    public new bool IsSuccess => base.IsSuccess;

    public Result(T1 value) : base(true, null) => Value = value;
    private Result(ErrorType error) : base(false, error) { }

    public static implicit operator Result<T1>(T1 value) => new(value);
    public static implicit operator Result<T1>(ErrorType error) => new(error);
}

public record Result<T1, T2> : Result<T1>
{
    public T2? ErrorValue { get; }

    public Result(T1 value) : base(value) {}
    private Result(ErrorType error) : base(error) { }
    private Result(ErrorType error, T2 errorValue) : base(error) => ErrorValue = errorValue;

    public static implicit operator Result<T1, T2>(T1 value) => new(value);
    public static implicit operator Result<T1, T2>((ErrorType, T2) input) => new(input.Item1, input.Item2);
    public static implicit operator Result<T1, T2>(ErrorType error) => new(error);
}
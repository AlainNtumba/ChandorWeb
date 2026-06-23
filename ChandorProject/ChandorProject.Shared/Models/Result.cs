using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChandorProject.Shared.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    [JsonIgnore]
    public Exception? Exception { get; }
    public T? Value { get; }

    private Result(bool isSuccess, T? value = default, string? errorMessage = null, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static Result<T> Success(T? value = default) => new(true, value);
    public static Result<T> Failure(string errorMessage, Exception? ex = null) =>
        new(false, default, errorMessage, ex);
}

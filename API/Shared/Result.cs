namespace API.Shared;

using System;
using System.Net;

public class Result<TValue>
{
    protected Result(TValue value, ResultCode code, Error error)
    {
        Value = value;
        Code = code;
        Error = error;
    }

    public ResultCode Code { get; }

    public TValue Value { get; }

    public Error Error { get; }

    public bool IsSuccessful => Code == ResultCode.Success && Error is null;

    public bool HasFailed => !IsSuccessful;

    public static Result<TValue> Failure(ResultCode code, Error error)
    {
        return new Result<TValue>(default, code, error);
    }

    public static Result<TValue> Failure((ResultCode Code, Error Error) data)
    {
        return new Result<TValue>(default, data.Code, data.Error);
    }

    public static Result<TValue> Success(TValue value)
    {
        return new Result<TValue>(value, ResultCode.Success, default);
    }

    public static Result<TValue> Success()
    {
        return new Result<TValue>(default, ResultCode.Success, default);
    }

    public Result<TDestination> ToResult<TDestination>()
    {
        return new Result<TDestination>(default, Code, Error);
    }

    public IResult ToActionResult()
    {
        return Code switch
        {
            ResultCode.Success => Value is not null ? Results.Ok(Value) : Results.NoContent(),
            ResultCode.ValidationError => Results.BadRequest(Error),
            ResultCode.NotFound => Results.NotFound(Error),
            ResultCode.ServerError => Results.Problem(
                title: Error.Message,
                statusCode: (int)HttpStatusCode.InternalServerError),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

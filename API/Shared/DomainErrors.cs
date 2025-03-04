namespace API.Shared;

public static class DomainErrors
{
    public static readonly (ResultCode Code, Error Error) NotFound =
        (ResultCode.NotFound, new Error("Long Url Not Found."));
        
    public static readonly (ResultCode Code, Error Error) InvalidUrl =
        (ResultCode.ValidationError, new Error("The specified Url is invalid."));
}
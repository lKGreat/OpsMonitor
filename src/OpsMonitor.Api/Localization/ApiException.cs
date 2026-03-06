namespace OpsMonitor.Api.Localization;

public class ApiException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }
    public object[] Args { get; }

    public ApiException(string code, int statusCode = StatusCodes.Status400BadRequest, params object[] args)
        : base(code)
    {
        Code = code;
        StatusCode = statusCode;
        Args = args;
    }
}

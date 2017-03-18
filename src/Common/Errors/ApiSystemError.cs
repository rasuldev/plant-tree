namespace Common.Errors
{
    public class ApiSystemError : ApiError
    {
        public ApiSystemError(string message, string code = null) : base(message, code, ApiErrorTypes.System)
        {
        }
    }
}
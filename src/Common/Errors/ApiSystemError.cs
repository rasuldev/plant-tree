namespace Common.Errors
{
    public class ApiSystemError : ApiError
    {
        public ApiSystemError(string message) : base(message, ApiErrorTypes.System)
        {
        }
    }
}
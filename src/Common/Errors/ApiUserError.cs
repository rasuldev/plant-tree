namespace Common.Errors
{
    public class ApiUserError : ApiError
    {
        public ApiUserError(string message, string code) : base(message, code, ApiErrorTypes.User)
        {
        }
    }
}
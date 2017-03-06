namespace Common.Errors
{
    public class ApiUserError : ApiError
    {
        public ApiUserError(string message) : base(message, ApiErrorTypes.User)
        {
        }
    }
}
namespace Common.Errors
{
    public class ApiError
    {
        public ApiErrorTypes ErrorType { get; protected set; }
        public string Message { get; protected set; }

        public ApiError(string message, ApiErrorTypes type = ApiErrorTypes.System)
        {
            Message = message;
            ErrorType = type;
        }

        public override string ToString()
        {
            return $"{ErrorType} error: {Message}";
        }
    }
}
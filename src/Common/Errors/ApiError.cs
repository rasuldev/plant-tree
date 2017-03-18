namespace Common.Errors
{
    public class ApiError
    {
        public string Type => ErrorType.ToString();
        protected ApiErrorTypes ErrorType { get; set; }
        public string Code { get; protected set; }
        public string Message { get; protected set; }

        public ApiError(string message, string code = null, ApiErrorTypes type = ApiErrorTypes.System)
        {
            Code = code;
            Message = message;
            ErrorType = type;
        }

        public override string ToString()
        {
            return $"Error with type: {ErrorType}, code: {Code}, message: {Message}.";
        }
    }
}
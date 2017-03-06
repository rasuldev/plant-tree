using System;

namespace Common.Errors
{
    public class ApiException : Exception
    {
        public ApiError Error { get; protected set; }

        public ApiException(ApiError error) : base(error.ToString())
        {
            Error = error;
        }

        public ApiException(string message, ApiErrorTypes type = ApiErrorTypes.System): this(new ApiError(message, type))
        {
        }
    }
}
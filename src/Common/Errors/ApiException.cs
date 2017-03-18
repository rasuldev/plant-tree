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

        public ApiException(string message, string code = null, ApiErrorTypes type = ApiErrorTypes.System): this(new ApiError(message, code, type))
        {
        }
    }
}
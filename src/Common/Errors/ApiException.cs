using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Errors
{
    public class ApiException : Exception
    {
        public IReadOnlyList<ApiError> Errors { get; protected set; }

        public ApiException(params ApiError[] errors) : base(string.Join("; ", errors.Select(e => e.ToString())))
        {
            Errors = new List<ApiError>(errors);
        }

        public ApiException(string message, string code = null, ApiErrorTypes type = ApiErrorTypes.System) : this(new ApiError(message, code, type))
        {
        }
    }
}
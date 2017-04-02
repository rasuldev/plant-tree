using System.Collections.Generic;
using Common.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common.Results
{
    public class ApiErrorResult : ObjectResult
    {
        public IReadOnlyList<ApiError> Errors { get; protected set; }

        public ApiErrorResult(params ApiError[] errors) : base(errors)
        {
            Errors = new List<ApiError>(errors);
            StatusCode = StatusCodes.Status400BadRequest;
        }

        public ApiErrorResult(string message, string code = null, ApiErrorTypes type = ApiErrorTypes.System) : this(new ApiError(message, code, type))
        {
        }

        //public ApiErrorResult(IEnumerable<IdentityError> identityErrors) :
        //    this(identityErrors.Select(i => new ApiSystemError(i.Description)).ToArray())
        //{
        //}
    }
}
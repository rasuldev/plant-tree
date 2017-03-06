using Microsoft.AspNetCore.Mvc;

namespace Common.Results
{
    public class CodeWithContentResult : ObjectResult
    {
        /// <summary>
        /// Creates a result with
        /// </summary>
        /// <param name="value">The value to format in the entity body.</param>
        /// <param name="statusCode"></param>
        public CodeWithContentResult(object value, int? statusCode = null)
          : base(value)
        {
            this.StatusCode = statusCode;
        }
    }    
}
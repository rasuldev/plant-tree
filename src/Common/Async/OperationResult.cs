using System.Collections.Generic;
using System.Linq;
using Common.Errors;

namespace Common.Async
{
    public class OperationResult
    {
        /// <summary>
        /// Flag indicating whether if the operation succeeded or not.
        /// </summary>
        /// <value>True if the operation succeeded, otherwise false.</value>
        public bool Succeeded { get; protected set; }

        /// <summary>        
        /// Errors that occurred during the identity operation.
        /// </summary>
        public List<ApiError> Errors { get; protected set; }

        /// <summary>
        /// Returns an OperationResult indicating a successful identity operation.
        /// </summary>
        public static OperationResult Success { get; } = new OperationResult()
        {
            Succeeded = true
        };

        /// <summary>
        /// Creates an OperationResult indicating a failed operation, with a list of <paramref name="errors" /> if applicable.
        /// </summary>
        /// <param name="errors">An optional array of OperationErrors which caused the operation to fail.</param>
        /// <returns>An OperationResult indicating a failed operation, with a list of <paramref name="errors" /> if applicable.</returns>
        public static OperationResult Failed(params ApiError[] errors)
        {
            var operationResult = new OperationResult()
            {
                Succeeded = false
            };
            if (errors != null)
                operationResult.Errors.AddRange(errors);
            return operationResult;
        }

        /// <summary>
        /// Converts the value of the current OperationResult object to its equivalent string representation.
        /// </summary>
        /// <returns>A string representation of the current OperationResult object.</returns>
        /// <remarks>
        /// If the operation was successful the ToString() will return "Succeeded" otherwise it returned
        /// "Failed : " followed by a comma delimited list of error messages from its OperationResult.Errors collection, if any.
        /// </remarks>
        public override string ToString()
        {
            if (!Succeeded)
                return string.Format("{0} : {1}", "Failed", string.Join(",", Errors.Select(x => x.ToString())));
            return "Succeeded";
        }
    }
}
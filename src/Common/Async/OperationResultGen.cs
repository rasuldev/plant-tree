using Common.Errors;

namespace Common.Async
{
    public class OperationResult<T>: OperationResult
    {
        public T Result { get; protected set; }

        /// <summary>
        /// Creates succeeded OperationResult
        /// </summary>
        /// <param name="result"></param>
        private OperationResult(T result)
        {
            Result = result;
            Succeeded = true;
        }

        private OperationResult(bool succeeded)
        {
            Succeeded = succeeded;
        }

        /// <summary>
        /// Returns an OperationResult indicating a successful identity operation.
        /// </summary>
        public new static OperationResult<T> Success(T result)
        {
            return new OperationResult<T>(result);
        }

        /// <summary>
        /// Creates an OperationResult indicating a failed operation, with a list of <paramref name="errors" /> if applicable.
        /// </summary>
        /// <param name="errors">An optional array of OperationErrors which caused the operation to fail.</param>
        /// <returns>An OperationResult indicating a failed operation, with a list of <paramref name="errors" /> if applicable.</returns>
        public new static OperationResult<T> Failed(params ApiError[] errors)
        {
            var operationResult = new OperationResult<T>(false);
            
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
        /// "Failed : " followed by a comma delimited list of error codes from its OperationResult.Errors collection, if any.
        /// </remarks>
        public override string ToString()
        {
            return !Succeeded ? base.ToString() : $"Succeeded: {Result}";
        }
    }
}
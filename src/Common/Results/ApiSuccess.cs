namespace Common.Results
{
    public class ApiSuccess
    {
        public string Message { get; set; }

        public ApiSuccess(string message = null)
        {
            Message = message;
        }

        public override string ToString()
        {
            return Message;
        }
    }
}
namespace Orders.API.Exceptions
{
    public class NotFoundException : Exception
    {
        public string ErrorCode { get; }
        public NotFoundException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    public class BusinessRuleException : Exception
    {
        public string ErrorCode { get; }
        public BusinessRuleException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    public class ValidationException : Exception
    {
        public string ErrorCode { get; }
        public ValidationException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
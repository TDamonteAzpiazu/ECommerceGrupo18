namespace Users.API.Exceptions
{
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
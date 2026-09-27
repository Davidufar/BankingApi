namespace BankingApi.Exceptions
{
    public class CustomUnauthorizedAccessException : Exception
    {
        public CustomUnauthorizedAccessException(string message) : base(message) { }
    }
}

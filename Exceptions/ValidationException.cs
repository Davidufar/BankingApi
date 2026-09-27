namespace BankingApi.Exceptions
{
    public class ValidationException : Exception
    {
        public string Msg { get; }

        public ValidationException(string msg)
            : base("One or more validation failures have occurred.")
        {
            Msg = msg;
        }
    }
}
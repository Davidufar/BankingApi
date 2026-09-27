namespace BankingApi.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SkipLoggingAttribute : Attribute
    {
    }
}

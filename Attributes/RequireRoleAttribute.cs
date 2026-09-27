using BankingApi.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Attributes
{
    public class RequireRoleAttribute : TypeFilterAttribute
    {
        public RequireRoleAttribute(string role) : base(typeof(RequireRoleFilter))
        {
            // Pass arguments into the filter's constructor
            Arguments = new object[] { role };
        }
    }
}

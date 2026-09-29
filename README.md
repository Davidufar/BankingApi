# BankingApi

DESCRIPTION
-----------
A secure, high-performance, and resilient banking RESTful API built with 
.NET 8.0 and SQL Server. This project features atomic financial 
transactions, JWT authentication with cached Refresh Tokens, and 
automatic sensitive data redaction in application logs.


KEY FEATURES
------------
- Atomic ACID transactions for fund transfers via IDbTransaction.
- JWT authentication & Refresh Token session management.
- In-memory caching (IMemoryCache) via the Decorator Pattern 
  (CachedAuthRepository) for optimal session retrieval and fault tolerance.
- Automatic sensitive data masking (SSN, Passwords) in logs using 
  a custom [SkipLogging] attribute.
- Custom authorization filters using TypeFilter for dynamic dependency 
  injection.
- Integrated OpenAPI / Swagger documentation.


TECH STACK
----------
- Framework: .NET 8.0 (ASP.NET Core Web API)
- Language: C# 12
- Data Access: Dapper / ADO.NET
- Database: Microsoft SQL Server
- Caching: Microsoft.Extensions.Caching.Memory
- Security: JWT (JSON Web Tokens), BCrypt.Net-Next


PROJECT STRUCTURE
-----------------
BankingApi/
├── Controllers/      -> API Endpoints (Auth, Account, Transfer)
├── Filters/          -> TypeFilter & Action Filters
├── Middleware/       -> Logging & Exception Handling Middleware
├── Models/           -> C# Records & DTOs
└── Infrastructure/
    ├── Persistence/  -> IAuthRepository & SQL Implementation (Dapper)
    └── Repositories/ -> CachedAuthRepository (Decorator Pattern)


CONFIGURATION & SETUP
---------------------
1. Prerequisites: .NET 8.0 SDK, SQL Server.
2. Clone the repository:
   git clone https://github.com/your-username/BankingApi.git
   cd BankingApi

3. Configure connection string and JWT settings in appsettings.json:
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=BankingDb;Trusted_Connection=True;TrustServerCertificate=True;"
     },
     "Jwt": {
       "Secret": "YOUR_SUPER_SECRET_KEY_AT_LEAST_32_BYTES_LONG"
     }
   }

4. Run the application:
   dotnet restore
   dotnet build
   dotnet run


MAIN ENDPOINTS
--------------
- POST /api/auth/register    : Register a new client
- POST /api/auth/login       : Client authentication
- POST /api/auth/refresh     : Refresh access token (IMemoryCache priority)
- POST /api/account/transfer : Atomic fund transfer between accounts
- GET  /api/client/search    : Client search (partial SSN lookup)

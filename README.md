# Automated Expense Tracker

A full-stack personal finance application for importing bank statements, categorizing transactions, tracking budgets, and detecting unusual spending.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 18 + TypeScript + Vite |
| Backend | C# / ASP.NET Core 8 Web API |
| Database | MySQL 8 |
| ORM | Entity Framework Core (Pomelo MySQL) |
| Auth | JWT Bearer tokens |
| Charts | Recharts |

---

## Project Structure

```
expenseTracker/
├── backend/
│   ├── ExpenseTracker.API/      # ASP.NET Core Web API
│   │   ├── Controllers/         # HTTP endpoints (thin layer)
│   │   ├── Services/            # Business logic
│   │   ├── Repositories/        # Data access (EF Core)
│   │   ├── Models/              # EF entity classes
│   │   ├── DTOs/                # API request/response shapes
│   │   ├── Parsers/             # Bank CSV parsers
│   │   ├── Data/                # DbContext + seed data
│   │   ├── Helpers/             # JWT, hashing utilities
│   │   └── Middleware/          # Global error handling
│   └── ExpenseTracker.Tests/    # xUnit unit tests
├── frontend/
│   └── expense-tracker-ui/      # React + TypeScript
│       └── src/
│           ├── api/             # Typed API functions (Axios)
│           ├── components/      # Reusable UI components
│           ├── pages/           # Route-level page components
│           ├── context/         # React Context (Auth)
│           └── types/           # Shared TypeScript interfaces
└── docs/
    ├── sample-pnc.csv           # Sample PNC bank statement for testing
    └── postman/                 # Postman collection
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) and npm
- [MySQL 8](https://dev.mysql.com/downloads/mysql/)

---

## Setup Instructions

### 1. Database

Create the MySQL database:

```sql
CREATE DATABASE expense_tracker_dev;
CREATE USER 'expenseuser'@'localhost' IDENTIFIED BY 'yourpassword';
GRANT ALL ON expense_tracker_dev.* TO 'expenseuser'@'localhost';
FLUSH PRIVILEGES;
```

### 2. Backend

```bash
cd backend/ExpenseTracker.API

# Update database connection string in appsettings.Development.json
# Change "server=localhost;port=3306;database=expense_tracker_dev;user=root;password=yourpassword"

# Install .NET tool for EF migrations (first time only)
dotnet tool install --global dotnet-ef

# Create and apply database migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# Run the API (listens on https://localhost:5001)
dotnet run
```

The API starts at `https://localhost:5001`.
Swagger UI is available at `https://localhost:5001/swagger`.

### 3. Frontend

```bash
cd frontend/expense-tracker-ui

# Install dependencies
npm install

# Start the dev server (listens on http://localhost:5173)
npm run dev
```

Open http://localhost:5173 in your browser.

---

## Running Tests

```bash
cd backend
dotnet test
```

---

## PNC Bank Statement Import

1. Log in to PNC Online Banking
2. Go to **Accounts → Account Activity**
3. Select date range
4. Click **Download Activity** → **CSV**
5. In the app, go to **Upload** page
6. Select **PNC** as the bank and upload the CSV

The CSV must have these columns:
```
Date,Description,Withdrawals,Deposits,Balance
```

A sample CSV is in `docs/sample-pnc.csv`.

---

## Adding a New Bank

1. **Create the parser** in `backend/ExpenseTracker.API/Parsers/`:
   ```csharp
   public class ChaseParser : IBankStatementParser
   {
       public string BankName => "Chase";
       public async Task<ParseResult> ParseAsync(Stream csvStream) { ... }
   }
   ```

2. **Register it** in `BankParserFactory.cs`:
   ```csharp
   _parsers = new List<IBankStatementParser>
   {
       new PncBankParser(),
       new ChaseParser(),   // ← Add this line
   };
   ```

3. **Add to frontend** in `UploadPage.tsx`:
   ```typescript
   const SUPPORTED_BANKS = ['PNC', 'Chase'];
   ```

That's all — no other changes needed.

---

## Enabling AI Categorization (Google Gemini)

The categorization system is designed to be swappable. To switch from rule-based to AI:

1. Create `GeminiCategorizationService.cs` implementing `ICategorizationService`
2. In `Program.cs`, change:
   ```csharp
   // Before:
   builder.Services.AddScoped<ICategorizationService, RuleBasedCategorizationService>();
   // After:
   builder.Services.AddScoped<ICategorizationService, GeminiCategorizationService>();
   ```

---

## API Reference

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | /api/auth/register | No | Create account |
| POST | /api/auth/login | No | Get JWT token |
| POST | /api/statements/upload | Yes | Upload CSV |
| GET | /api/statements | Yes | Upload history |
| GET | /api/transactions | Yes | Filtered list |
| PUT | /api/transactions/{id}/category | Yes | Re-categorize |
| DELETE | /api/transactions/{id} | Yes | Delete |
| GET | /api/analytics/dashboard | Yes | Dashboard data |
| GET | /api/analytics/monthly | Yes | Monthly breakdown |
| GET | /api/analytics/yearly | Yes | Yearly breakdown |
| GET | /api/analytics/trends | Yes | 6-month trend |
| GET | /api/budgets | Yes | Budget list |
| POST | /api/budgets | Yes | Create budget |
| PUT | /api/budgets/{id} | Yes | Update limit |
| DELETE | /api/budgets/{id} | Yes | Delete budget |
| GET | /api/alerts | Yes | Alert list |
| PUT | /api/alerts/{id}/read | Yes | Mark read |
| PUT | /api/alerts/read-all | Yes | Dismiss all |
| GET | /api/categories | Yes | Category list |
| POST | /api/categories | Yes | Create category |

---

## Anomaly Detection

The system automatically flags transactions that match any of these rules:
- **Large purchase**: amount > $500
- **New merchant**: no prior transactions at this merchant
- **Spending spike**: transaction is 3× the 3-month average for its category

Flagged transactions are marked `isAnomaly = true` and generate an Alert.

---

## Security Notes

- Passwords are hashed with BCrypt (work factor 12)
- JWT tokens expire in 24 hours (configurable in `JwtSettings:ExpiryMinutes`)
- All authenticated endpoints verify the JWT and extract `userId` from claims
- Users can only access their own data — all queries include `WHERE userId = ?`
- Input validation on all API endpoints using DataAnnotations

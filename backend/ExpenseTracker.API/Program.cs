using System.Text;
using ExpenseTracker.API.Data;
using ExpenseTracker.API.Helpers;
using ExpenseTracker.API.Middleware;
using ExpenseTracker.API.Parsers;
using ExpenseTracker.API.Repositories;
using ExpenseTracker.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

/*
 * Program.cs — Application entry point and composition root.
 *
 * This file has two responsibilities:
 * 1. Register services in the Dependency Injection (DI) container
 *    (everything between "var builder" and "var app = builder.Build()")
 * 2. Configure the HTTP request pipeline (middleware)
 *    (everything after "var app = builder.Build()")
 *
 * DEPENDENCY INJECTION PATTERN:
 * Instead of classes creating their own dependencies (new UserRepository()),
 * we register all dependencies here and let ASP.NET Core inject them.
 * This makes testing easy — swap real repos with mocks in unit tests.
 *
 * MIDDLEWARE PIPELINE:
 * HTTP requests pass through middleware in the order registered below.
 * Each middleware can inspect/modify the request AND response.
 */

var builder = WebApplication.CreateBuilder(args);

// =====================================================================
// 1. DATABASE
// =====================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()  // Auto-retry on transient failures
    ));

// =====================================================================
// 2. AUTHENTICATION (JWT Bearer)
// =====================================================================
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JWT Secret not configured.");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "ExpenseTrackerAPI";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "ExpenseTrackerClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,           // Reject expired tokens
            ValidateIssuerSigningKey = true,   // Verify signature
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero          // Don't add extra time buffer to token expiry
        };
    });

builder.Services.AddAuthorization();

// =====================================================================
// 3. CORS (Cross-Origin Resource Sharing)
// Allows the React frontend (localhost:5173) to call the API (localhost:5001)
// =====================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",   // Vite dev server
            "http://localhost:3000",   // Alternative dev port
            "http://localhost:4173"    // Vite preview
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// =====================================================================
// 4. SWAGGER / OPENAPI
// Interactive API documentation available at /swagger in development
// =====================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Expense Tracker API",
        Version = "v1",
        Description = "REST API for the Automated Expense Tracker application"
    });

    // Configure Swagger to accept JWT tokens for testing authenticated endpoints
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Enter: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// =====================================================================
// 5. REGISTER APPLICATION SERVICES (DEPENDENCY INJECTION)
// =====================================================================

// Helpers
builder.Services.AddScoped<JwtHelper>();

// Parsers — singleton because they have no state
builder.Services.AddSingleton<BankParserFactory>();

// Repositories — scoped to the HTTP request (one instance per request)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IStatementRepository, StatementRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IStatementService, StatementService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();

// Categorization service — swap this line to use AI categorization in the future:
// builder.Services.AddScoped<ICategorizationService, GeminiCategorizationService>();
builder.Services.AddScoped<ICategorizationService, RuleBasedCategorizationService>();

builder.Services.AddScoped<IAnomalyDetectionService, AnomalyDetectionService>();

builder.Services.AddControllers();

// =====================================================================
// BUILD THE APPLICATION
// =====================================================================
var app = builder.Build();

// =====================================================================
// 6. SEED THE DATABASE
// Run on startup — creates tables (if not exist) and seeds default categories.
// =====================================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.SeedAsync(db);
}

// =====================================================================
// 7. CONFIGURE THE HTTP REQUEST PIPELINE (MIDDLEWARE ORDER MATTERS)
// =====================================================================

// Global exception handler — must be FIRST to catch all errors
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Expense Tracker API v1"));
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");   // CORS before Authentication
app.UseAuthentication();          // Validate JWT tokens
app.UseAuthorization();           // Check [Authorize] attributes
app.MapControllers();

app.Run();

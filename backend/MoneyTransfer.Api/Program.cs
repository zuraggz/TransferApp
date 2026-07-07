using Microsoft.EntityFrameworkCore;
using MoneyTransfer.Api.Data;
using MoneyTransfer.Api.Middleware;
using MoneyTransfer.Api.Services.Accounts;
using MoneyTransfer.Api.Services.Transactions;
using MoneyTransfer.Api.Services.Transfers;

var builder = WebApplication.CreateBuilder(args);

// ---- Services ----------------------------------------------------------

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register one service per bounded area of business logic. Add new
// interfaces/implementations here as the domain grows (e.g. IUserService).
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITransferService, TransferService>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Money Transfer API",
        Version = "v1",
        Description = "API for managing accounts and executing money transfers between them."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// ---- Database migration (development convenience) -----------------------
// Applies any pending EF Core migrations on startup so the API is usable
// immediately after `docker-compose up` without a manual migration step.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to apply database migrations on startup.");
    }
}

// ---- Middleware pipeline -------------------------------------------------

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Money Transfer API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("FrontendClient");
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>
/// Exposed so integration tests can bootstrap the API via WebApplicationFactory.
/// </summary>
public partial class Program { }

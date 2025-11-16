using LibraryTracking.Data;
using LibraryTracking.Services;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.SqlServer;
using LibraryTracking.Data.Queries;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// EF Core + SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Hangfire (use same SQL Server)
builder.Services.AddHangfire(cfg =>
{
    cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings()
       .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
       {
           SchemaName = configuration.GetValue<string>("Hangfire:SchemaName") ?? "hangfire",
           PrepareSchemaIfNecessary = true,
           QueuePollInterval = TimeSpan.FromSeconds(15)
       });
});
builder.Services.AddHangfireServer();
builder.Services.AddScoped<BookQueries>();
// Register DI services
builder.Services.AddScoped<BookEventHandler>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<CleanupService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure Hangfire dashboard (optional authentication not included here)
app.UseHangfireDashboard("/hangfire");
using (var scope = app.Services.CreateScope())
{
    var cleanupService = scope.ServiceProvider.GetRequiredService<CleanupService>();
    RecurringJob.AddOrUpdate(
        "cleanup-soft-deleted",
        () => cleanupService.CleanSoftDeletedOlderThanDays(0),
        "*/3 * * * *"
    );
}

// Map controllers
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var cleanupService = scope.ServiceProvider.GetRequiredService<CleanupService>();
    // 0 يوم → يمسح كل Soft Deleted فوراً
    await cleanupService.CleanSoftDeletedOlderThanDays(0);
}


app.Run();

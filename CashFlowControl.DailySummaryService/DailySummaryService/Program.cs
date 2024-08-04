using CashFlowControl.Infrastructure.Data;
using CashFlowControl.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://*:80");

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is null or empty.");
}

builder.Services.AddDbContext<CashFlowContext>(options =>
    options.UseMySQL(connectionString));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<DailySummaryService>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting application");

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CashFlowContext>();
    try
    {
        dbContext.Database.Migrate();    
        logger.LogInformation("Applied database migrations");
    }
    catch (Exception ex)
    {    
        logger.LogError(ex, "An error occurred while applying the database migrations");
    }
    
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    logger.LogInformation("Configuring Swagger in Development environment");
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DailySummaryService API V1");
        logger.LogInformation("Swagger endpoint configured");
    });
    app.UseDeveloperExceptionPage(); 
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

logger.LogInformation("Application is running");
app.Run();

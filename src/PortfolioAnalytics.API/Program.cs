using PortfolioAnalytics.API.Data;
using PortfolioAnalytics.API.Services;
using PortfolioAnalytics.API.Services.Interfaces;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDataContext, DataContext>();
builder.Services.AddTransient<IPerformanceCalculator, PerformanceCalculator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
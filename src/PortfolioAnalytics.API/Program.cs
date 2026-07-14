using PortfolioAnalytics.API.Data;
using PortfolioAnalytics.API.Services;
using PortfolioAnalytics.API.Services.Interfaces;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Portfolio Analytics API",
        Version = "v1",
        Description = "Web API focada em algoritmos financeiros, analytics de risco, e otimização de rebalanceamento de carteiras.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Seu Nome",
            Email = "seu.email@exemplo.com",
            Url = new Uri("https://github.com/seu-usuario")
        }
    });

    // Configura o Swagger para ler os comentários XML do código
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddSingleton<IDataContext, DataContext>();
builder.Services.AddTransient<IPerformanceCalculator, PerformanceCalculator>();
builder.Services.AddTransient<IRiskAnalyzer, RiskAnalyzer>();
builder.Services.AddTransient<IRebalancingOptimizer, RebalancingOptimizer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Portfolio Analytics API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
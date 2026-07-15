namespace PortfolioAnalytics.API.Models.DTOs;

public class PerformanceResponse
{
    public decimal TotalInvestment { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal TotalReturn { get; set; }
    public decimal TotalReturnAmount { get; set; }
    public decimal? AnnualizedReturn { get; set; }
    public decimal? Volatility { get; set; }
    public List<PositionPerformanceDto> PositionsPerformance { get; set; } = [];
}

public class PositionPerformanceDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal InvestedAmount { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal Return { get; set; }
    public decimal Weight { get; set; }
}
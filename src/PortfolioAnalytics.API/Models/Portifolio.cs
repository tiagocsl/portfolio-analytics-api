namespace PortfolioAnalytics.API.Models;

public class Portfolio
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public decimal TotalInvestment { get; set; }
    public List<Position> Positions { get; set; } = [];
}
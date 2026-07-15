namespace PortfolioAnalytics.API.Models.DTOs;

public class MarketData
{
    public decimal SelicRate { get; set; }
}

public class SeedDataWrapper
{
    public List<Asset> Assets { get; set; } = [];
    public List<Portfolio> Portfolios { get; set; } = [];
    public Dictionary<string, List<PriceHistory>> PriceHistory { get; set; } = [];
    public MarketData MarketData { get; set; } = new();
}
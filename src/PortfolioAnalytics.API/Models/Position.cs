namespace PortfolioAnalytics.API.Models;

public class Position
{
    public string AssetSymbol { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal TargetAllocation { get; set; } 
}
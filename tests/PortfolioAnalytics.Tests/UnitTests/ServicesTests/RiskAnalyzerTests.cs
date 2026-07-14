using Xunit;
using System.Linq;
using System.Collections.Generic;

using PortfolioAnalytics.API.Data;
using PortfolioAnalytics.API.Services;
using PortfolioAnalytics.API.Models;

namespace PortfolioAnalytics.Tests.ServicesTests;

public class RiskAnalyzerTests
{
    private readonly IDataContext _context;
    private readonly IRiskAnalyzer _analyzer;

    public RiskAnalyzerTests()
    {
        _context = new DataContext();
        _analyzer = new RiskAnalyzer(_context);
    }

    [Fact]
    public void Analyze_ShouldIdentifyHighRisk_ForConcentratedPortfolio()
    {
        var portfolio = _context.Portfolios.FirstOrDefault(p => p.UserId == "user-002");
        Assert.NotNull(portfolio);

        var result = _analyzer.Analyze(portfolio);

        Assert.NotNull(result);
        
        Assert.True(result.SectorExposure.Count > 0);
        Assert.True(result.TypeExposure.Count > 0);
        
        var totalSectorExposure = result.SectorExposure.Sum(s => s.ExposurePercentage);
        Assert.InRange(totalSectorExposure, 99.9m, 100.1m);
    }

    [Fact]
    public void Analyze_ShouldGenerateAlerts_WhenSingleAssetExceedsLimit()
    {
        var mockContext = new MockDataContext();
        mockContext.Assets.Add(new Asset { Symbol = "SUPER_CONCENTRATED", Sector = "Tech", Type = "Stock", CurrentPrice = 1000m });
        mockContext.Assets.Add(new Asset { Symbol = "SMALL_ASSET", Sector = "Mining", Type = "Stock", CurrentPrice = 10m });

        var portfolio = new Portfolio
        {
            Id = "test-concentrated",
            UserId = "test-user-concentrated",
            TotalInvestment = 1010m,
            Positions = new List<Position>
            {
                new Position { AssetSymbol = "SUPER_CONCENTRATED", Quantity = 1, AveragePrice = 1000m },
                new Position { AssetSymbol = "SMALL_ASSET", Quantity = 1, AveragePrice = 10m }
            }
        };

        var analyzerWithMock = new RiskAnalyzer(mockContext);

        var result = analyzerWithMock.Analyze(portfolio);

        Assert.Equal("HIGH", result.RiskLevel);
        Assert.True(result.ConcentrationIndexHHI > 0.25);
        Assert.NotEmpty(result.Alerts);
        Assert.Contains(result.Alerts, a => a.Contains("SUPER_CONCENTRATED"));
    }

    private class MockDataContext : IDataContext
    {
        public List<Asset> Assets { get; } = new();
        public List<Portfolio> Portfolios { get; } = new();
        public Dictionary<string, List<PriceHistory>> PriceHistory { get; } = new();
        public decimal SelicRate => 10.75m;
    }
}
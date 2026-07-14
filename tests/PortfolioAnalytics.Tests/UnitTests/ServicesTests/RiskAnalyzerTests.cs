using Xunit;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;

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
        _context = new DataContext(NullLogger<DataContext>.Instance);
        _analyzer = new RiskAnalyzer(_context, NullLogger<RiskAnalyzer>.Instance);
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

        var analyzerWithMock = new RiskAnalyzer(mockContext, NullLogger<RiskAnalyzer>.Instance);

        var result = analyzerWithMock.Analyze(portfolio);

        Assert.Equal("HIGH", result.RiskLevel);
        Assert.True(result.ConcentrationIndexHHI > 0.25);
        Assert.NotEmpty(result.Alerts);
        Assert.Contains(result.Alerts, a => a.Contains("SUPER_CONCENTRATED"));
    }

    [Fact]
    public void Analyze_ShouldThrowArgumentNullException_WhenPortfolioIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _analyzer.Analyze(null!));
    }

    [Fact]
    public void Analyze_ShouldReturnLowRiskAndWarning_WhenPortfolioIsEmptyOrHasZeroValue()
    {
        var mockContext = new MockDataContext();
        var emptyPortfolio = new Portfolio
        {
            Id = "empty-risk",
            UserId = "user-empty-risk",
            TotalInvestment = 1000m,
            Positions = new List<Position>()
        };

        var analyzerWithMock = new RiskAnalyzer(mockContext, NullLogger<RiskAnalyzer>.Instance);

        var result = analyzerWithMock.Analyze(emptyPortfolio);

        Assert.Equal("LOW", result.RiskLevel);
        Assert.Contains(result.Alerts, a => a.Contains("não possui posições ativas"));
    }

    [Fact]
    public void Analyze_ShouldSkipPosition_WhenAssetDoesNotExistInContext()
    {
        var mockContext = new MockDataContext();
        mockContext.Assets.Add(new Asset { Symbol = "EXISTING", Sector = "Tech", Type = "Stock", CurrentPrice = 100m });

        var portfolio = new Portfolio
        {
            Id = "test-missing-asset",
            UserId = "user-missing-asset",
            TotalInvestment = 2000m,
            Positions = new List<Position>
            {
                new Position { AssetSymbol = "EXISTING", Quantity = 10, AveragePrice = 100m },
                new Position { AssetSymbol = "NON_EXISTING", Quantity = 10, AveragePrice = 100m }
            }
        };

        var analyzerWithMock = new RiskAnalyzer(mockContext, NullLogger<RiskAnalyzer>.Instance);

        var result = analyzerWithMock.Analyze(portfolio);

        Assert.Single(result.SectorExposure);
        Assert.Equal(100m, result.SectorExposure.First().ExposurePercentage);
    }

    [Fact]
    public void Analyze_ShouldReturnLowRisk_WhenPortfolioIsHighlyDiversified()
    {
        var mockContext = new MockDataContext();
        var assets = new List<Asset>();
        var positions = new List<Position>();

        for (int i = 1; i <= 10; i++)
        {
            string symbol = $"A{i}";
            mockContext.Assets.Add(new Asset { Symbol = symbol, Sector = $"Sector{i}", Type = "Stock", CurrentPrice = 100m });
            positions.Add(new Position { AssetSymbol = symbol, Quantity = 1, AveragePrice = 100m });
        }

        var diversifiedPortfolio = new Portfolio
        {
            Id = "test-diversified",
            UserId = "user-diversified",
            TotalInvestment = 1000m,
            Positions = positions
        };

        var analyzerWithMock = new RiskAnalyzer(mockContext, NullLogger<RiskAnalyzer>.Instance);

        var result = analyzerWithMock.Analyze(diversifiedPortfolio);

        Assert.True(result.ConcentrationIndexHHI < 0.15);
        Assert.Equal("LOW", result.RiskLevel);
    }


    private class MockDataContext : IDataContext
    {
        public List<Asset> Assets { get; } = new();
        public List<Portfolio> Portfolios { get; } = new();
        public Dictionary<string, List<PriceHistory>> PriceHistory { get; } = new();
        public decimal SelicRate => 10.75m;
    }
}
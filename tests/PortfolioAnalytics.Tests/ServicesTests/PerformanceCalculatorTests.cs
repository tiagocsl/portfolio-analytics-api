using Xunit;
using System.Linq;
using System.Collections.Generic;

using PortfolioAnalytics.API.Data;
using PortfolioAnalytics.API.Models;
using PortfolioAnalytics.API.Services;
using PortfolioAnalytics.API.Services.Interfaces;

namespace PortfolioAnalytics.Tests.ServicesTests;

public class PerformanceCalculatorTests
{
    private readonly IDataContext _context;
    private readonly IPerformanceCalculator _calculator;

    public PerformanceCalculatorTests()
    {
        _context = new DataContext();
        _calculator = new PerformanceCalculator(_context);
    }

    [Fact]
    public void Calculate_ShouldReturnCorrectPerformance_ForConservativePortfolio()
    {
        var portfolio = _context.Portfolios.FirstOrDefault(p => p.UserId == "user-001");
        Assert.NotNull(portfolio);

        var result = _calculator.Calculate(portfolio);

        Assert.NotNull(result);
        
        Assert.Equal(portfolio.TotalInvestment, result.TotalInvestment);
        Assert.True(result.CurrentValue > 0, "O valor atual do portfólio deve ser maior que zero.");
        Assert.Equal(result.CurrentValue - result.TotalInvestment, result.TotalReturnAmount);
        
        var totalWeight = result.PositionsPerformance.Sum(p => p.Weight);
        Assert.InRange(totalWeight, 99.9m, 100.1m);

        foreach (var pos in result.PositionsPerformance)
        {
            Assert.True(pos.InvestedAmount > 0);
            Assert.True(pos.CurrentValue > 0);
        }
    }

    [Fact]
    public void Calculate_ShouldReturnNullVolatility_WhenAssetHasNoPriceHistory()
    {
        var mockContext = new MockDataContext();
        
        var assetWithoutHistory = new Asset 
        { 
            Symbol = "MOCK1", 
            CurrentPrice = 100.0m 
        };
        mockContext.Assets.Add(assetWithoutHistory);

        var portfolio = new Portfolio
        {
            Id = "test-empty",
            UserId = "test-user-empty",
            TotalInvestment = 1000.0m,
            Positions = new List<Position>
            {
                new Position { AssetSymbol = "MOCK1", Quantity = 10, AveragePrice = 100.0m, TargetAllocation = 1.0m }
            }
        };

        var calculatorWithMock = new PerformanceCalculator(mockContext);

        var result = calculatorWithMock.Calculate(portfolio);

        Assert.Null(result.Volatility);
    }

    private class MockDataContext : IDataContext
    {
        public List<Asset> Assets { get; } = new();
        public List<Portfolio> Portfolios { get; } = new();
        public Dictionary<string, List<PriceHistory>> PriceHistory { get; } = new();
        public decimal SelicRate => 10.75m;
    }
}
using Xunit;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;

using PortfolioAnalytics.API.Data;
using PortfolioAnalytics.API.Services;
using PortfolioAnalytics.API.Models;

namespace PortfolioAnalytics.Tests.ServicesTests;

public class RebalancingOptimizerTests
{
    private readonly IDataContext _context;
    private readonly IRebalancingOptimizer _optimizer;

    public RebalancingOptimizerTests()
    {
        _context = new DataContext(NullLogger<DataContext>.Instance);
        _optimizer = new RebalancingOptimizer(_context, NullLogger<RebalancingOptimizer>.Instance);
    }

    [Fact]
    public void Optimize_ShouldGenerateInstructions_WhenPortfolioIsUnbalanced()
    {
        var portfolio = _context.Portfolios.FirstOrDefault(p => p.UserId == "user-003");
        Assert.NotNull(portfolio);

        var result = _optimizer.Optimize(portfolio);

        Assert.NotNull(result);
        Assert.True(result.TotalValue > 0);
        Assert.NotEmpty(result.CurrentVsTargetAllocation);

        foreach (var inst in result.Instructions)
        {
            Assert.Contains(inst.Action, new[] { "BUY", "SELL" });
            Assert.True(inst.Quantity > 0);
            Assert.Equal(inst.Quantity * inst.Price, inst.EstimatedCost);
        }
    }

    [Fact]
    public void Optimize_ShouldNotGenerateInstructions_WhenPortfolioIsPerfectedBalanced()
    {
        var mockContext = new MockDataContext();
        mockContext.Assets.Add(new Asset { Symbol = "BALANCED", CurrentPrice = 100m });

        var portfolio = new Portfolio
        {
            Id = "test-balanced",
            UserId = "test-user-balanced",
            TotalInvestment = 1000m,
            Positions = new List<Position>
            {
                new Position { AssetSymbol = "BALANCED", Quantity = 10, AveragePrice = 100m, TargetAllocation = 1.0m }
            }
        };

        var optimizerWithMock = new RebalancingOptimizer(mockContext, NullLogger<RebalancingOptimizer>.Instance);

        var result = optimizerWithMock.Optimize(portfolio);

        Assert.Empty(result.Instructions);
        var allocation = result.CurrentVsTargetAllocation.First();
        Assert.Equal(100m, allocation.CurrentAllocation);
        Assert.Equal(100m, allocation.TargetAllocation);
        Assert.Equal(0m, allocation.Deviation);
    }

    [Fact]
    public void MockDataContext_SelicRate_ShouldBeCovered()
    {
        var mock = new MockDataContext();
        Assert.Equal(10.75m, mock.SelicRate);
    }

    [Fact]
    public void Optimize_ShouldThrowArgumentNullException_WhenPortfolioIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _optimizer.Optimize(null!));
    }

    [Fact]
    public void Optimize_ShouldSkipPosition_WhenAssetDoesNotExistInContext()
    {
        var mockContext = new MockDataContext();

        var portfolio = new Portfolio
        {
            Id = "test-missing-asset-rebalancing",
            UserId = "user-missing-rebalancing",
            TotalInvestment = 1000m,
            Positions = new List<Position>
            {
                new Position { AssetSymbol = "NON_EXISTENT", Quantity = 10, AveragePrice = 100m, TargetAllocation = 1.0m }
            }
        };

        var optimizerWithMock = new RebalancingOptimizer(mockContext, NullLogger<RebalancingOptimizer>.Instance);

        var result = optimizerWithMock.Optimize(portfolio);

        Assert.Equal(0m, result.TotalValue);
        Assert.Empty(result.CurrentVsTargetAllocation);
        Assert.Empty(result.Instructions);
    }

    [Fact]
    public void Optimize_ShouldReturnEmptyResponse_WhenTotalValueIsZeroOrNegative()
    {
        var mockContext = new MockDataContext();
        mockContext.Assets.Add(new Asset { Symbol = "FREE_ASSET", CurrentPrice = 0m });

        var portfolio = new Portfolio
        {
            Id = "zero-value",
            UserId = "user-zero-value",
            TotalInvestment = 1000m,
            Positions = new List<Position>
            {
                new Position { AssetSymbol = "FREE_ASSET", Quantity = 10, AveragePrice = 100m, TargetAllocation = 1.0m }
            }
        };

        var optimizerWithMock = new RebalancingOptimizer(mockContext, NullLogger<RebalancingOptimizer>.Instance);

        var result = optimizerWithMock.Optimize(portfolio);

        Assert.Equal(0m, result.TotalValue);
        Assert.Empty(result.CurrentVsTargetAllocation);
        Assert.Empty(result.Instructions);
    }

    [Fact]
    public void Optimize_ShouldNormalizeTargetAllocation_WhenSumIsNotOneHundredPercent()
    {
        var mockContext = new MockDataContext();
        mockContext.Assets.Add(new Asset { Symbol = "A", CurrentPrice = 100m });
        mockContext.Assets.Add(new Asset { Symbol = "B", CurrentPrice = 100m });

        var portfolio = new Portfolio
        {
            Id = "unbalanced-targets",
            UserId = "user-unbalanced-targets",
            TotalInvestment = 1000m,
            Positions = new List<Position>
            {
                new Position { AssetSymbol = "A", Quantity = 5, AveragePrice = 100m, TargetAllocation = 0.40m },
                new Position { AssetSymbol = "B", Quantity = 5, AveragePrice = 100m, TargetAllocation = 0.40m }
            }
        };

        var optimizerWithMock = new RebalancingOptimizer(mockContext, NullLogger<RebalancingOptimizer>.Instance);

        var result = optimizerWithMock.Optimize(portfolio);

        Assert.NotNull(result);

        Assert.Empty(result.Instructions);

        foreach (var allocation in result.CurrentVsTargetAllocation)
        {
            Assert.Equal(50m, allocation.TargetAllocation);
            Assert.Equal(50m, allocation.CurrentAllocation);
            Assert.Equal(0m, allocation.Deviation);
        }
    }

    private class MockDataContext : IDataContext
    {
        public List<Asset> Assets { get; } = new();
        public List<Portfolio> Portfolios { get; } = new();
        public Dictionary<string, List<PriceHistory>> PriceHistory { get; } = new();
        public decimal SelicRate => 10.75m;
    }
}
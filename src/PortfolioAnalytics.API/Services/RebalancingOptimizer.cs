using PortfolioAnalytics.API.Data;
using PortfolioAnalytics.API.Models;
using PortfolioAnalytics.API.Models.DTOs;

namespace PortfolioAnalytics.API.Services;

public class RebalancingOptimizer : IRebalancingOptimizer
{
    private readonly IDataContext _context;

    public RebalancingOptimizer(IDataContext context)
    {
        _context = context;
    }

    public RebalancingResponse Optimize(Portfolio portfolio)
    {
        if (portfolio == null) throw new ArgumentNullException(nameof(portfolio));

        var response = new RebalancingResponse();
        var instructions = new List<RebalancingInstructionDto>();
        var allocations = new List<AssetAllocationDto>();

        decimal totalValue = 0;
        var activePositions = new List<(Position Position, Asset Asset, decimal CurrentValue)>();

        foreach (var position in portfolio.Positions)
        {
            var asset = _context.Assets.FirstOrDefault(a => a.Symbol == position.AssetSymbol);
            if (asset == null) continue;

            decimal posValue = position.Quantity * asset.CurrentPrice;
            totalValue += posValue;

            activePositions.Add((position, asset, posValue));
        }

        response.TotalValue = Math.Round(totalValue, 2);

        if (totalValue <= 0)
        {
            return response; 
        }

        foreach (var item in activePositions)
        {
            decimal currentPrice = item.Asset.CurrentPrice;
            
            decimal targetAllocationDecimal = item.Position.TargetAllocation; 
            decimal targetAllocationPercentage = targetAllocationDecimal * 100;

            decimal currentAllocationPercentage = (item.CurrentValue / totalValue) * 100;
            decimal deviation = currentAllocationPercentage - targetAllocationPercentage;

            allocations.Add(new AssetAllocationDto
            {
                AssetSymbol = item.Position.AssetSymbol,
                CurrentAllocation = Math.Round(currentAllocationPercentage, 2),
                TargetAllocation = Math.Round(targetAllocationPercentage, 2),
                Deviation = Math.Round(deviation, 2)
            });

            decimal targetValue = totalValue * targetAllocationDecimal;
            decimal differenceValue = targetValue - item.CurrentValue;

            if (Math.Abs(differenceValue) >= currentPrice)
            {
                int quantity = (int)Math.Round(Math.Abs(differenceValue) / currentPrice, MidpointRounding.AwayFromZero);

                if (quantity > 0)
                {
                    string action = differenceValue > 0 ? "BUY" : "SELL";

                    instructions.Add(new RebalancingInstructionDto
                    {
                        AssetSymbol = item.Position.AssetSymbol,
                        Action = action,
                        Quantity = quantity,
                        Price = currentPrice,
                        EstimatedCost = Math.Round(quantity * currentPrice, 2)
                    });
                }
            }
        }

        response.CurrentVsTargetAllocation = allocations.OrderByDescending(a => a.Deviation).ToList();
        response.Instructions = instructions.OrderByDescending(i => i.EstimatedCost).ToList();

        return response;
    }
}
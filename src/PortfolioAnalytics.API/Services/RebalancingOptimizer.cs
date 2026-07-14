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

        // 1. Calcular o Valor Atual de cada posição e o total da carteira
        decimal totalValue = 0;
        var activePositions = new List<(Position Position, Asset Asset, decimal CurrentValue)>();

        foreach (var position in portfolio.Positions)
        {
            var asset = _context.Assets.FirstOrDefault(a => a.Symbol.ToUpperInvariant() == position.AssetSymbol.ToUpperInvariant());
            if (asset == null) continue;

            decimal posValue = position.Quantity * asset.CurrentPrice;
            totalValue += posValue;

            activePositions.Add((position, asset, posValue));
        }

        response.TotalValue = Math.Round(totalValue, 2);

        if (totalValue <= 0 || !activePositions.Any())
        {
            return response; // Carteira vazia ou sem valor atual
        }

        // 2. Mitigação: Validar e Normalizar o TargetAllocation
        // Se a soma das metas for diferente de 1.0 (100%), normalizamos proporcionalmente.
        decimal totalTargetAllocation = activePositions.Sum(p => p.Position.TargetAllocation);
        
        // Usamos um dicionário para guardar as metas normalizadas temporariamente
        var normalizedTargets = new Dictionary<string, decimal>();

        // Tolerância para pequenas variações de arredondamento (ex: de 0.9999 a 1.0001)
        bool needsNormalization = Math.Abs(totalTargetAllocation - 1.0m) > 0.001m;

        foreach (var item in activePositions)
        {
            decimal originalTarget = item.Position.TargetAllocation;
            decimal normalizedTarget = originalTarget;

            if (needsNormalization && totalTargetAllocation > 0)
            {
                // Ex: Se o ativo tem meta de 20% em uma carteira cuja soma é 80%:
                // Novo Target = 0.20 / 0.80 = 0.25 (25%)
                normalizedTarget = originalTarget / totalTargetAllocation;
            }

            normalizedTargets[item.Position.AssetSymbol.ToUpperInvariant()] = normalizedTarget;
        }

        // 3. Analisar desvios e formular sugestões de rebalanceamento
        foreach (var item in activePositions)
        {
            decimal currentPrice = item.Asset.CurrentPrice;
            string symbolUpper = item.Position.AssetSymbol.ToUpperInvariant();
            
            decimal targetAllocationDecimal = normalizedTargets[symbolUpper]; 
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

            // Valor ideal que o ativo deveria ter na carteira após normalização das metas
            decimal targetValue = totalValue * targetAllocationDecimal;
            decimal differenceValue = targetValue - item.CurrentValue;

            // Se a diferença de alocação exigir uma movimentação financeira maior ou igual ao preço de 1 cota:
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
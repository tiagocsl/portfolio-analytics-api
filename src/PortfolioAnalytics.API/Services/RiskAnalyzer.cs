using PortfolioAnalytics.API.Data;
using PortfolioAnalytics.API.Models;
using PortfolioAnalytics.API.Models.DTOs;

namespace PortfolioAnalytics.API.Services;

public class RiskAnalyzer : IRiskAnalyzer
{
    private readonly IDataContext _context;

    public RiskAnalyzer(IDataContext context)
    {
        _context = context;
    }

    public RiskAnalysisResponse Analyze(Portfolio portfolio)
    {
        if (portfolio == null) throw new ArgumentNullException(nameof(portfolio));

        var response = new RiskAnalysisResponse();
        var alerts = new List<string>();

        decimal totalPortfolioValue = 0;
        var positionValues = new List<(string Symbol, string Sector, string Type, decimal CurrentValue)>();

        foreach (var position in portfolio.Positions)
        {
            var asset = _context.Assets.FirstOrDefault(a => a.Symbol == position.AssetSymbol);
            if (asset == null) continue;

            decimal assetCurrentValue = position.Quantity * asset.CurrentPrice;
            totalPortfolioValue += assetCurrentValue;

            positionValues.Add((position.AssetSymbol, asset.Sector, asset.Type, assetCurrentValue));
        }

        if (totalPortfolioValue <= 0)
        {
            response.RiskLevel = "LOW";
            response.Alerts.Add("Portfólio não possui posições ativas ou valor financeiro.");
            return response;
        }

        double hhi = 0;
        foreach (var pos in positionValues)
        {
            double weight = (double)(pos.CurrentValue / totalPortfolioValue);
            hhi += Math.Pow(weight, 2);

            if (weight > 0.20)
            {
                alerts.Add($"Concentração elevada: O ativo '{pos.Symbol}' representa {Math.Round(weight * 100, 2)}% do portfólio.");
            }
        }
        response.ConcentrationIndexHHI = Math.Round(hhi, 4);

        if (hhi < 0.15)
        {
            response.RiskLevel = "LOW";
        }
        else if (hhi <= 0.25)
        {
            response.RiskLevel = "MEDIUM";
        }
        else
        {
            response.RiskLevel = "HIGH";
        }

        var sectorGroups = positionValues
            .GroupBy(p => p.Sector)
            .Select(g => new SectorExposureDto
            {
                Sector = g.Key,
                ExposurePercentage = Math.Round((g.Sum(x => x.CurrentValue) / totalPortfolioValue) * 100, 2)
            })
            .OrderByDescending(s => s.ExposurePercentage)
            .ToList();

        response.SectorExposure = sectorGroups;

        foreach (var sector in sectorGroups)
        {
            if (sector.ExposurePercentage > 40)
            {
                alerts.Add($"Concentração setorial elevada: O setor '{sector.Sector}' representa {sector.ExposurePercentage}% do portfólio.");
            }
        }

        var typeGroups = positionValues
            .GroupBy(p => p.Type)
            .Select(g => new TypeExposureDto
            {
                Type = g.Key,
                ExposurePercentage = Math.Round((g.Sum(x => x.CurrentValue) / totalPortfolioValue) * 100, 2)
            })
            .OrderByDescending(t => t.ExposurePercentage)
            .ToList();

        response.TypeExposure = typeGroups;

        response.Alerts = alerts;

        return response;
    }
}
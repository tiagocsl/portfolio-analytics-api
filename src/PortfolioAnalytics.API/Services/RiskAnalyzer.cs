using Microsoft.Extensions.Logging;
using PortfolioAnalytics.API.Data;
using PortfolioAnalytics.API.Models;
using PortfolioAnalytics.API.Models.DTOs;

namespace PortfolioAnalytics.API.Services;

public class RiskAnalyzer : IRiskAnalyzer
{
    private readonly IDataContext _context;
    private readonly ILogger<RiskAnalyzer> _logger;

    public RiskAnalyzer(IDataContext context, ILogger<RiskAnalyzer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public RiskAnalysisResponse Analyze(Portfolio portfolio)
    {
        if (portfolio == null) throw new ArgumentNullException(nameof(portfolio));

        _logger.LogDebug("Starting risk analysis for portfolio {PortfolioId}", portfolio.Id);

        var response = new RiskAnalysisResponse();
        var alerts = new List<string>();

        decimal totalPortfolioValue = 0;
        var positionValues = new List<(string Symbol, string Sector, string Type, decimal CurrentValue)>();

        foreach (var position in portfolio.Positions)
        {
            var asset = _context.Assets.FirstOrDefault(a => a.Symbol == position.AssetSymbol);
            if (asset == null)
            {
                _logger.LogWarning("Asset not found for risk analysis position {AssetSymbol} in portfolio {PortfolioId}", position.AssetSymbol, portfolio.Id);
                continue;
            }

            decimal assetCurrentValue = position.Quantity * asset.CurrentPrice;
            totalPortfolioValue += assetCurrentValue;

            positionValues.Add((position.AssetSymbol, asset.Sector, asset.Type, assetCurrentValue));
        }

        if (totalPortfolioValue <= 0)
        {
            _logger.LogWarning("Risk analysis ended early because portfolio {PortfolioId} has no active value.", portfolio.Id);
            response.RiskLevel = "LOW";
            response.Alerts.Add("Portfólio não possui posições ativas ou valor financeiro.");
            return response;
        }

        _logger.LogDebug("Total portfolio value for risk analysis {PortfolioId} = {TotalPortfolioValue}", portfolio.Id, totalPortfolioValue);

        double hhi = 0;
        foreach (var pos in positionValues)
        {
            double weight = (double)(pos.CurrentValue / totalPortfolioValue);
            hhi += Math.Pow(weight, 2);

            _logger.LogDebug("Risk position {Symbol}: sector={Sector}, type={Type}, currentValue={CurrentValue}, weight={Weight}", pos.Symbol, pos.Sector, pos.Type, pos.CurrentValue, weight);

            if (weight > 0.20)
            {
                alerts.Add($"Concentração elevada: O ativo '{pos.Symbol}' representa {Math.Round(weight * 100, 2)}% do portfólio.");
                _logger.LogWarning("High concentration alert for asset {Symbol} at weight {Weight:P2} in portfolio {PortfolioId}", pos.Symbol, weight, portfolio.Id);
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

        _logger.LogDebug("Concentration HHI for portfolio {PortfolioId} = {HHI}, riskLevel={RiskLevel}", portfolio.Id, response.ConcentrationIndexHHI, response.RiskLevel);

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
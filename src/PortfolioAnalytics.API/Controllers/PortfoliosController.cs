using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PortfolioAnalytics.API.Data;
using PortfolioAnalytics.API.Services;
using PortfolioAnalytics.API.Services.Interfaces;
using PortfolioAnalytics.API.Models.DTOs;

namespace PortfolioAnalytics.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfoliosController : ControllerBase
{
    private readonly IDataContext _context;
    private readonly IPerformanceCalculator _performanceCalculator;
    private readonly IRiskAnalyzer _riskAnalyzer;
    private readonly IRebalancingOptimizer _rebalancingOptimizer;
    private readonly ILogger<PortfoliosController> _logger;

    public PortfoliosController(
        IDataContext context,
        IPerformanceCalculator performanceCalculator,
        IRiskAnalyzer riskAnalyzer,
        IRebalancingOptimizer rebalancingOptimizer,
        ILogger<PortfoliosController> logger)
    {
        _context = context;
        _performanceCalculator = performanceCalculator;
        _riskAnalyzer = riskAnalyzer;
        _rebalancingOptimizer = rebalancingOptimizer;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/portfolios/{id}/performance
    /// Retorna métricas de performance detalhadas do portfólio.
    /// </summary>
    [HttpGet("{id}/performance")]
    [ProducesResponseType(typeof(PerformanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetPerformance(string id)
    {
        _logger.LogDebug("GetPerformance request received for PortfolioId={PortfolioId}", id);

        // Procuramos o portfolio pelo ID (que mapeamos como o userId vindo do SeedData)
        var portfolio = _context.Portfolios.FirstOrDefault(p => p.Id == id || p.UserId == id);
        
        if (portfolio == null)
        {
            _logger.LogWarning("Portfólio não encontrado para PortfolioId={PortfolioId}", id);
            return NotFound(new { message = $"Portfólio com o ID '{id}' não foi encontrado." });
        }

        _logger.LogDebug(
            "Portfólio encontrado: PortfolioId={PortfolioId}, UserId={UserId}, PositionCount={PositionCount}",
            portfolio.Id,
            portfolio.UserId,
            portfolio.Positions?.Count ?? 0);

        var result = _performanceCalculator.Calculate(portfolio);
        _logger.LogDebug("Performance calculada para PortfolioId={PortfolioId}", portfolio.Id);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/portfolios/{id}/risk-analysis
    /// Analisa o risco de concentração, alocação setorial e classe de ativos, além de gerar alertas.
    /// </summary>
    [HttpGet("{id}/risk-analysis")]
    [ProducesResponseType(typeof(RiskAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetRiskAnalysis(string id)
    {
        _logger.LogDebug("GetRiskAnalysis request received for PortfolioId={PortfolioId}", id);

        var portfolio = _context.Portfolios.FirstOrDefault(p => p.Id == id || p.UserId == id);
        
        if (portfolio == null)
        {
            _logger.LogWarning("Portfólio não encontrado para PortfolioId={PortfolioId}", id);
            return NotFound(new { message = $"Portfólio com o ID '{id}' não foi encontrado." });
        }

        var result = _riskAnalyzer.Analyze(portfolio);
        _logger.LogDebug("Risk analysis completed for PortfolioId={PortfolioId}", portfolio.Id);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/portfolios/{id}/rebalancing
    /// Sugere compras e vendas exatas para reequilibrar o portfólio de acordo com as metas estipuladas.
    /// </summary>
    [HttpGet("{id}/rebalancing")]
    [ProducesResponseType(typeof(RebalancingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetRebalancing(string id)
    {
        _logger.LogDebug("GetRebalancing request received for PortfolioId={PortfolioId}", id);

        var portfolio = _context.Portfolios.FirstOrDefault(p => p.Id == id || p.UserId == id);
        
        if (portfolio == null)
        {
            _logger.LogWarning("Portfólio não encontrado para PortfolioId={PortfolioId}", id);
            return NotFound(new { message = $"Portfólio com o ID '{id}' não foi encontrado." });
        }

        var result = _rebalancingOptimizer.Optimize(portfolio);
        _logger.LogDebug("Rebalancing optimization completed for PortfolioId={PortfolioId}", portfolio.Id);
        return Ok(result);
    }
}
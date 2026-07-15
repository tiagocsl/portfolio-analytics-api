using PortfolioAnalytics.API.Models;
using PortfolioAnalytics.API.Models.DTOs;

namespace PortfolioAnalytics.API.Services;

public interface IRiskAnalyzer
{
    RiskAnalysisResponse Analyze(Portfolio portfolio);
}
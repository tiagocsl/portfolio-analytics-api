using PortfolioAnalytics.API.Models;
using PortfolioAnalytics.API.Models.DTOs;

namespace PortfolioAnalytics.API.Services;

public interface IRebalancingOptimizer
{
    RebalancingResponse Optimize(Portfolio portfolio);
}
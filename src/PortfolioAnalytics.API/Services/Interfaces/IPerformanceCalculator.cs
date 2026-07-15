using PortfolioAnalytics.API.Models;
using PortfolioAnalytics.API.Models.DTOs;

namespace PortfolioAnalytics.API.Services.Interfaces;

public interface IPerformanceCalculator
{
    PerformanceResponse Calculate(Portfolio portfolio);
}
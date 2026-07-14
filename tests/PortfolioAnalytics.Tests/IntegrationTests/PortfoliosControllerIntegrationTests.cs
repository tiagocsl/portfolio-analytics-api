using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PortfolioAnalytics.API.Models.DTOs;
using Xunit;

namespace PortfolioAnalytics.Tests.IntegrationTests;

public class PortfoliosControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PortfoliosControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("user-001")] 
    [InlineData("user-002")] 
    [InlineData("user-003")] 
    public async Task GetPerformance_ReturnsSuccessAndCorrectPayload(string id)
    {
        var response = await _client.GetAsync($"/api/portfolios/{id}/performance");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<PerformanceResponse>();
        Assert.NotNull(data);
        Assert.True(data.TotalInvestment > 0);
        Assert.True(data.CurrentValue > 0);
        Assert.NotEmpty(data.PositionsPerformance);
    }

    [Fact]
    public async Task GetRiskAnalysis_ReturnsSuccessAndCorrectData_ForGrowthPortfolio()
    {
        var response = await _client.GetAsync("/api/portfolios/user-002/risk-analysis");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<RiskAnalysisResponse>();
        Assert.NotNull(data);
        Assert.NotNull(data.RiskLevel);
        Assert.NotEmpty(data.SectorExposure);
    }

    [Fact]
    public async Task GetRebalancing_ReturnsSuccessAndCorrectInstructions()
    {
        var response = await _client.GetAsync("/api/portfolios/user-003/rebalancing");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<RebalancingResponse>();
        Assert.NotNull(data);
        Assert.True(data.TotalValue > 0);
        Assert.NotEmpty(data.CurrentVsTargetAllocation);
    }

    [Fact]
    public async Task GetPerformance_ReturnsNotFound_WhenPortfolioDoesNotExist()
    {
        var response = await _client.GetAsync("/api/portfolios/usuario-inexistente/performance");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
using System.Text.Json;
using PortfolioAnalytics.API.Models;
using PortfolioAnalytics.API.Models.DTOs;

namespace PortfolioAnalytics.API.Data;

public interface IDataContext
{
    List<Asset> Assets { get; }
    List<Portfolio> Portfolios { get; }
    Dictionary<string, List<PriceHistory>> PriceHistory { get; }
    decimal SelicRate { get; }
}

public class DataContext : IDataContext
{
    public List<Asset> Assets { get; private set; } = [];
    public List<Portfolio> Portfolios { get; private set; } = [];
    public Dictionary<string, List<PriceHistory>> PriceHistory { get; private set; } = [];
    public decimal SelicRate { get; private set; }

    public DataContext()
    {
        LoadSeedData();
    }

    private void LoadSeedData()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "SeedData.json");
        
        if (!File.Exists(jsonPath))
        {
            jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData.json");
        }

        if (!File.Exists(jsonPath))
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var tempPath = Path.Combine(directory.FullName, "SeedData.json");
                if (File.Exists(tempPath))
                {
                    jsonPath = tempPath;
                    break;
                }

                var apiSubPath = Path.Combine(directory.FullName, "src", "PortfolioAnalytics.API", "SeedData.json");
                if (File.Exists(apiSubPath))
                {
                    jsonPath = apiSubPath;
                    break;
                }

                directory = directory.Parent;
            }
        }

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"O arquivo SeedData.json não foi encontrado em nenhuma das tentativas. Último caminho verificado: {jsonPath}");
        }

        var jsonString = File.ReadAllText(jsonPath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var wrapper = JsonSerializer.Deserialize<SeedDataWrapper>(jsonString, options);

        if (wrapper != null)
        {
            Assets = wrapper.Assets;
            Portfolios = wrapper.Portfolios;
            PriceHistory = wrapper.PriceHistory;
            SelicRate = wrapper.MarketData.SelicRate;

            foreach (var portfolio in Portfolios)
            {
                if (string.IsNullOrEmpty(portfolio.Id))
                {
                    portfolio.Id = portfolio.UserId;
                }
            }
        }
    }
}
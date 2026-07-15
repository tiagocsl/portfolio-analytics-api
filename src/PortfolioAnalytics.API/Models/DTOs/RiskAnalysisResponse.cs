namespace PortfolioAnalytics.API.Models.DTOs;

public class RiskAnalysisResponse
{
    public double ConcentrationIndexHHI { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<SectorExposureDto> SectorExposure { get; set; } = [];
    public List<TypeExposureDto> TypeExposure { get; set; } = [];
    public List<string> Alerts { get; set; } = [];
}

public class SectorExposureDto
{
    public string Sector { get; set; } = string.Empty;
    public decimal ExposurePercentage { get; set; }
}

public class TypeExposureDto
{
    public string Type { get; set; } = string.Empty;
    public decimal ExposurePercentage { get; set; }
}
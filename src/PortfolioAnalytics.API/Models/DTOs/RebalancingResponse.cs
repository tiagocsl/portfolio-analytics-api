namespace PortfolioAnalytics.API.Models.DTOs;

public class RebalancingResponse
{
    public decimal TotalValue { get; set; }
    public List<RebalancingInstructionDto> Instructions { get; set; } = [];
    public List<AssetAllocationDto> CurrentVsTargetAllocation { get; set; } = [];
}

public class RebalancingInstructionDto
{
    public string AssetSymbol { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal EstimatedCost { get; set; }
}

public class AssetAllocationDto
{
    public string AssetSymbol { get; set; } = string.Empty;
    public decimal CurrentAllocation { get; set; }
    public decimal TargetAllocation { get; set; }
    public decimal Deviation { get; set; }
}
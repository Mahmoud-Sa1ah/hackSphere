using System.Text.Json.Serialization;

namespace PentestHub.API.Data.DTOs;

public class AIResponseDto
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("next_steps")]
    public string NextSteps { get; set; } = string.Empty;

    [JsonPropertyName("risk_score")]
    public int RiskScore { get; set; }

    [JsonPropertyName("attack_path")]
    public string AttackPath { get; set; } = string.Empty;

    [JsonPropertyName("recommendations")]
    public string Recommendations { get; set; } = string.Empty;

    [JsonPropertyName("vulnerability_count")]
    public int VulnerabilityCount { get; set; }
}


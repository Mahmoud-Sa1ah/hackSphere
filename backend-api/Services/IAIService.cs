using PentestHub.API.Data.DTOs;

namespace PentestHub.API.Services;

public interface IAIService
{
    Task<AIResponseDto> AnalyzeScanOutputAsync(AIAnalyzeDto analyzeDto);
    Task<string> GenerateLabFeedbackAsync(string details, string labTitle, string labDescription);
    Task<string> ChatAsync(ChatDto dto, int userId);
}


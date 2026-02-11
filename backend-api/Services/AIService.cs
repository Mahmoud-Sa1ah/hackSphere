using Microsoft.EntityFrameworkCore;
using PentestHub.API.Data;
using PentestHub.API.Data.DTOs;
using PentestHub.API.Data.Models;
using System.Text;
using System.Text.Json;

namespace PentestHub.API.Services;

public class AIService : IAIService
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;

    public AIService(IConfiguration configuration, ApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
        _httpClient = new HttpClient();
    }

    public async Task<AIResponseDto> AnalyzeScanOutputAsync(AIAnalyzeDto analyzeDto)
    {
        var apiKey = _configuration["AI:ApiKey"];
        var apiUrl = $"{_configuration["AI:ApiUrl"]}?key={apiKey}";

        var prompt = $@"Analyze the following penetration testing scan output and provide a comprehensive security assessment.

Tool: {analyzeDto.Tool}
Target: {analyzeDto.Target}
Output:
{analyzeDto.RawOutput}

Please provide:
1. A concise summary of findings. If everything looks secure, explicitly mention that ""Everything looks good"".
2. Recommended next steps. If no actions are needed, state ""No immediate action required, your system appears secure"". If any dangerous, suspicious, or ""wired"" output is found, provide detailed remediation steps.
3. A risk score (0-100)
4. Potential attack paths
5. Remediation recommendations

6. Total number of vulnerabilities found (vulnerability_count)

Format your response EXACTLY as a JSON object with these keys: summary, next_steps, risk_score, attack_path, recommendations, vulnerability_count. 
Do not include any other text or markdown blocks, just the raw JSON.";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        try
        {
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = JsonDocument.Parse(responseContent);
                var aiMessage = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (!string.IsNullOrEmpty(aiMessage))
                {
                    // Clean markdown if AI includes it
                    aiMessage = aiMessage.Replace("```json", "").Replace("```", "").Trim();
                    
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var aiResponse = JsonSerializer.Deserialize<AIResponseDto>(aiMessage, options);
                        if (aiResponse != null)
                        {
                            return aiResponse;
                        }
                    }
                    catch
                    {
                        // Fallback if JSON parsing fails
                    }
                }
            }
        }
        catch
        {
            // Fallback
        }

        return new AIResponseDto
        {
            Summary = "AI analysis service is currently unavailable. Please review the raw output manually.",
            NextSteps = "1. Review the scan output carefully\n2. Identify potential vulnerabilities\n3. Plan remediation steps",
            RiskScore = 50,
            AttackPath = "Review scan output to identify attack vectors",
            Recommendations = "Ensure all systems are patched and properly configured",
            VulnerabilityCount = 0
        };
    }

    public async Task<string> GenerateLabFeedbackAsync(string details, string labTitle, string labDescription)
    {
        var apiKey = _configuration["AI:ApiKey"];
        var apiUrl = $"{_configuration["AI:ApiUrl"]}?key={apiKey}";

        var prompt = $@"Provide feedback on this lab submission:

Lab: {labTitle}
Description: {labDescription}
Submission Details: {details}

Provide constructive feedback on the submission, highlighting strengths and areas for improvement.";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        try
        {
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = JsonDocument.Parse(responseContent);
                var feedback = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return feedback ?? "Feedback generation unavailable.";
            }
        }
        catch
        {
            // Fallback
        }

        return "AI feedback service is currently unavailable. Your submission has been recorded.";
    }

    public async Task<string> ChatAsync(ChatDto dto, int userId)
    {
        var apiKey = _configuration["AI:ApiKey"];
        var apiUrl = $"{_configuration["AI:ApiUrl"]}?key={apiKey}";

        // Get conversation history
        var recentMessages = await _context.AIConversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        // AI System Rules and Instructions
        var systemPrompt = @"You are a cybersecurity AI assistant for CyberToolkit Security Testing Platform. Your role is to help users with:

1. **Security Testing Guidance:**
   - Explain how to use security tools effectively
   - Interpret scan results and identify vulnerabilities
   - Provide remediation recommendations
   - Suggest next steps in penetration testing workflows

2. **Tool Assistance:**
   - Help users understand tool parameters and arguments
   - Troubleshoot tool execution issues
   - Recommend appropriate tools for specific tasks

3. **Security Best Practices:**
   - Provide secure coding practices
   - Explain common vulnerabilities (OWASP Top 10, etc.)
   - Guide on responsible disclosure

4. **Rules:**
   - Always prioritize security and ethical hacking practices
   - Never provide instructions for illegal activities
   - Encourage responsible security testing
   - Focus on defensive security and remediation
   - Be concise but thorough in explanations

5. **Response Format:**
   - Use clear, structured responses
   - Provide actionable advice
   - Include relevant examples when helpful
   - Reference security standards when appropriate";

        var contents = new List<object>();
        
        contents.Add(new { role = "user", parts = new[] { new { text = "SYSTEM INSTRUCTIONS: " + systemPrompt } } });
        contents.Add(new { role = "model", parts = new[] { new { text = "I understand my role and rules. How can I assist you today?" } } });

        foreach (var msg in recentMessages)
        {
            contents.Add(new { role = "user", parts = new[] { new { text = msg.MessageText } } });
            if (!string.IsNullOrEmpty(msg.ResponseText))
            {
                contents.Add(new { role = "model", parts = new[] { new { text = msg.ResponseText } } });
            }
        }
        
        var messageParts = new List<object>();
        messageParts.Add(new { text = dto.Message });
        
        if (!string.IsNullOrEmpty(dto.FileData))
        {
            // Inline data for Gemini
            messageParts.Add(new 
            { 
                inline_data = new 
                { 
                    mime_type = dto.FileType ?? "application/octet-stream",
                    data = dto.FileData 
                } 
            });
        }
        
        contents.Add(new { role = "user", parts = messageParts.ToArray() });

        var requestBody = new
        {
            contents = contents
        };

        try
        {
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            string aiResponse = "I'm sorry, I'm having trouble processing your request right now.";

            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = JsonDocument.Parse(responseContent);
                aiResponse = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? aiResponse;
            }
            else
            {
                Console.WriteLine($"[AI ERROR] Gemini API returned {response.StatusCode}: {responseContent}");
            }

            // Save conversation
            var conversation = new AIConversation
            {
                UserId = userId,
                MessageText = dto.Message,
                ResponseText = aiResponse,
                CreatedAt = DateTime.UtcNow
            };

            _context.AIConversations.Add(conversation);
            await _context.SaveChangesAsync();

            return aiResponse;
        }
        catch
        {
            return "AI chat service is currently unavailable. Please try again later.";
        }
    }
}


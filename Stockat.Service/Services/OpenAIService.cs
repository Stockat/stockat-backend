using Microsoft.Extensions.Configuration;
using Stockat.Core;
using Stockat.Core.DTOs.ChatBotDTOs;
using Stockat.Core.Helpers;
using Stockat.Core.IServices;
using System.Text;
using System.Text.Json;

namespace Stockat.Service.Services;

public class OpenAIService : IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILoggerManager _logger;
    private readonly OpenAIConfigs _openAIConfigs;
    private readonly string _baseUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAIService(IConfiguration configuration, ILoggerManager logger, HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
        
        // Manual configuration binding instead of using Bind method
        _openAIConfigs = new OpenAIConfigs
        {
            ApiKey = configuration["OpenAI:ApiKey"] ?? "",
            Model = configuration["OpenAI:Model"] ?? "gpt-3.5-turbo",
            MaxTokens = int.TryParse(configuration["OpenAI:MaxTokens"], out var maxTokens) ? maxTokens : 800,
            Temperature = double.TryParse(configuration["OpenAI:Temperature"], out var temperature) ? temperature : 0.7
        };
        
        if (string.IsNullOrEmpty(_openAIConfigs.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key not configured. Please add your API key to appsettings.json");
        }
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAIConfigs.ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Stockat-ChatBot/1.0");
    }

    public async Task<string> GenerateResponseAsync(string userMessage, object contextData)
    {
        try
        {
            _logger.LogInfo($"OpenAI: Starting API call for message: {userMessage}");
            
            var systemPrompt = CreateSystemPrompt(contextData);
            _logger.LogInfo($"OpenAI: System prompt created, length: {systemPrompt.Length}");
            
            var requestBody = new
            {
                model = _openAIConfigs.Model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage }
                },
                max_tokens = _openAIConfigs.MaxTokens,
                temperature = _openAIConfigs.Temperature,
                top_p = 1,
                frequency_penalty = 0,
                presence_penalty = 0
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            _logger.LogInfo($"OpenAI: Request body serialized, length: {jsonContent.Length}");
            
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogInfo($"OpenAI: Making HTTP request to {_baseUrl}");
            var response = await _httpClient.PostAsync(_baseUrl, content);
            
            _logger.LogInfo($"OpenAI: Received response with status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"OpenAI API error: {response.StatusCode} - {errorContent}");
                throw new Exception($"OpenAI API error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInfo($"OpenAI: Response content received, length: {responseContent.Length}");
            _logger.LogInfo($"OpenAI: Raw response: {responseContent}");
            
            var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);
            _logger.LogInfo($"OpenAI: Deserialized response: {openAIResponse != null}");
            
            if (openAIResponse != null)
            {
                _logger.LogInfo($"OpenAI: Choices count: {openAIResponse.Choices?.Count ?? 0}");
                if (openAIResponse.Choices?.Any() == true)
                {
                    var firstChoice = openAIResponse.Choices.First();
                    _logger.LogInfo($"OpenAI: First choice message: {firstChoice.Message?.Content ?? "NULL"}");
                }
            }
            
            var result = openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content;
            
            // Fallback: try to parse manually if structured approach fails
            if (string.IsNullOrEmpty(result))
            {
                _logger.LogInfo("OpenAI: Structured parsing failed, trying manual JSON parsing");
                try
                {
                    var jsonDoc = JsonDocument.Parse(responseContent);
                    var choices = jsonDoc.RootElement.GetProperty("choices");
                    if (choices.GetArrayLength() > 0)
                    {
                        var firstChoice = choices[0];
                        var message = firstChoice.GetProperty("message");
                        result = message.GetProperty("content").GetString();
                        _logger.LogInfo($"OpenAI: Manual parsing successful: {result}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"OpenAI: Manual parsing also failed: {ex.Message}");
                }
            }
            
            if (string.IsNullOrEmpty(result))
            {
                result = "I couldn't generate a response. Please try again.";
            }
            
            _logger.LogInfo($"OpenAI: Final result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calling OpenAI API: {ex.Message}");
            _logger.LogError($"OpenAI API call failed: {ex}");
            throw;
        }
    }

    public async Task<string> GenerateResponseWithHistoryAsync(string userMessage, IEnumerable<ChatBotMessageDto> chatHistory, object contextData)
    {
        try
        {
            var systemPrompt = CreateSystemPrompt(contextData);
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            // Add chat history (last 10 messages to avoid token limits)
            var recentHistory = chatHistory.TakeLast(10);
            foreach (var message in recentHistory)
            {
                messages.Add(new { role = message.Role, content = message.MessageText });
            }

            // Add current user message
            messages.Add(new { role = "user", content = userMessage });

            var requestBody = new
            {
                model = _openAIConfigs.Model,
                messages = messages,
                max_tokens = _openAIConfigs.MaxTokens,
                temperature = _openAIConfigs.Temperature,
                top_p = 1,
                frequency_penalty = 0,
                presence_penalty = 0
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_baseUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"OpenAI API error: {response.StatusCode} - {errorContent}");
                throw new Exception($"OpenAI API error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);
            
            return openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? 
                   "I couldn't generate a response. Please try again.";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calling OpenAI API: {ex.Message}");
            throw;
        }
    }

    public async Task<string> GenerateResponseWithModelAsync(string userMessage, object contextData, string model)
    {
        try
        {
            _logger.LogInfo($"OpenAI: Testing model {model} for message: {userMessage}");
            
            var systemPrompt = CreateSystemPrompt(contextData);
            
            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage }
                },
                max_tokens = _openAIConfigs.MaxTokens,
                temperature = _openAIConfigs.Temperature,
                top_p = 1,
                frequency_penalty = 0,
                presence_penalty = 0
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_baseUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"OpenAI API error with model {model}: {response.StatusCode} - {errorContent}");
                throw new Exception($"OpenAI API error with model {model}: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);
            
            var result = openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? 
                        "I couldn't generate a response. Please try again.";
            
            _logger.LogInfo($"OpenAI: Model {model} generated response: {result}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calling OpenAI API with model {model}: {ex.Message}");
            throw;
        }
    }

    private string CreateSystemPrompt(object contextData)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine("You are Stockat AI, an intelligent assistant for the Stockat B2B manufacturing platform. Your role is to help users find information about products, services, sellers, auctions, and more.");
        prompt.AppendLine();
        prompt.AppendLine("Key capabilities:");
        prompt.AppendLine("- Provide information about top sellers and their performance");
        prompt.AppendLine("- Share details about popular products and trending items");
        prompt.AppendLine("- Inform about live auctions and bidding opportunities");
        prompt.AppendLine("- Help with service information and categories");
        prompt.AppendLine("- Offer platform statistics and analytics");
        prompt.AppendLine("- Provide general help and guidance");
        prompt.AppendLine();
        prompt.AppendLine("Guidelines:");
        prompt.AppendLine("- Be helpful, professional, and friendly");
        prompt.AppendLine("- Provide accurate information based on platform data");
        prompt.AppendLine("- If you don't have specific data, acknowledge it and offer alternatives");
        prompt.AppendLine("- Keep responses concise but informative");
        prompt.AppendLine("- Use emojis sparingly to make responses engaging");
        prompt.AppendLine("- Always maintain a helpful and supportive tone");
        prompt.AppendLine();
        prompt.AppendLine("Context: You're assisting users on a B2B manufacturing platform where businesses can buy, sell, and auction products and services.");
        prompt.AppendLine();
        
        // Include actual platform data in the system prompt
        try
        {
            _logger.LogInfo($"CreateSystemPrompt: Processing context data of type: {contextData?.GetType().Name}");
            
            var contextJson = JsonSerializer.Serialize(contextData);
            _logger.LogInfo($"CreateSystemPrompt: Context JSON length: {contextJson.Length}");
            
            var contextDict = JsonSerializer.Deserialize<Dictionary<string, object>>(contextJson);
            
            if (contextDict != null && contextDict.ContainsKey("platformData"))
            {
                var platformDataJson = JsonSerializer.Serialize(contextDict["platformData"]);
                _logger.LogInfo($"CreateSystemPrompt: Platform data JSON length: {platformDataJson.Length}");
                
                var platformData = JsonSerializer.Deserialize<Dictionary<string, object>>(platformDataJson);
                
                if (platformData != null)
                {
                    prompt.AppendLine("Current Platform Data:");
                    
                    // Add top sellers information
                    if (platformData.ContainsKey("topSellers"))
                    {
                        var topSellersJson = JsonSerializer.Serialize(platformData["topSellers"]);
                        prompt.AppendLine($"Top Sellers: {topSellersJson}");
                        _logger.LogInfo($"CreateSystemPrompt: Added {topSellersJson.Length} chars of top sellers data");
                    }
                    
                    // Add top products information
                    if (platformData.ContainsKey("topProducts"))
                    {
                        var topProductsJson = JsonSerializer.Serialize(platformData["topProducts"]);
                        prompt.AppendLine($"Top Products: {topProductsJson}");
                        _logger.LogInfo($"CreateSystemPrompt: Added {topProductsJson.Length} chars of top products data");
                    }
                    
                    // Add live auctions information
                    if (platformData.ContainsKey("liveAuctions"))
                    {
                        var liveAuctionsJson = JsonSerializer.Serialize(platformData["liveAuctions"]);
                        prompt.AppendLine($"Live Auctions: {liveAuctionsJson}");
                        _logger.LogInfo($"CreateSystemPrompt: Added {liveAuctionsJson.Length} chars of live auctions data");
                    }
                    
                    // Add top services information
                    if (platformData.ContainsKey("topServices"))
                    {
                        var topServicesJson = JsonSerializer.Serialize(platformData["topServices"]);
                        prompt.AppendLine($"Top Services: {topServicesJson}");
                        _logger.LogInfo($"CreateSystemPrompt: Added {topServicesJson.Length} chars of top services data");
                    }
                    
                    // Add categories information
                    if (platformData.ContainsKey("categories"))
                    {
                        var categoriesJson = JsonSerializer.Serialize(platformData["categories"]);
                        prompt.AppendLine($"Categories: {categoriesJson}");
                        _logger.LogInfo($"CreateSystemPrompt: Added {categoriesJson.Length} chars of categories data");
                    }
                    
                    // Add platform statistics
                    if (platformData.ContainsKey("platformStats"))
                    {
                        var platformStatsJson = JsonSerializer.Serialize(platformData["platformStats"]);
                        prompt.AppendLine($"Platform Statistics: {platformStatsJson}");
                        _logger.LogInfo($"CreateSystemPrompt: Added {platformStatsJson.Length} chars of platform stats data");
                    }
                    
                    prompt.AppendLine();
                    prompt.AppendLine("Use this real platform data to provide accurate and specific information to users. When users ask about sellers, products, auctions, services, or statistics, reference this actual data.");
                    
                    _logger.LogInfo($"CreateSystemPrompt: Final prompt length: {prompt.Length}");
                }
                else
                {
                    _logger.LogWarn("CreateSystemPrompt: Platform data is null after deserialization");
                }
            }
            else
            {
                _logger.LogWarn("CreateSystemPrompt: No platformData found in context or context is null");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarn($"Error parsing platform data for system prompt: {ex.Message}");
            prompt.AppendLine("Note: Platform data is currently unavailable. Provide general information about the platform capabilities.");
        }

        return prompt.ToString();
    }

    // OpenAI API response models
    private class OpenAIResponse
    {
        public List<Choice> Choices { get; set; } = new();
        public string? Id { get; set; }
        public string? Object { get; set; }
        public long? Created { get; set; }
        public string? Model { get; set; }
        public Usage? Usage { get; set; }
    }

    private class Choice
    {
        public Message Message { get; set; } = new();
        public string? FinishReason { get; set; }
        public int? Index { get; set; }
    }

    private class Message
    {
        public string Content { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    private class Usage
    {
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
        public int? TotalTokens { get; set; }
    }
} 
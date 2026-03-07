using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Pausalio.Application.DTOs.AIAssistant;
using Pausalio.Application.Helpers;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Implementations
{
    public class AIAssistantService : IAIAssistantService
    {
        private readonly IFinancialContextService _financialContextService;
        private readonly IOptions<OpenRouterSettings> _configuration;
        private readonly HttpClient _httpClient;

        public AIAssistantService(
            IFinancialContextService financialContextService,
            IOptions<OpenRouterSettings> configuration,
            HttpClient httpClient)
        {
            _financialContextService = financialContextService;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<AIResponseDto> SendMessageAsync(UserChatMessage message)
        {
            var financialContext = await _financialContextService.BuildContextAsync();

            var systemPrompt = AIAssistantPromptHelper.BuildSystemPrompt(financialContext);

            var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

            foreach (var item in message.History)
            {
                messages.Add(new { role = item.Role, content = item.Content });
            }

            messages.Add(new { role = "user", content = message.Message });

            var requestBody = new
            {
                model = _configuration.Value.Model,
                messages,
                max_tokens = 1000,
                temperature = 0.7
            };

            var apiKey = _configuration.Value.ApiKey;
            var apiUrl = _configuration.Value.ApiUrl;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JsonDocument.Parse(responseString);

            var aiMessage = responseJson
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "Nije moguće dobiti odgovor.";

            return new AIResponseDto { Message = aiMessage };
        }
    }
}

using Microsoft.Extensions.Options;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Implementations.AIAssistant
{
    public class OpenRouterClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<OpenRouterSettings> _configuration;
        private readonly IEncryptionService _encryption;

        public OpenRouterClientService(
            HttpClient httpClient,
            IOptions<OpenRouterSettings> configuration,
            IEncryptionService encryption)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _encryption = encryption;
        }

        public async Task<HttpResponseMessage> SendRequestAsync(
            string encryptedApiKey,
            string model,
            IEnumerable<object> messages,
            IEnumerable<object> tools,
            bool stream,
            CancellationToken cancellationToken = default,
            double temperature = 0.2
            )
        {
            var requestBody = new
            {
                model = model,
                messages = messages,
                tools = tools,
                tool_choice = "auto",
                max_tokens = 1000,
                temperature = temperature,
                stream = stream
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, _configuration.Value.ApiUrl)
            {
                Content = content
            };

            var decryptedApiKey = _encryption.Decrypt(encryptedApiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", decryptedApiKey);

            var completionOption = stream ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead;

            return await _httpClient.SendAsync(request, completionOption, cancellationToken);
        }
    }
}

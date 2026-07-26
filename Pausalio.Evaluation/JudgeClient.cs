using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pausalio.Evaluation.Models;

namespace Pausalio.Evaluation
{
    public class JudgeClient
    {
        private readonly HttpClient _httpClient;
        private readonly EvaluationSettings _settings;
        private readonly ILogger<JudgeClient> _logger;

        private const string ResultsFilePath = "results.jsonl";

        public JudgeClient(
            HttpClient httpClient,
            IOptions<EvaluationSettings> settings,
            ILogger<JudgeClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task EvaluateResultAsync(EvalResult result)
        {
            if (result.Ignored)
            {
                _logger.LogInformation(
                    "Skipping judge call for ignored/failed question ID {Id}",
                    result.QuestionId);
                return;
            }

            _logger.LogInformation(
                "Calling judge for question ID {Id}...",
                result.QuestionId);

            string toolResults = string.Join("\n\n",
                result.ActualToolCalls.Select(tc =>
                    $"[Alat: {tc.ToolName}, Status: {(tc.Success ? "Uspešno" : "Greška")}]\n" +
                    $"Parametri: {tc.Arguments}\n" +
                    $"Rezultat: {tc.Result}"));

            string prompt = $@"Pitanje korisnika: {result.Question}
Rezultat iz alata (stvarni podaci): {toolResults}
Odgovor asistenta: {result.AssistantResponse}

Oceni odgovor asistenta prema sledećim kriterijumima. Prvo napiši kratko obrazloženje, zatim vrati ISKLJUČIVO JSON u ovom formatu:

{{
  ""reasoning"": ""..."",
  ""accuracy"": 0-5,
  ""completeness"": 0-2,
  ""clarity"": 0-3
}}

accuracy = da li se odgovor slaže sa stvarnim podacima iz alata (bez izmišljenih brojeva)
completeness = da li odgovor pokriva sve delove pitanja
clarity = jasnoća i razumljivost odgovora na srpskom jeziku";

            try
            {
                var requestBody = new
                {
                    model = _settings.JudgeModel,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.1
                };

                var requestJson = JsonSerializer.Serialize(requestBody);

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    _settings.OpenRouterApiUrl)
                {
                    Content = new StringContent(
                        requestJson,
                        Encoding.UTF8,
                        "application/json")
                };

                if (!string.IsNullOrWhiteSpace(_settings.OpenRouterApiKey))
                {
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            _settings.OpenRouterApiKey);
                }

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                using var responseDoc = JsonDocument.Parse(responseString);

                var choice = responseDoc.RootElement
                    .GetProperty("choices")[0];

                var messageContent = choice
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;

                var (reasoning, accuracy, completeness, clarity) =
                    ParseJudgeResponse(messageContent);

                result.JudgeReasoning = reasoning;
                result.JudgeAccuracy = accuracy;
                result.JudgeCompleteness = completeness;
                result.JudgeClarity = clarity;

                _logger.LogInformation(
                    "Judge evaluation completed for question ID {Id}. Accuracy: {Acc}/5, Completeness: {Comp}/2, Clarity: {Clar}/3",
                    result.QuestionId,
                    accuracy,
                    completeness,
                    clarity);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to run judge evaluation for question ID {Id}",
                    result.QuestionId);

                result.JudgeReasoning =
                    $"Greška prilikom pozivanja judge modela: {ex.Message}";

                result.JudgeAccuracy = 0;
                result.JudgeCompleteness = 0;
                result.JudgeClarity = 0;
            }
        }

        private (string reasoning, int accuracy, int completeness, int clarity)
            ParseJudgeResponse(string rawContent)
        {
            var content = rawContent.Trim();

            if (content.StartsWith("```"))
            {
                int firstNewLine = content.IndexOf('\n');

                if (firstNewLine != -1)
                {
                    content = content.Substring(firstNewLine + 1);
                }
                else
                {
                    content = content.Substring(3);
                }

                if (content.EndsWith("```"))
                {
                    content = content.Substring(0, content.Length - 3);
                }

                content = content.Trim();
            }

            int jsonStart = content.IndexOf('{');
            int jsonEnd = content.LastIndexOf('}');

            if (jsonStart != -1 && jsonEnd != -1 && jsonEnd > jsonStart)
            {
                content = content.Substring(
                    jsonStart,
                    jsonEnd - jsonStart + 1);
            }

            try
            {
                using var doc = JsonDocument.Parse(content);

                var root = doc.RootElement;

                string reasoning =
                    root.TryGetProperty("reasoning", out var reasoningProp)
                        ? reasoningProp.GetString() ?? string.Empty
                        : string.Empty;

                int accuracy =
                    root.TryGetProperty("accuracy", out var accProp)
                        ? accProp.ValueKind == JsonValueKind.Number
                            ? accProp.GetInt32()
                            : int.TryParse(
                                accProp.GetString(),
                                out var tempAcc)
                                ? tempAcc
                                : 0
                        : 0;

                int completeness =
                    root.TryGetProperty("completeness", out var compProp)
                        ? compProp.ValueKind == JsonValueKind.Number
                            ? compProp.GetInt32()
                            : int.TryParse(
                                compProp.GetString(),
                                out var tempComp)
                                ? tempComp
                                : 0
                        : 0;

                int clarity =
                    root.TryGetProperty("clarity", out var clarProp)
                        ? clarProp.ValueKind == JsonValueKind.Number
                            ? clarProp.GetInt32()
                            : int.TryParse(
                                clarProp.GetString(),
                                out var tempClar)
                                ? tempClar
                                : 0
                        : 0;

                return (reasoning, accuracy, completeness, clarity);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to parse judge JSON. Content was: \"{Content}\"",
                    rawContent);

                return (
                    $"Neuspešno parsiranje odgovora sudije: {ex.Message}. Originalni odgovor: {rawContent}",
                    0,
                    0,
                    0);
            }
        }

        public async Task EvaluateResultsAsync(List<EvalResult> results)
        {
            foreach (var result in results)
            {
                // Ako je već ocenjen, preskoči
                if (IsAlreadyJudged(result))
                {
                    _logger.LogInformation(
                        "Skipping already judged question ID {Id}",
                        result.QuestionId);

                    continue;
                }

                try
                {
                    await EvaluateResultAsync(result);

                    SaveProgress(results);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected judge error for question ID {Id}. Continuing...",
                        result.QuestionId);

                    result.JudgeReasoning =
                        $"Neočekivana judge greška: {ex.Message}";

                    SaveProgress(results);
                }
            }
        }

        private bool IsAlreadyJudged(EvalResult result)
        {
            return result.JudgeAccuracy > 0 ||
                   result.JudgeCompleteness > 0 ||
                   result.JudgeClarity > 0;
        }

        private void SaveProgress(List<EvalResult> results)
        {
            File.WriteAllText(
                ResultsFilePath,
                string.Empty);

            foreach (var result in results)
            {
                var json = JsonSerializer.Serialize(result);

                File.AppendAllText(
                    ResultsFilePath,
                    json + Environment.NewLine);
            }
        }
    }
}
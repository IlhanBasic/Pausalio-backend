using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Pausalio.Evaluation.Models;

namespace Pausalio.Evaluation
{
    public static class MetricsCalculator
    {
        public static void Calculate(EvalResult result)
        {
            if (result.Ignored)
            {
                result.IsToolCallMatch = false;
                result.ParameterAccuracyScore = 0.0;
                return;
            }

            // Level 1: Tool Selection Match (exact set match)
            var expectedSet = new HashSet<string>(result.ExpectedTools);
            var actualSet = new HashSet<string>(result.ActualToolCalls.Select(tc => tc.ToolName));

            result.IsToolCallMatch = expectedSet.SetEquals(actualSet);

            // Level 2: Parameter Accuracy
            if (result.ExpectedParameters == null || result.ExpectedParameters.Count == 0)
            {
                result.ParameterAccuracyScore = 1.0; // No expected parameters means perfect accuracy by default
                return;
            }

            // Merge all actual tool call argument parameters
            var actualParams = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            foreach (var tc in result.ActualToolCalls)
            {
                if (string.IsNullOrWhiteSpace(tc.Arguments)) continue;
                try
                {
                    using var doc = JsonDocument.Parse(tc.Arguments);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        actualParams[prop.Name] = prop.Value.Clone();
                    }
                }
                catch
                {
                    // Ignore malformed tool call arguments
                }
            }

            int matchCount = 0;
            int totalExpected = result.ExpectedParameters.Count;

            foreach (var kvp in result.ExpectedParameters)
            {
                var expectedKey = kvp.Key;
                var expectedValElement = AsJsonElement(kvp.Value);

                if (actualParams.TryGetValue(expectedKey, out var actualValElement))
                {
                    if (CompareJsonElements(expectedValElement, actualValElement))
                    {
                        matchCount++;
                    }
                }
            }

            result.ParameterAccuracyScore = (double)matchCount / totalExpected;
        }

        private static JsonElement AsJsonElement(object val)
        {
            if (val is JsonElement je) return je;
            var json = JsonSerializer.Serialize(val);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }

        private static bool CompareJsonElements(JsonElement expected, JsonElement actual)
        {
            if (expected.ValueKind == JsonValueKind.Number)
            {
                if (actual.ValueKind != JsonValueKind.Number)
                {
                    if (actual.ValueKind == JsonValueKind.String && double.TryParse(actual.GetString(), out var dActual))
                    {
                        return Math.Abs(expected.GetDouble() - dActual) < 0.0001;
                    }
                    return false;
                }
                return Math.Abs(expected.GetDouble() - actual.GetDouble()) < 0.0001;
            }

            if (expected.ValueKind == JsonValueKind.String)
            {
                var expectedStr = expected.GetString() ?? string.Empty;
                var actualStr = actual.ValueKind == JsonValueKind.String
                    ? actual.GetString() ?? string.Empty
                    : actual.GetRawText();

                return string.Equals(expectedStr.Trim(), actualStr.Trim(), StringComparison.OrdinalIgnoreCase);
            }

            if (expected.ValueKind == JsonValueKind.True || expected.ValueKind == JsonValueKind.False)
            {
                if (actual.ValueKind != JsonValueKind.True && actual.ValueKind != JsonValueKind.False)
                {
                    if (actual.ValueKind == JsonValueKind.String && bool.TryParse(actual.GetString(), out var bActual))
                    {
                        return expected.GetBoolean() == bActual;
                    }
                    return false;
                }
                return expected.GetBoolean() == actual.GetBoolean();
            }

            if (expected.ValueKind == JsonValueKind.Null)
            {
                return actual.ValueKind == JsonValueKind.Null;
            }

            return expected.GetRawText() == actual.GetRawText();
        }

        public class ToolMetrics
        {
            public string ToolName { get; set; } = string.Empty;
            public int TruePositives { get; set; }
            public int FalsePositives { get; set; }
            public int FalseNegatives { get; set; }
            public int TrueNegatives { get; set; }

            public double Precision => (TruePositives + FalsePositives) > 0 
                ? (double)TruePositives / (TruePositives + FalsePositives) 
                : 1.0;

            public double Recall => (TruePositives + FalseNegatives) > 0 
                ? (double)TruePositives / (TruePositives + FalseNegatives) 
                : 1.0;

            public double F1 => (Precision + Recall) > 0 
                ? 2 * (Precision * Recall) / (Precision + Recall) 
                : 0.0;

            public double Accuracy => (TruePositives + TrueNegatives + FalsePositives + FalseNegatives) > 0
                ? (double)(TruePositives + TrueNegatives) / (TruePositives + TrueNegatives + FalsePositives + FalseNegatives)
                : 1.0;
        }

        public static Dictionary<string, ToolMetrics> CalculateAggregateToolMetrics(List<EvalResult> results, IEnumerable<string> allKnownTools)
        {
            var metrics = allKnownTools.ToDictionary(t => t, t => new ToolMetrics { ToolName = t });

            foreach (var result in results)
            {
                if (result.Ignored) continue;

                var expectedSet = new HashSet<string>(result.ExpectedTools);
                var actualSet = new HashSet<string>(result.ActualToolCalls.Select(tc => tc.ToolName));

                // Combine all tools appeared in this test case + all known tools
                var toolsToEvaluate = expectedSet.Union(actualSet).Union(allKnownTools);

                foreach (var tool in toolsToEvaluate)
                {
                    if (!metrics.ContainsKey(tool))
                    {
                        metrics[tool] = new ToolMetrics { ToolName = tool };
                    }

                    bool expected = expectedSet.Contains(tool);
                    bool actual = actualSet.Contains(tool);

                    if (expected && actual)
                        metrics[tool].TruePositives++;
                    else if (!expected && actual)
                        metrics[tool].FalsePositives++;
                    else if (expected && !actual)
                        metrics[tool].FalseNegatives++;
                    else
                        metrics[tool].TrueNegatives++;
                }
            }

            return metrics;
        }
    }
}

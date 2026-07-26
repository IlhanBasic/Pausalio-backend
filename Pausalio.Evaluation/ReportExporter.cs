using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Pausalio.Evaluation.Models;

namespace Pausalio.Evaluation
{
    public static class ReportExporter
    {
        private const string DetailedCsvPath = "results-detailed.csv";
        private const string SummaryJsonPath = "results-summary.json";
        private const string TemperatureComparisonCsvPath = "results-temperature-comparison.csv";
        private const string TemperatureComparisonJsonPath = "results-temperature-comparison.json";
        public static void ExportTemperatureComparison(List<EvalResult> allResults)
        {
            var byTemperature = allResults
                .GroupBy(r => r.Temperature)
                .OrderBy(g => g.Key)
                .Select(g => new TemperatureSummaryResult
                {
                    Temperature = g.Key,
                    TotalQuestions = g.Count(),
                    ToolMatchRate = g.Any() ? (double)g.Count(r => r.IsToolCallMatch) / g.Count() : 0.0,
                    AvgParameterAccuracy = g.Any() ? g.Average(r => r.ParameterAccuracyScore) : 0.0,
                    AvgDurationMs = g.Any() ? g.Average(r => r.ExecutionDurationMs) : 0.0,
                    TotalPromptTokens = g.Sum(r => r.PromptTokens),
                    TotalCompletionTokens = g.Sum(r => r.CompletionTokens)
                })
                .ToList();

            using (var writer = new StreamWriter(TemperatureComparisonCsvPath, false, System.Text.Encoding.UTF8))
            {
                writer.WriteLine("Temperature,TotalQuestions,ToolMatchRate,AvgParameterAccuracy,AvgDurationMs,TotalPromptTokens,TotalCompletionTokens");
                foreach (var s in byTemperature)
                {
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture,
                        "{0},{1},{2:F4},{3:F4},{4:F2},{5},{6}",
                        s.Temperature, s.TotalQuestions, s.ToolMatchRate, s.AvgParameterAccuracy,
                        s.AvgDurationMs, s.TotalPromptTokens, s.TotalCompletionTokens));
                }
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(TemperatureComparisonJsonPath, JsonSerializer.Serialize(byTemperature, options));
        }

        public static void Export(List<EvalResult> results, EvaluationSettings settings)
        {
            ExportDetailedCsv(results);
            ExportSummaryJson(results, settings);
        }

        private static void ExportDetailedCsv(List<EvalResult> results)
        {
            using var writer = new StreamWriter(DetailedCsvPath, false, System.Text.Encoding.UTF8);
            // CSV Header
            writer.WriteLine("QuestionId,Category,IsToolCallMatch,ParameterAccuracyScore,JudgeAccuracy,JudgeCompleteness,JudgeClarity,PromptTokens,CompletionTokens,TotalTokens,ExecutionDurationMs,ErrorMessage");

            foreach (var r in results)
            {
                var errorEscaped = string.IsNullOrEmpty(r.ErrorMessage) 
                    ? "" 
                    : $"\"{r.ErrorMessage.Replace("\"", "\"\"")}\"";

                writer.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "{0},{1},{2},{3:F4},{4},{5},{6},{7},{8},{9},{10},{11}",
                    r.QuestionId,
                    $"\"{r.Category.Replace("\"", "\"\"")}\"",
                    r.IsToolCallMatch ? 1 : 0,
                    r.ParameterAccuracyScore,
                    r.JudgeAccuracy,
                    r.JudgeCompleteness,
                    r.JudgeClarity,
                    r.PromptTokens,
                    r.CompletionTokens,
                    r.TotalTokens,
                    r.ExecutionDurationMs,
                    errorEscaped
                ));
            }
        }

        private static void ExportSummaryJson(List<EvalResult> results, EvaluationSettings settings)
        {
            var activeResults = results.Where(r => !r.Ignored).ToList();
            int totalQuestions = results.Count;
            int successfulRuns = activeResults.Count;

            // Gather all tools
            var allToolsInDataset = results.SelectMany(r => r.ExpectedTools)
                .Union(results.SelectMany(r => r.ActualToolCalls.Select(tc => tc.ToolName)))
                .Distinct()
                .ToList();

            var toolMetricsMap = MetricsCalculator.CalculateAggregateToolMetrics(results, allToolsInDataset);

            double avgLevel2 = activeResults.Any() 
                ? activeResults.Average(r => r.ParameterAccuracyScore) 
                : 0.0;

            double avgJudgeAccuracy = activeResults.Any() 
                ? activeResults.Average(r => r.JudgeAccuracy) 
                : 0.0;

            double avgJudgeCompleteness = activeResults.Any() 
                ? activeResults.Average(r => r.JudgeCompleteness) 
                : 0.0;

            double avgJudgeClarity = activeResults.Any() 
                ? activeResults.Average(r => r.JudgeClarity) 
                : 0.0;

            int totalPromptTokens = results.Sum(r => r.PromptTokens);
            int totalCompletionTokens = results.Sum(r => r.CompletionTokens);
            double avgDurationMs = results.Any() 
                ? results.Average(r => r.ExecutionDurationMs) 
                : 0.0;

            decimal estimatedCost = CalculateCosts(results, settings);

            var summary = new
            {
                EvaluationDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                GenerationModel = settings.GenerationModel,
                JudgeModel = settings.JudgeModel,
                TotalQuestions = totalQuestions,
                SuccessfulRuns = successfulRuns,
                FailedRuns = totalQuestions - successfulRuns,
                Level1OverallToolMatchRate = results.Any() ? (double)results.Count(r => r.IsToolCallMatch) / totalQuestions : 0.0,
                Level2AverageParameterAccuracy = avgLevel2,
                Level3AverageJudgeScores = new
                {
                    Accuracy = avgJudgeAccuracy,
                    Completeness = avgJudgeCompleteness,
                    Clarity = avgJudgeClarity
                },
                TotalPromptTokens = totalPromptTokens,
                TotalCompletionTokens = totalCompletionTokens,
                TotalTokens = totalPromptTokens + totalCompletionTokens,
                AverageDurationMs = avgDurationMs,
                EstimatedCostUsd = estimatedCost,
                Notes = "IMPORTANT: Per-tool 'Accuracy' (TP+TN)/(TP+TN+FP+FN) is misleadingly high due to heavy class imbalance " +
                        "(most tools are irrelevant to most questions, inflating True Negatives). " +
                        "Use Level1OverallToolMatchRate for overall tool selection quality, and per-tool Precision/Recall/F1 " +
                        "for individual tool performance. Do NOT cite per-tool Accuracy as a standalone quality metric.",
                ToolMetrics = toolMetricsMap.Values.Select(tm => new
                {
                    ToolName = tm.ToolName,
                    TruePositives = tm.TruePositives,
                    FalsePositives = tm.FalsePositives,
                    FalseNegatives = tm.FalseNegatives,
                    TrueNegatives = tm.TrueNegatives,
                    Precision = tm.Precision,
                    Recall = tm.Recall,
                    F1Score = tm.F1,
                    Accuracy = tm.Accuracy
                }).OrderByDescending(tm => tm.F1Score).ToList()
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(summary, options);
            File.WriteAllText(SummaryJsonPath, json);
        }

        private static decimal CalculateCosts(List<EvalResult> results, EvaluationSettings settings)
        {
            decimal cost = 0.0m;
            foreach (var r in results)
            {
                cost += CalculateEstimatedCost(settings.GenerationModel, r.PromptTokens, r.CompletionTokens);
                
                // Estimate Judge costs if judge calls succeeded (approximate deepseek r1 cost or similar if non-free)
                if (!r.Ignored && r.JudgeAccuracy > 0)
                {
                    // Let's assume judge prompt size is around 1200 tokens + 400 tokens completion
                    cost += CalculateEstimatedCost(settings.JudgeModel, 1200, 400);
                }
            }
            return cost;
        }

        private static decimal CalculateEstimatedCost(string model, int promptTokens, int completionTokens)
        {
            decimal inputRate = 0.0m;
            decimal outputRate = 0.0m;

            if (model.Contains("gpt-4o-mini"))
            {
                inputRate = 0.15m;
                outputRate = 0.60m;
            }
            else if (model.Contains("gemini-2.5-flash"))
            {
                inputRate = 0.075m;
                outputRate = 0.30m;
            }
            else if (model.Contains("deepseek-chat"))
            {
                inputRate = 0.14m;
                outputRate = 0.28m;
            }

            return ((promptTokens * inputRate) + (completionTokens * outputRate)) / 1_000_000m;
        }
    }
}

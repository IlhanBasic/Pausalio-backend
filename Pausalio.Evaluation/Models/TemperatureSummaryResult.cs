namespace Pausalio.Evaluation.Models
{
    public class TemperatureSummaryResult
    {
        public double Temperature { get; set; }
        public int TotalQuestions { get; set; }
        public double ToolMatchRate { get; set; }
        public double AvgParameterAccuracy { get; set; }
        public double AvgDurationMs { get; set; }
        public int TotalPromptTokens { get; set; }
        public int TotalCompletionTokens { get; set; }
    }
}
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pausalio.Evaluation.Models
{
    public class EvalQuestion
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("expected_tools")]
        public List<string> ExpectedTools { get; set; } = new();

        [JsonPropertyName("expected_parameters")]
        public Dictionary<string, object> ExpectedParameters { get; set; } = new();
    }
}

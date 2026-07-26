using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Pausalio.Evaluation.Models;

namespace Pausalio.Evaluation
{
    public class DatasetLoader
    {
        public static List<EvalQuestion> Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                var fallbackPath = Path.Combine(AppContext.BaseDirectory, filePath);
                if (File.Exists(fallbackPath))
                {
                    filePath = fallbackPath;
                }
                else
                {
                    throw new FileNotFoundException("Harness evaluation dataset not found.", filePath);
                }
            }

            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            return JsonSerializer.Deserialize<List<EvalQuestion>>(json, options) ?? new List<EvalQuestion>();
        }
    }
}

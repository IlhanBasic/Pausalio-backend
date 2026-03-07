using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Shared.Configuration
{
    public class OpenRouterSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }
}

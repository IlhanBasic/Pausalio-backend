using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.Authentication
{
    public class GoogleLoginRequestDto
    {
        public string IdToken { get; set; } = null!;
    }
}

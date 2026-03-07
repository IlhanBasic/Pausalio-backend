using Pausalio.Application.DTOs.AIAssistant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IAIAssistantService
    {
        Task<AIResponseDto> SendMessageAsync(UserChatMessage message);
    }
}

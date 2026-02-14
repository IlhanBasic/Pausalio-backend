using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IUploadFileService
    {
        Task DeleteFileAsync(string fileUrl);
        Task<string> UploadFileAsync(Stream fileStream, string originalFileName, string contentType);
    }
}

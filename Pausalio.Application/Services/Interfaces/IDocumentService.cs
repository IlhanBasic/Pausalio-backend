using Pausalio.Application.DTOs.Document;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<List<DocumentToReturnDto>> GetAllAsync();
        Task<DocumentToReturnDto?> GetByIdAsync(Guid id);
        Task CreateAsync(AddDocumentDto dto);
        Task UpdateAsync(Guid id, UpdateDocumentDto dto);
        Task DeleteAsync(Guid id);
    }
}
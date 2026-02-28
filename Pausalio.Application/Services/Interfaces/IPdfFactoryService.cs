using QuestPDF.Fluent;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IPdfFactoryService
    {
        byte[] GeneratePdf(Func<Document> documentFactory);
    }
}

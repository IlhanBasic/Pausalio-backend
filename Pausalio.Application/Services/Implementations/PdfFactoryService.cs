using Pausalio.Application.Services.Interfaces;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Implementations
{
    public class PdfFactoryService : IPdfFactoryService
    {
        private static readonly object _lockObject = new object();

        public byte[] GeneratePdf(Func<Document> documentFactory) // Promenjeno IDocument -> Document
        {
            lock (_lockObject)
            {
                try
                {
                    var document = documentFactory();
                    return document.GeneratePdf(); // Ovo sada radi
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Greška pri generisanju PDF-a: " + ex.Message, ex);
                }
            }
        }

       
    }
}

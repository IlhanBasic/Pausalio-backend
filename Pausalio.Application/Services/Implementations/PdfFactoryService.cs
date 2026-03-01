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

        public byte[] GeneratePdf(Func<Document> documentFactory)
        {
            lock (_lockObject)
            {
                try
                {
                    var document = documentFactory();
                    return document.GeneratePdf();
                }
                catch (Exception ex)
                {
                    var fullMessage = BuildExceptionMessage(ex);
                    throw new InvalidOperationException("Greška pri generisanju PDF-a: " + fullMessage, ex);
                }
            }
        }

        private string BuildExceptionMessage(Exception ex)
        {
            var sb = new StringBuilder();
            var current = ex;
            int depth = 0;

            while (current != null && depth < 5)
            {
                sb.AppendLine($"[Level {depth}]: {current.GetType().Name}: {current.Message}");
                if (current.StackTrace != null)
                    sb.AppendLine(current.StackTrace.Split('\n').FirstOrDefault());
                current = current.InnerException;
                depth++;
            }

            return sb.ToString();
        }


    }
}

using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.Document
{
    public class UpdateDocumentDto
    {
        public DocumentType DocumentType { get; set; } = DocumentType.Other;
        public string DocumentNumber { get; set; } = null!;
        public string FilePath { get; set; } = null!;
    }
}

using AutoMapper;
using Pausalio.Application.DTOs.Document;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Mappings
{
    public class DocumentMappingProfile : Profile
    {
        public DocumentMappingProfile()
        {
            CreateMap<Document, DocumentToReturnDto>();
            CreateMap<AddDocumentDto, Document>();
            CreateMap<UpdateDocumentDto, Document>();
        }
    }
}

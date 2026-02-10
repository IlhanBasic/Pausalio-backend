using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class ActivityCodeConfiguration : IEntityTypeConfiguration<ActivityCode>
    {
        public void Configure(EntityTypeBuilder<ActivityCode> builder)
        {
            builder.ToTable("ActivityCodes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired();

            builder.HasIndex(x => x.Code)
                .IsUnique();

            builder.HasData(
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Code = "IT01", Description = "Računarsko programiranje" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Code = "IT02", Description = "Razvoj veb aplikacija" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Code = "IT03", Description = "Razvoj mobilnih aplikacija" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), Code = "IT04", Description = "Softversko inženjerstvo" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000005"), Code = "IT05", Description = "Administracija baza podataka" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000006"), Code = "IT06", Description = "Konsalting u oblasti informacionih tehnologija" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000007"), Code = "IT07", Description = "Razvoj video igara" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000008"), Code = "IT08", Description = "Testiranje softvera" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000009"), Code = "DS01", Description = "Analiza podataka" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000010"), Code = "DS02", Description = "Mašinsko učenje" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000011"), Code = "DS03", Description = "Poslovna inteligencija" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000012"), Code = "GD01", Description = "Grafički dizajn" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000013"), Code = "GD02", Description = "UI/UX dizajn" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000014"), Code = "GD03", Description = "Dizajn logotipa i brendiranje" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000015"), Code = "GD04", Description = "Ilustracija i crtež" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000016"), Code = "GD05", Description = "Dizajn štampanih materijala" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000017"), Code = "WR01", Description = "Pisanje sadržaja za veb sajtove" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000018"), Code = "WR02", Description = "Kopirajting" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000019"), Code = "WR03", Description = "Tehničko pisanje" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000020"), Code = "TR01", Description = "Prevodenje tekstova" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000021"), Code = "TR02", Description = "Lektura i korekcija teksta" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000022"), Code = "TR03", Description = "Transkribovanje audio i video zapisa" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000023"), Code = "MK01", Description = "Digitalni marketing" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000024"), Code = "MK02", Description = "Upravljanje društvenim mrežama" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000025"), Code = "MK03", Description = "SEO optimizacija" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000026"), Code = "MK04", Description = "E-mail marketing" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000027"), Code = "MK05", Description = "Upravljanje Google Ads kampanjama" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000028"), Code = "VA01", Description = "Virtuelna asistenca" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000029"), Code = "VA02", Description = "Administrativna podrška" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000030"), Code = "VA03", Description = "Unošenje podataka" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000031"), Code = "VA04", Description = "Online istraživanje" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000032"), Code = "AV01", Description = "Video montaža" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000033"), Code = "AV02", Description = "Audio produkcija" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000034"), Code = "AV03", Description = "Fotografija" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000035"), Code = "AV04", Description = "Animacija" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000036"), Code = "AV05", Description = "Snimanje i produkcija podkasta" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000037"), Code = "ED01", Description = "Online podučavanje i mentoring" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000038"), Code = "ED02", Description = "Izrada online kurseva" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000039"), Code = "ED03", Description = "Poslovni konsalting" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000040"), Code = "ED04", Description = "Karijerno savetovanje" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000041"), Code = "ET01", Description = "Online prodaja proizvoda" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000042"), Code = "ET02", Description = "Dropshipping" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000043"), Code = "ET03", Description = "Upravljanje e-trgovinom" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000044"), Code = "ET04", Description = "Afilijet marketing" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000045"), Code = "FK01", Description = "Online knjigovodstvo" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000046"), Code = "FK02", Description = "Finansijsko savetovanje" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000047"), Code = "FK03", Description = "Poresko savetovanje" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000048"), Code = "FK04", Description = "Priprema poreskih prijava" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000049"), Code = "PR01", Description = "Pravno savetovanje" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000050"), Code = "PR02", Description = "Izrada pravnih dokumenata" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000051"), Code = "TE01", Description = "CAD projektovanje" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000052"), Code = "TE02", Description = "Arhitektonsko projektovanje" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000053"), Code = "TE03", Description = "3D modelovanje" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000054"), Code = "ZH01", Description = "Online psihološko savetovanje" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000055"), Code = "ZH02", Description = "Nutricionističko savetovanje" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000056"), Code = "ZH03", Description = "Fitnes trenering online" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000057"), Code = "OS01", Description = "Planiranje događaja online" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000058"), Code = "OS02", Description = "Online istraživanje tržišta" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000059"), Code = "OS03", Description = "Recenziranje proizvoda i usluga" },
                new ActivityCode { Id = Guid.Parse("00000000-0000-0000-0000-000000000060"), Code = "OS04", Description = "Moderacija online sadržaja" }
            );
        }
    }
}
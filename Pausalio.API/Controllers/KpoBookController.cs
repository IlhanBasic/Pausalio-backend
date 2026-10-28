using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.Services.Interfaces;

namespace Pausalio.API.Controllers
{
    [Route("api/kpo")]
    [ApiController]
    [Authorize]
    public class KpoBookController : ControllerBase
    {
        private readonly IKpoBookService _kpoBookService;

        public KpoBookController(IKpoBookService kpoBookService)
        {
            _kpoBookService = kpoBookService;
        }

        /// <summary>Sve KPO stavke za godinu.</summary>
        [HttpGet("{year:int}")]
        public async Task<IActionResult> GetByYear(int year)
        {
            var entries = await _kpoBookService.GetEntriesByYearAsync(year);
            return Ok(entries);
        }

        /// <summary>Godišnji sumar sa limitima i alertima.</summary>
        [HttpGet("{year:int}/summary")]
        public async Task<IActionResult> GetYearlySummary(int year)
        {
            var summary = await _kpoBookService.GetYearlySummaryAsync(year);
            return Ok(summary);
        }

        /// <summary>Pojedinačna KPO stavka.</summary>
        [HttpGet("entry/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entry = await _kpoBookService.GetByIdAsync(id);
            if (entry == null)
                return NotFound();
            return Ok(entry);
        }

        /// <summary>PDF eksport cele godine.</summary>
        [HttpGet("{year:int}/export/pdf")]
        public async Task<IActionResult> ExportPdf(int year)
        {
            var pdfBytes = await _kpoBookService.ExportPdfAsync(year);
            return File(pdfBytes, "application/pdf", $"KPO_{year}.pdf");
        }

        /// <summary>Backfill KPO zapisa za postojeće fakture.</summary>
        [HttpPost("backfill/{year:int}")]
        public async Task<IActionResult> Backfill(int year)
        {
            var count = await _kpoBookService.BackfillFromExistingInvoicesAsync(year);
            return Ok(new { BackfilledCount = count });
        }
    }
}

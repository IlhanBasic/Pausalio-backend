using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.Invoice;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "RegularUser")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ILocalizationHelper _localizationHelper;

        public InvoiceController(
            IInvoiceService invoiceService,
            ILocalizationHelper localizationHelper)
        {
            _invoiceService = invoiceService;
            _localizationHelper = localizationHelper;
        }

        /// <summary>
        /// Vraća sve fakture trenutne firme
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var invoices = await _invoiceService.GetAllAsync();
                return Ok(new { success = true, data = invoices, count = invoices.Count() });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Vraća fakturu po ID-ju
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var invoice = await _invoiceService.GetByIdAsync(id);

                if (invoice == null)
                    return NotFound(new { success = false, message = _localizationHelper.InvoiceNotFound });

                return Ok(new { success = true, data = invoice });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Vraća fakture po invoice statusu (Draft, Sent, Cancelled)
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(InvoiceStatus status)
        {
            try
            {
                var invoices = await _invoiceService.GetByStatusAsync(status);
                return Ok(new { success = true, data = invoices, count = invoices.Count() });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Vraća fakture po payment statusu (Paid, Unpaid, PartiallyPaid)
        /// </summary>
        [HttpGet("payment-status/{paymentStatus}")]
        public async Task<IActionResult> GetByPaymentStatus(PaymentStatus paymentStatus)
        {
            try
            {
                var invoices = await _invoiceService.GetByPaymentStatusAsync(paymentStatus);
                return Ok(new { success = true, data = invoices, count = invoices.Count() });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Kreira novu fakturu
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> Create([FromBody] AddInvoiceDto dto)
        {
            try
            {
                var created = await _invoiceService.CreateAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = _localizationHelper.InvoiceCreatedSuccessfully,
                    data = created
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Ažurira postojeću fakturu
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInvoiceDto dto)
        {
            try
            {
                await _invoiceService.UpdateAsync(id, dto);
                return Ok(new { success = true, message = _localizationHelper.InvoiceUpdatedSuccessfully });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Briše fakturu (soft delete)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _invoiceService.DeleteAsync(id);
                return Ok(new { success = true, message = _localizationHelper.InvoiceDeletedSuccessfully });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Vraća pregled (summary) faktura
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var summary = await _invoiceService.GetSummaryAsync();
                return Ok(new { success = true, data = summary });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
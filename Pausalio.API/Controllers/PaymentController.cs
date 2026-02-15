using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.Payment;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "RegularUser")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILocalizationHelper _localizationHelper;

        public PaymentController(
            IPaymentService paymentService,
            ILocalizationHelper localizationHelper)
        {
            _paymentService = paymentService;
            _localizationHelper = localizationHelper;
        }

        /// <summary>
        /// Vraća sva plaćanja trenutne firme
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var payments = await _paymentService.GetAllAsync();
                return Ok(new { success = true, data = payments, count = payments.Count() });
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
        /// Vraća plaćanja po tipu (InvoicePayment, TaxPayment, ExpensePayment)
        /// </summary>
        [HttpGet("type/{paymentType}")]
        public async Task<IActionResult> GetByType(PaymentType paymentType)
        {
            try
            {
                var payments = await _paymentService.GetByTypeAsync(paymentType);
                return Ok(new { success = true, data = payments, count = payments.Count() });
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
        /// Vraća jedno plaćanje po ID-ju
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var payment = await _paymentService.GetByIdAsync(id);

                if (payment == null)
                    return NotFound(new { success = false, message = _localizationHelper.PaymentNotFound });

                return Ok(new { success = true, data = payment });
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
        /// Kreira novo plaćanje (Invoice, Tax, ili Expense)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> Create([FromBody] AddPaymentDto dto)
        {
            try
            {
                var createdPayment = await _paymentService.CreateAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = _localizationHelper.PaymentCreatedSuccessfully,
                    data = createdPayment
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
        /// Ažurira plaćanje (samo Description i ReferenceNumber)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentDto dto)
        {
            try
            {
                await _paymentService.UpdateAsync(id, dto);
                return Ok(new { success = true, message = _localizationHelper.PaymentUpdatedSuccessfully });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Briše plaćanje i revertuje promene na povezanim entitetima
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _paymentService.DeleteAsync(id);
                return Ok(new { success = true, message = _localizationHelper.PaymentDeletedSuccessfully });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Vraća pregled (summary) plaćanja
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var summary = await _paymentService.GetSummaryAsync();
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.Expense;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "RegularUser")]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly ILocalizationHelper _localizationHelper;

        public ExpenseController(
            IExpenseService expenseService,
            ILocalizationHelper localizationHelper)
        {
            _expenseService = expenseService;
            _localizationHelper = localizationHelper;
        }

        /// <summary>
        /// Vraća sve troškove trenutne firme
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var expenses = await _expenseService.GetAllAsync();
                return Ok(new { success = true, data = expenses, count = expenses.Count() });
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
        /// Vraća troškove po statusu (Pending, Paid, Archived)
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(ExpenseStatus status)
        {
            try
            {
                var expenses = await _expenseService.GetByStatusAsync(status);
                return Ok(new { success = true, data = expenses, count = expenses.Count() });
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
        /// Vraća jedan trošak po ID-ju
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var expense = await _expenseService.GetByIdAsync(id);

                if (expense == null)
                    return NotFound(new { success = false, message = _localizationHelper.ExpenseNotFound });

                return Ok(new { success = true, data = expense });
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
        /// Kreira novi trošak za trenutnu firmu
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> Create([FromBody] AddExpenseDto dto)
        {
            try
            {
                var createdExpense = await _expenseService.CreateAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = _localizationHelper.ExpenseCreatedSuccessfully,
                    data = createdExpense
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
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
        /// Ažurira postojeći trošak
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExpenseDto dto)
        {
            try
            {
                await _expenseService.UpdateAsync(id, dto);
                return Ok(new { success = true, message = _localizationHelper.ExpenseUpdatedSuccessfully });
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
        /// Soft delete - briše trošak (postavlja IsDeleted = true)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _expenseService.DeleteAsync(id);
                return Ok(new { success = true, message = _localizationHelper.ExpenseDeletedSuccessfully });
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
        /// Arhivira trošak (promeni status u Archived)
        /// </summary>
        [HttpPatch("{id:guid}/archive")]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> Archive(Guid id)
        {
            try
            {
                await _expenseService.ArchiveAsync(id);
                return Ok(new { success = true, message = _localizationHelper.ExpenseArchivedSuccessfully });
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
        /// Vraća ukupan iznos troškova po statusu
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var summary = await _expenseService.GetSummaryAsync();
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
using Microsoft.Extensions.Logging;
using Pausalio.Application.DTOs.Expense;
using Pausalio.Application.DTOs.Invoice;
using Pausalio.Application.DTOs.Payment;
using Pausalio.Application.DTOs.TaxObligation;
using Pausalio.Shared.Enums;
using System.Text.Json;

namespace Pausalio.Application.Services.Implementations.AIAssistant
{
    public class AIAssistantToolExecutor
    {
        private readonly ILogger<AIAssistantToolExecutor> _logger;

        public AIAssistantToolExecutor(ILogger<AIAssistantToolExecutor> logger)
        {
            _logger = logger;
        }

        public string ExecuteTool(string functionName, string argumentsJson, CachedToolData data)
        {
            try
            {
                using var document = JsonDocument.Parse(argumentsJson);
                var args = document.RootElement;

                switch (functionName)
                {
                    case "get_top_clients":
                        return ExecuteGetTopClients(args, data);
                    case "get_invoices_by_status":
                        return ExecuteGetInvoicesByStatus(args, data);
                    case "get_invoices_by_payment_status":
                        return ExecuteGetInvoicesByPaymentStatus(args, data);
                    case "get_invoices_by_year":
                        return ExecuteGetInvoicesByYear(args, data);
                    case "get_overdue_invoices":
                        return ExecuteGetOverdueInvoices(data);
                    case "get_invoice_summary":
                        return JsonSerializer.Serialize(data.InvoiceSummary);
                    case "get_expenses_by_status":
                        return ExecuteGetExpensesByStatus(args, data);
                    case "get_expense_summary":
                        return JsonSerializer.Serialize(data.ExpenseSummary);
                    case "get_tax_obligations_by_year":
                        return ExecuteGetTaxObligationsByYear(args, data);
                    case "get_tax_obligations_by_status":
                        return ExecuteGetTaxObligationsByStatus(args, data);
                    case "get_overdue_taxes":
                        return ExecuteGetOverdueTaxes(data);
                    case "get_tax_summary":
                        return ExecuteGetTaxSummary(args, data);
                    case "get_monthly_income":
                        return ExecuteGetMonthlyIncome(args, data);
                    case "get_income_vs_expenses":
                        return ExecuteGetIncomeVsExpenses(args, data);
                    case "get_top_services":
                        return ExecuteGetTopServices(args, data);
                    case "get_actual_cashflow":
                        return ExecuteGetActualCashflow(args, data);
                    case "get_avg_payment_delay_by_client":
                        return ExecuteGetAvgPaymentDelayByClient(args, data);
                    case "get_tax_delay_analysis":
                        return ExecuteGetTaxDelayAnalysis(data);
                    case "get_client_service_breakdown":
                        return ExecuteGetClientServiceBreakdown(args, data);
                    default:
                        return "Alat nije pronađen.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid argument provided for tool: {ToolName}", functionName);
                return $"Invalid argument provided for tool '{functionName}': {ex.Message}";
            }
        }

        private static int GetRequiredInt(JsonElement args, string propertyName)
        {
            if (!args.TryGetProperty(propertyName, out var propertyValue)
                || propertyValue.ValueKind != JsonValueKind.Number
                || !propertyValue.TryGetInt32(out var parsedValue))
            {
                throw new ArgumentException($"Missing or invalid integer argument '{propertyName}'.");
            }

            return parsedValue;
        }

        private static string GetRequiredString(JsonElement args, string propertyName)
        {
            if (!args.TryGetProperty(propertyName, out var propertyValue)
                || propertyValue.ValueKind == JsonValueKind.Null
                || string.IsNullOrWhiteSpace(propertyValue.GetString()))
            {
                throw new ArgumentException($"Missing or invalid string argument '{propertyName}'.");
            }

            return propertyValue.GetString()!;
        }

        private static string? GetOptionalString(JsonElement args, string propertyName)
        {
            if (!args.TryGetProperty(propertyName, out var propertyValue)
                || propertyValue.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            return propertyValue.GetString();
        }

        private static int? GetOptionalInt(JsonElement args, string propertyName)
        {
            if (!args.TryGetProperty(propertyName, out var propertyValue)
                || propertyValue.ValueKind != JsonValueKind.Number
                || !propertyValue.TryGetInt32(out var parsedValue))
            {
                return null;
            }

            return parsedValue;
        }

        private string ExecuteGetTopClients(JsonElement args, CachedToolData data)
        {
            var top = GetRequiredInt(args, "top");
            var clientTypeFilter = ParseEnum<ClientType>(args, "clientType");

            var topClients = data.Invoices
                .Where(x => clientTypeFilter == null || x.Client.ClientType == clientTypeFilter)
                .GroupBy(x => new { x.Client.Id, x.Client.Name })
                .Select(g => new
                {
                    Klijent = g.Key.Name,
                    UkupnoFakturisano = g.Sum(x => x.TotalAmountRSD),
                    BrojFaktura = g.Count()
                })
                .OrderByDescending(x => x.UkupnoFakturisano)
                .Take(top)
                .ToList();

            return JsonSerializer.Serialize(topClients);
        }

        private string ExecuteGetInvoicesByStatus(JsonElement args, CachedToolData data)
        {
            var statusStr = GetRequiredString(args, "status");
            if (!Enum.TryParse<InvoiceStatus>(statusStr, true, out var status))
                return "Nepoznat status fakture.";

            var result = data.Invoices
                .Where(x => x.InvoiceStatus == status)
                .Select(x => new
                {
                    x.InvoiceNumber,
                    Klijent = x.Client.Name,
                    x.TotalAmountRSD,
                    x.PaymentStatus,
                    x.IssueDate
                });

            return JsonSerializer.Serialize(result);
        }

        private string ExecuteGetInvoicesByPaymentStatus(JsonElement args, CachedToolData data)
        {
            var statusStr = GetRequiredString(args, "paymentStatus");
            if (!Enum.TryParse<PaymentStatus>(statusStr, true, out var paymentStatus))
                return "Nepoznat status plaćanja.";

            var result = data.Invoices
                .Where(x => x.PaymentStatus == paymentStatus)
                .Select(x => new
                {
                    x.InvoiceNumber,
                    Klijent = x.Client.Name,
                    x.TotalAmountRSD,
                    x.InvoiceStatus,
                    x.DueDate,
                    x.IssueDate
                });

            return JsonSerializer.Serialize(result);
        }

        private string ExecuteGetInvoicesByYear(JsonElement args, CachedToolData data)
        {
            var year = GetRequiredInt(args, "year");

            var result = data.Invoices
                .Where(x => x.IssueDate.Year == year)
                .Select(x => new
                {
                    x.InvoiceNumber,
                    Klijent = x.Client.Name,
                    x.TotalAmountRSD,
                    x.PaymentStatus,
                    x.InvoiceStatus,
                    x.IssueDate
                });

            return JsonSerializer.Serialize(result);
        }

        private string ExecuteGetOverdueInvoices(CachedToolData data)
        {
            var now = DateTime.UtcNow;

            var result = data.Invoices
                .Where(x => x.PaymentStatus == PaymentStatus.Unpaid
                            && x.DueDate.HasValue
                            && x.DueDate < now
                            && x.InvoiceStatus != InvoiceStatus.Cancelled)
                .Select(x => new
                {
                    x.InvoiceNumber,
                    Klijent = x.Client.Name,
                    x.TotalAmountRSD,
                    x.DueDate,
                    DanaKasnjenja = (int)(now - x.DueDate!.Value).TotalDays
                })
                .OrderByDescending(x => x.DanaKasnjenja);

            return JsonSerializer.Serialize(result);
        }

        private string ExecuteGetExpensesByStatus(JsonElement args, CachedToolData data)
        {
            var statusStr = GetRequiredString(args, "status");
            if (!Enum.TryParse<ExpenseStatus>(statusStr, true, out var status))
                return "Nepoznat status troška.";

            var result = data.Expenses
                .Where(x => x.Status == status)
                .Select(x => new
                {
                    x.Name,
                    x.Amount,
                    x.Status,
                    x.ReferenceNumber
                });

            return JsonSerializer.Serialize(result);
        }

        private string ExecuteGetTaxObligationsByYear(JsonElement args, CachedToolData data)
        {
            var year = GetRequiredInt(args, "year");

            var result = data.TaxObligations
                .Where(x => x.Year == year)
                .Select(x => new
                {
                    x.Year,
                    x.Month,
                    x.Type,
                    x.TotalAmount,
                    x.Status,
                    x.DueDate
                });

            return JsonSerializer.Serialize(result);
        }

        private string ExecuteGetTaxObligationsByStatus(JsonElement args, CachedToolData data)
        {
            var statusStr = GetRequiredString(args, "status");
            if (!Enum.TryParse<TaxObligationStatus>(statusStr, true, out var status))
                return "Nepoznat status poreske obaveze.";

            var result = data.TaxObligations
                .Where(x => x.Status == status)
                .Select(x => new
                {
                    x.Year,
                    x.Month,
                    x.Type,
                    x.TotalAmount,
                    x.DueDate
                });

            return JsonSerializer.Serialize(result);
        }

        private string ExecuteGetOverdueTaxes(CachedToolData data)
        {
            var now = DateTime.UtcNow;

            var result = data.TaxObligations
                .Where(x => x.Status == TaxObligationStatus.Pending && x.DueDate < now)
                .Select(x => new
                {
                    x.Year,
                    x.Month,
                    x.Type,
                    x.TotalAmount,
                    x.DueDate,
                    DanaKasnjenja = (int)(now - x.DueDate).TotalDays
                })
                .OrderByDescending(x => x.DanaKasnjenja);

            return JsonSerializer.Serialize(result);
        }

        private string ExecuteGetTaxSummary(JsonElement args, CachedToolData data)
        {
            int? year = GetOptionalInt(args, "year");

            var obligations = year.HasValue
                ? data.TaxObligations.Where(x => x.Year == year.Value).ToList()
                : data.TaxObligations;

            var summary = new
            {
                UkupnoObaveza = obligations.Sum(x => x.TotalAmount),
                BrojObaveza = obligations.Count(),
                Placeno = obligations.Where(x => x.Status == TaxObligationStatus.Paid).Sum(x => x.TotalAmount),
                NePlaceno = obligations.Where(x => x.Status == TaxObligationStatus.Pending).Sum(x => x.TotalAmount)
            };

            return JsonSerializer.Serialize(summary);
        }

        private string ExecuteGetMonthlyIncome(JsonElement args, CachedToolData data)
        {
            var year = GetRequiredInt(args, "year");

            var result = data.Invoices
                .Where(x => x.IssueDate.Year == year && x.InvoiceStatus != InvoiceStatus.Cancelled)
                .GroupBy(x => x.IssueDate.Month)
                .Select(g => new
                {
                    Mesec = g.Key,
                    UkupnoRSD = g.Sum(x => x.TotalAmountRSD),
                    BrojFaktura = g.Count()
                })
                .OrderBy(x => x.Mesec);

            return JsonSerializer.Serialize(result);
        }

        private string ExecuteGetIncomeVsExpenses(JsonElement args, CachedToolData data)
        {
            var year = GetRequiredInt(args, "year");

            var ukupniPrihodi = data.Invoices
                .Where(x => x.IssueDate.Year == year && x.InvoiceStatus != InvoiceStatus.Cancelled)
                .Sum(x => x.TotalAmountRSD);

            var ukupniTroskovi = data.Expenses.Sum(x => x.Amount);

            var result = new
            {
                Godina = year,
                UkupniPrihodiRSD = ukupniPrihodi,
                UkupniTroskoviRSD = ukupniTroskovi,
                NetoPrihodRSD = ukupniPrihodi - ukupniTroskovi
            };

            return JsonSerializer.Serialize(result);
        }

        private string ExecuteGetTopServices(JsonElement args, CachedToolData data)
        {
            var top = GetRequiredInt(args, "top");
            var itemTypeFilter = ParseEnum<ItemType>(args, "itemType");
            var year = GetOptionalInt(args, "year");
            var clientId = GetOptionalString(args, "clientId");

            var result = data.Invoices
                .Where(x => x.InvoiceStatus != InvoiceStatus.Cancelled)
                .Where(x => year == null || x.IssueDate.Year == year)
                .Where(x => clientId == null || x.Client.Id.ToString() == clientId)
                .SelectMany(x => x.Items)
                .Where(x => itemTypeFilter == null || x.ItemType == itemTypeFilter)
                .GroupBy(x => new { x.Name, x.ItemType })
                .Select(g => new
                {
                    Naziv = g.Key.Name,
                    Tip = g.Key.ItemType.ToString(),
                    UkupanPrihodRSD = g.Sum(x => x.TotalPrice),
                    BrojPojavljivanja = g.Count(),
                    UkupnoKolicina = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.UkupanPrihodRSD)
                .Take(top);

            return JsonSerializer.Serialize(result);
        }

        private string ExecuteGetActualCashflow(JsonElement args, CachedToolData data)
        {
            var year = GetRequiredInt(args, "year");
            var month = GetOptionalInt(args, "month");

            var result = data.Payments
                .Where(x => x.PaymentType == PaymentType.InvoicePayment)
                .Where(x => x.PaymentDate.Year == year)
                .Where(x => month == null || x.PaymentDate.Month == month)
                .GroupBy(x => x.PaymentDate.Month)
                .Select(g => new
                {
                    Mesec = g.Key,
                    UkupnoNaplaćenoRSD = g.Sum(x => x.AmountRSD),
                    BrojUplata = g.Count()
                })
                .OrderBy(x => x.Mesec);

            return JsonSerializer.Serialize(result);
        }

        private string ExecuteGetAvgPaymentDelayByClient(JsonElement args, CachedToolData data)
        {
            var top = GetOptionalInt(args, "top") ?? 5;

            var result = data.Payments
                .Where(x => x.PaymentType == PaymentType.InvoicePayment
                            && x.Invoice != null
                            && x.Invoice.DueDate.HasValue)
                .GroupBy(x => x.Invoice!.Client.Name)
                .Select(g => new
                {
                    Klijent = g.Key,
                    ProsečnoKašnjenjeDana = (int)g
                        .Where(x => x.PaymentDate > x.Invoice!.DueDate!.Value)
                        .Select(x => (x.PaymentDate - x.Invoice!.DueDate!.Value).TotalDays)
                        .DefaultIfEmpty(0)
                        .Average(),
                    NajdužeKašnjenjeDana = (int)g
                        .Where(x => x.PaymentDate > x.Invoice!.DueDate!.Value)
                        .Select(x => (x.PaymentDate - x.Invoice!.DueDate!.Value).TotalDays)
                        .DefaultIfEmpty(0)
                        .Max(),
                    BrojFaktura = g.Count(),
                    BrojKasnihPlacanja = g.Count(x => x.PaymentDate > x.Invoice!.DueDate!.Value)
                })
                .OrderByDescending(x => x.ProsečnoKašnjenjeDana)
                .Take(top);

            return JsonSerializer.Serialize(result);
        }

        private string ExecuteGetTaxDelayAnalysis(CachedToolData data)
        {
            var result = data.TaxObligations
                .Where(x => x.Status == TaxObligationStatus.Paid && x.PaidDate.HasValue)
                .GroupBy(x => x.Type)
                .Select(g => new
                {
                    TipPoreza = g.Key.ToString(),
                    BrojPlacanja = g.Count(),
                    BrojKasnihPlacanja = g.Count(x => x.PaidDate!.Value > x.DueDate),
                    ProsečnoKašnjenjeDana = (int)g
                        .Where(x => x.PaidDate!.Value > x.DueDate)
                        .Select(x => (x.PaidDate!.Value - x.DueDate).TotalDays)
                        .DefaultIfEmpty(0)
                        .Average(),
                    NajdužeKašnjenjeDana = (int)g
                        .Where(x => x.PaidDate!.Value > x.DueDate)
                        .Select(x => (x.PaidDate!.Value - x.DueDate).TotalDays)
                        .DefaultIfEmpty(0)
                        .Max()
                })
                .OrderByDescending(x => x.ProsečnoKašnjenjeDana);

            return JsonSerializer.Serialize(result);
        }

        private string ExecuteGetClientServiceBreakdown(JsonElement args, CachedToolData data)
        {
            var clientName = GetRequiredString(args, "clientName");

            var klijentInvoices = data.Invoices
                .Where(x => x.Client.Name.Contains(clientName, StringComparison.OrdinalIgnoreCase)
                            && x.InvoiceStatus != InvoiceStatus.Cancelled)
                .ToList();

            if (!klijentInvoices.Any())
                return $"Nije pronađen klijent sa imenom '{clientName}'.";

            var imeKlijenta = klijentInvoices.First().Client.Name;

            var uslugeBreakdown = klijentInvoices
                .SelectMany(x => x.Items)
                .GroupBy(x => new { x.Name, x.ItemType })
                .Select(g => new
                {
                    Naziv = g.Key.Name,
                    Tip = g.Key.ItemType.ToString(),
                    UkupanPrihodRSD = g.Sum(x => x.TotalPrice),
                    BrojPojavljivanja = g.Count(),
                    UkupnoKolicina = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.UkupanPrihodRSD);

            var result = new
            {
                Klijent = imeKlijenta,
                UkupnoFakturisanoRSD = klijentInvoices.Sum(x => x.TotalAmountRSD),
                BrojFaktura = klijentInvoices.Count,
                Usluge = uslugeBreakdown
            };

            return JsonSerializer.Serialize(result);
        }

        private static TEnum? ParseEnum<TEnum>(JsonElement args, string propertyName)
            where TEnum : struct, Enum
        {
            if (!args.TryGetProperty(propertyName, out var propertyValue)
                || propertyValue.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            var value = propertyValue.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Enum.TryParse<TEnum>(value, true, out var parsedValue)
                ? parsedValue
                : null;
        }
    }
}

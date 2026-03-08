using Microsoft.Extensions.Options;
using Pausalio.Application.DTOs.AIAssistant;
using Pausalio.Application.Helpers;
using Pausalio.Application.Helpers.Pausalio.Application.Helpers;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Configuration;
using Pausalio.Shared.Enums;
using System.Text;
using System.Text.Json;

namespace Pausalio.Application.Services.Implementations
{
    public class AIAssistantService : IAIAssistantService
    {
        private readonly IFinancialContextService _financialContextService;
        private readonly IInvoiceService _invoiceService;
        private readonly IExpenseService _expenseService;
        private readonly ITaxObligationService _taxObligationService;
        private readonly IOptions<OpenRouterSettings> _configuration;
        private readonly HttpClient _httpClient;

        public AIAssistantService(
            IFinancialContextService financialContextService,
            IInvoiceService invoiceService,
            IExpenseService expenseService,
            ITaxObligationService taxObligationService,
            IOptions<OpenRouterSettings> configuration,
            HttpClient httpClient)
        {
            _financialContextService = financialContextService;
            _invoiceService = invoiceService;
            _expenseService = expenseService;
            _taxObligationService = taxObligationService;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<AIResponseDto> SendMessageAsync(UserChatMessage message)
        {
            var financialContext = await _financialContextService.BuildContextAsync();
            var systemPrompt = AIAssistantPromptHelper.BuildSystemPrompt(financialContext);

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            foreach (var item in message.History)
                messages.Add(new { role = item.Role, content = item.Content });

            messages.Add(new { role = "user", content = message.Message });

            var tools = AIToolsDefinition.GetTools();

            while (true)
            {
                var requestBody = new
                {
                    model = _configuration.Value.Model,
                    messages,
                    tools,
                    tool_choice = "auto",
                    max_tokens = 1000,
                    temperature = 0.7
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration.Value.ApiKey}");

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_configuration.Value.ApiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseString);
                var choice = responseJson.RootElement.GetProperty("choices")[0];
                var finishReason = choice.GetProperty("finish_reason").GetString();
                var aiMessage = choice.GetProperty("message");

                if (finishReason != "tool_calls")
                {
                    var text = aiMessage.GetProperty("content").GetString()
                        ?? "Nije moguće dobiti odgovor.";
                    return new AIResponseDto { Message = text };
                }

                messages.Add(JsonSerializer.Deserialize<object>(aiMessage.GetRawText())!);

                var toolCalls = aiMessage.GetProperty("tool_calls");
                foreach (var toolCall in toolCalls.EnumerateArray())
                {
                    var toolCallId = toolCall.GetProperty("id").GetString()!;
                    var functionName = toolCall.GetProperty("function").GetProperty("name").GetString()!;
                    var argumentsJson = toolCall.GetProperty("function").GetProperty("arguments").GetString()!;

                    var toolResult = await ExecuteToolAsync(functionName, argumentsJson);

                    messages.Add(new
                    {
                        role = "tool",
                        tool_call_id = toolCallId,
                        content = toolResult
                    });
                }
            }
        }

        private async Task<string> ExecuteToolAsync(string functionName, string argumentsJson)
        {
            var args = JsonDocument.Parse(argumentsJson).RootElement;

            switch (functionName)
            {
                // ===== KLIJENTI =====
                case "get_top_clients":
                    {
                        var top = args.GetProperty("top").GetInt32();

                        ClientType? clientTypeFilter = null;
                        if (args.TryGetProperty("clientType", out var clientTypeProp))
                        {
                            if (Enum.TryParse<ClientType>(clientTypeProp.GetString(), out var parsedType))
                                clientTypeFilter = parsedType;
                        }

                        var invoices = await _invoiceService.GetAllAsync();

                        var topClients = invoices
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

                // ===== FAKTURE =====
                case "get_invoices_by_status":
                    {
                        var statusStr = args.GetProperty("status").GetString()!;
                        if (!Enum.TryParse<InvoiceStatus>(statusStr, out var status))
                            return "Nepoznat status fakture.";

                        var invoices = await _invoiceService.GetByStatusAsync(status);

                        var result = invoices.Select(x => new
                        {
                            x.InvoiceNumber,
                            Klijent = x.Client.Name,
                            x.TotalAmountRSD,
                            x.PaymentStatus,
                            x.IssueDate
                        });

                        return JsonSerializer.Serialize(result);
                    }

                case "get_invoices_by_payment_status":
                    {
                        var statusStr = args.GetProperty("paymentStatus").GetString()!;
                        if (!Enum.TryParse<PaymentStatus>(statusStr, out var paymentStatus))
                            return "Nepoznat status plaćanja.";

                        var invoices = await _invoiceService.GetByPaymentStatusAsync(paymentStatus);

                        var result = invoices.Select(x => new
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

                case "get_invoices_by_year":
                    {
                        var year = args.GetProperty("year").GetInt32();
                        var invoices = await _invoiceService.GetAllAsync();

                        var result = invoices
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

                case "get_overdue_invoices":
                    {
                        var invoices = await _invoiceService.GetAllAsync();
                        var now = DateTime.UtcNow;

                        var result = invoices
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

                case "get_invoice_summary":
                    {
                        var summary = await _invoiceService.GetSummaryAsync();
                        return JsonSerializer.Serialize(summary);
                    }

                // ===== TROŠKOVI =====
                case "get_expenses_by_status":
                    {
                        var statusStr = args.GetProperty("status").GetString()!;
                        if (!Enum.TryParse<ExpenseStatus>(statusStr, out var status))
                            return "Nepoznat status troška.";

                        var expenses = await _expenseService.GetByStatusAsync(status);

                        var result = expenses.Select(x => new
                        {
                            x.Name,
                            x.Amount,
                            x.Status,
                            x.ReferenceNumber
                        });

                        return JsonSerializer.Serialize(result);
                    }

                case "get_expense_summary":
                    {
                        var summary = await _expenseService.GetSummaryAsync();
                        return JsonSerializer.Serialize(summary);
                    }

                // ===== POREZI =====
                case "get_tax_obligations_by_year":
                    {
                        var year = args.GetProperty("year").GetInt32();
                        var obligations = await _taxObligationService.GetByYearAsync(year);

                        var result = obligations.Select(x => new
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

                case "get_tax_obligations_by_status":
                    {
                        var statusStr = args.GetProperty("status").GetString()!;
                        if (!Enum.TryParse<TaxObligationStatus>(statusStr, out var status))
                            return "Nepoznat status poreske obaveze.";

                        var obligations = await _taxObligationService.GetByStatusAsync(status);

                        var result = obligations.Select(x => new
                        {
                            x.Year,
                            x.Month,
                            x.Type,
                            x.TotalAmount,
                            x.DueDate
                        });

                        return JsonSerializer.Serialize(result);
                    }

                case "get_overdue_taxes":
                    {
                        var obligations = await _taxObligationService.GetByStatusAsync(TaxObligationStatus.Pending);
                        var now = DateTime.UtcNow;

                        var result = obligations
                            .Where(x => x.DueDate < now)
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

                case "get_tax_summary":
                    {
                        int? year = null;
                        if (args.TryGetProperty("year", out var yearProp))
                            year = yearProp.GetInt32();

                        var summary = await _taxObligationService.GetSummaryAsync(year);
                        return JsonSerializer.Serialize(summary);
                    }

                // ===== PRIHODI =====
                case "get_monthly_income":
                    {
                        var year = args.GetProperty("year").GetInt32();
                        var invoices = await _invoiceService.GetAllAsync();

                        var result = invoices
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

                case "get_income_vs_expenses":
                    {
                        var year = args.GetProperty("year").GetInt32();
                        var invoices = await _invoiceService.GetAllAsync();
                        var expenses = await _expenseService.GetAllAsync();

                        var ukupniPrihodi = invoices
                            .Where(x => x.IssueDate.Year == year && x.InvoiceStatus != InvoiceStatus.Cancelled)
                            .Sum(x => x.TotalAmountRSD);

                        var ukupniTroskovi = expenses
                            .Sum(x => x.Amount);

                        var result = new
                        {
                            Godina = year,
                            UkupniPrihodiRSD = ukupniPrihodi,
                            UkupniTroskoviRSD = ukupniTroskovi,
                            NetoPrihodRSD = ukupniPrihodi - ukupniTroskovi
                        };

                        return JsonSerializer.Serialize(result);
                    }

                default:
                    return "Alat nije pronađen.";
            }
        }
    }
}
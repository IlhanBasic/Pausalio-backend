namespace Pausalio.Application.Helpers
{
    public static class AIToolsDefinition
    {
        public static object[] GetTools() => new object[]
        {
            // ===== KLIJENTI =====
            new
            {
                type = "function",
                function = new
                {
                    name = "get_top_clients",
                    description = "Use this tool only when the user asks for the highest-value clients ranked by total invoiced amount. Do not use it for a general client directory, a single client detail lookup, or a service breakdown. For a list of all clients use get_client_list. For one client's service mix use get_client_service_breakdown.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            top = new
                            {
                                type = "integer",
                                description = "How many clients to return. Example: 5. If the user does not specify a number, use 5."
                            },
                            clientType = new
                            {
                                type = "string",
                                description = "Optional client type filter. Allowed values: Individual, Domestic, Foreign. Example: Domestic. Leave it out when the user asks for all clients."
                            }
                        },
                        required = new[] { "top" },
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_client_list",
                    description = "Use this tool only when the user asks for a registry or directory of clients, such as 'show me all clients', 'who are my clients', or 'list clients with PIB'. Do not use it for a ranked top-client list, invoice analysis, or one client's service composition. For top clients use get_top_clients. For a single client's purchased services use get_client_service_breakdown.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            clientType = new
                            {
                                type = "string",
                                description = "Optional client type filter. Allowed values: Individual, Domestic, Foreign. Example: Foreign."
                            },
                            isActive = new
                            {
                                type = "boolean",
                                description = "Optional active/inactive filter. Example: true."
                            },
                            limit = new
                            {
                                type = "integer",
                                description = "Optional maximum number of records to return. Example: 25."
                            }
                        },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_client_service_breakdown",
                    description = "Use this tool only when the user asks what products or services a specific client bought, how much they spent, or how their purchasing pattern looks. Do not use it for a client directory, top-client ranking, or a general invoice list. For a list of all clients use get_client_list. For ranking clients by revenue use get_top_clients.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            clientName = new
                            {
                                type = "string",
                                description = "Client name or a partial name fragment. Example: 'ABC'. The tool performs a case-insensitive partial match."
                            }
                        },
                        required = new[] { "clientName" },
                        additionalProperties = false
                    }
                }
            },

            // ===== FAKTURE =====
            new
            {
                type = "function",
                function = new
                {
                    name = "get_invoices",
                    description = "Use this tool only when the user asks for invoice records or wants invoices filtered by year, invoice status, payment status, or overdue state. Do not use this tool for totals, invoice summaries, or a single invoice lookup by invoice number; use get_invoice_summary for totals and get_invoice_by_number for one exact invoice. Example questions: 'Show invoices for 2026', 'List unpaid invoices', 'Which invoices are overdue?'.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            year = new
                            {
                                type = "integer",
                                description = "Optional invoice year filter. Example: 2026."
                            },
                            invoiceStatus = new
                            {
                                type = "string",
                                description = "Optional invoice status filter. Allowed values: Draft, Sent, Finished, Cancelled, Archived. Example: Sent."
                            },
                            paymentStatus = new
                            {
                                type = "string",
                                description = "Optional payment status filter. Allowed values: Unpaid, Paid, PartiallyPaid. Example: Unpaid."
                            },
                            overdueOnly = new
                            {
                                type = "boolean",
                                description = "If true, return only invoices that are unpaid, past due, and not cancelled. Example: true."
                            },
                            limit = new
                            {
                                type = "integer",
                                description = "Optional maximum number of invoice records to return. Example: 25."
                            }
                        },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_overdue_invoices",
                    description = "Use this tool only when the user asks for overdue unpaid invoices that are past their due date. Do not use it for a general invoice list, invoice summary, or a single invoice lookup. For filtered invoice records use get_invoices. For one exact invoice use get_invoice_by_number. Example questions: 'Show overdue invoices', 'Which customers have overdue invoices?'.",
                    parameters = new
                    {
                        type = "object",
                        properties = new { },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_invoice_summary",
                    description = "Use this tool only when the user asks for an aggregate invoice overview, such as total issued amount, total paid, total unpaid, or invoice status distribution. Do not use it for a list of individual invoice rows or for one exact invoice lookup. For invoice records use get_invoices. For one invoice by number use get_invoice_by_number. Example questions: 'What is the invoice summary?', 'How much is unpaid?', 'Give me invoice totals by status'.",
                    parameters = new
                    {
                        type = "object",
                        properties = new { },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_invoice_by_number",
                    description = "Use this tool only when the user asks for one exact invoice by its invoice number, such as 'show me invoice INV-1024' or 'what is on invoice 2026-001'. Do not use it for a yearly filtered invoice list, a summary, or an overdue invoice search. For filtered invoice record sets use get_invoices. For overall totals use get_invoice_summary. Example questions: 'Show invoice INV-1024', 'What does invoice 2026-001 contain?'.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            invoiceNumber = new
                            {
                                type = "string",
                                description = "Exact invoice number to look up. Example: INV-1024."
                            }
                        },
                        required = new[] { "invoiceNumber" },
                        additionalProperties = false
                    }
                }
            },

            // ===== TROŠKOVI =====
            new
            {
                type = "function",
                function = new
                {
                    name = "get_expenses",
                    description = "Use this tool only when the user asks for expense records or wants expense rows filtered by year or expense status. Do not use this tool for totals or an expense overview; use get_expense_summary for aggregates. Example questions: 'List expenses for 2026', 'Show paid expenses', 'Which expenses are pending?'.",

                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            year = new
                            {
                                type = "integer",
                                description = "Optional expense year filter. Example: 2026."
                            },
                            expenseStatus = new
                            {
                                type = "string",
                                description = "Optional expense status filter. Allowed values: Pending, Paid, Archived. Example: Paid."
                            },
                            limit = new
                            {
                                type = "integer",
                                description = "Optional maximum number of expense records to return. Example: 25."
                            }
                        },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_expense_summary",
                    description = "Use this tool only when the user asks for an expense overview or totals, such as total expenses, paid amount, pending amount, or yearly expense totals. Do not use it for a list of individual expense rows. For expense records use get_expenses. Example questions: 'What are total expenses?', 'Give me the expense summary for this year'.",
                    parameters = new
                    {
                        type = "object",
                        properties = new { },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },

            // ===== POREZI =====
            new
            {
                type = "function",
                function = new
                {
                    name = "get_tax_obligations",
                    description = "Use this tool only when the user asks for tax obligation records or wants tax obligations filtered by year, status, type, or overdue state. Do not use this tool for a total tax overview or tax-payment delay analysis; use get_tax_summary for totals and get_tax_delay_analysis for late-payment behavior. Example questions: 'Show tax obligations for 2026', 'List pending taxes', 'Which taxes are overdue?'.",

                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            year = new
                            {
                                type = "integer",
                                description = "Optional obligation year filter. Example: 2026."
                            },
                            taxStatus = new
                            {
                                type = "string",
                                description = "Optional tax obligation status filter. Allowed values: Pending, Paid, Archived. Example: Pending."
                            },
                            taxType = new
                            {
                                type = "string",
                                description = "Optional tax type filter. Example: VAT, IncomeTax, SocialContribution."
                            },
                            overdueOnly = new
                            {
                                type = "boolean",
                                description = "If true, return only obligations whose due date is in the past and are still pending. Example: true."
                            },
                            limit = new
                            {
                                type = "integer",
                                description = "Optional maximum number of tax obligation records to return. Example: 25."
                            }
                        },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_overdue_taxes",
                    description = "Use this tool only when the user asks for overdue pending tax obligations whose due date has passed. Do not use it for a general tax summary, tax delay analysis, or a record list of all obligations. For obligation records use get_tax_obligations. For aggregate totals use get_tax_summary. Example questions: 'Show overdue taxes', 'Which taxes are past due?'.",
                    parameters = new
                    {
                        type = "object",
                        properties = new { },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_tax_summary",
                    description = "Use this tool only when the user asks for a tax overview or total amounts, such as total obligations, paid amount, unpaid amount, or a yearly tax summary. Do not use it for a list of individual tax obligations, overdue-only filtering, or late-payment analysis. For obligation records use get_tax_obligations. For delay analysis by tax type use get_tax_delay_analysis. Example questions: 'What is the total tax summary?', 'How much is still unpaid this year?'.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            year = new
                            {
                                type = "integer",
                                description = "Optional tax year filter. Example: 2026. If omitted, the tool returns totals across all available obligations."
                            }
                        },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_tax_delay_analysis",
                    description = "Use this tool only when the user asks about delayed tax payments, such as average delay, longest delay, or how often a tax type was paid late. Do not use it for a general tax summary, overdue obligation list, or status-based filtering. For tax obligation records use get_tax_obligations. For total tax amounts use get_tax_summary. Example questions: 'Which taxes are usually paid late?', 'What is the average delay for VAT payments?'.",
                    parameters = new
                    {
                        type = "object",
                        properties = new { },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },

            // ===== PRIHODI I CASH FLOW =====
            new
            {
                type = "function",
                function = new
                {
                    name = "get_monthly_income",
                    description = "Use this tool only when the user asks for invoice-based monthly revenue grouped by month for one year. Do not use it for cash-flow analysis from payment dates, service rankings, or overall income-vs-expense comparisons. For actual paid cash by month use get_actual_cashflow. For income versus expenses use get_income_vs_expenses.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            year = new
                            {
                                type = "integer",
                                description = "Year for which monthly invoice income should be grouped. Example: 2026."
                            }
                        },
                        required = new[] { "year" },
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_income_vs_expenses",
                    description = "Use this tool only when the user asks for a profitability snapshot comparing total income and total expenses for a specific year. Do not use it for monthly income breakdowns, cash-flow by payment date, or service rankings. For monthly invoiced income use get_monthly_income. For actual money collected by payment date use get_actual_cashflow.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            year = new
                            {
                                type = "integer",
                                description = "Year to compare. Example: 2026."
                            }
                        },
                        required = new[] { "year" },
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_top_services",
                    description = "Use this tool only when the user asks which products or services generate the most revenue, or which items are most frequently sold. Do not use it for general invoice lists, client directory lookups, or profitability snapshots. For a single client's purchased services use get_client_service_breakdown. For overall income vs expenses use get_income_vs_expenses.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            top = new
                            {
                                type = "integer",
                                description = "How many services/products to return. Example: 5. If the user does not specify, use 5."
                            },
                            itemType = new
                            {
                                type = "string",
                                description = "Optional item category filter. Allowed values: Product, Service. Example: Service."
                            },
                            year = new
                            {
                                type = "integer",
                                description = "Optional year filter. Example: 2026."
                            },
                            clientId = new
                            {
                                type = "string",
                                description = "Optional client GUID filter. Example: 3a8e6d31-4d70-4c8f-9d1b-7ac32bee2dd6. Use only when the user explicitly asks about one client."
                            }
                        },
                        required = new[] { "top" },
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_actual_cashflow",
                    description = "Use this tool only when the user asks about actual money collected by payment date, monthly cash receipts, or payment timing. Do not use it for invoiced monthly income or an invoice summary. For invoice-based monthly revenue by issue date use get_monthly_income. For overall income vs expenses use get_income_vs_expenses.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            year = new
                            {
                                type = "integer",
                                description = "Year for the cash-flow report. Example: 2026."
                            },
                            month = new
                            {
                                type = "integer",
                                description = "Optional month filter from 1 to 12. Example: 6."
                            }
                        },
                        required = new[] { "year" },
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_client_payment_delay_analysis",
                    description = "Use this tool only when the user asks which clients are late in paying, average payment delay by client, or the longest delay by client. Do not use it for a single invoice lookup, client directory, or tax-payment delay analysis. For tax-payment delay analysis use get_tax_delay_analysis. For overdue invoice records use get_overdue_invoices or get_invoices with overdueOnly=true. Example questions: 'Which clients pay the latest?', 'Show average payment delay by client'.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            top = new
                            {
                                type = "integer",
                                description = "Optional number of clients to return. Example: 5. If omitted, use 5."
                            }
                        },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },

            new
            {
                type = "function",
                function = new
                {
                    name = "get_business_financial_health",
                    description = "Use this tool only when the user asks for a single business-health snapshot combining income, expenses, profitability, unpaid invoices, and pending tax obligations. Do not use it for invoice lists, expense lists, tax obligation records, or detailed client analysis. For invoice records use get_invoices. For expense records use get_expenses. For tax records use get_tax_obligations. Example questions: 'How healthy is the business financially?', 'Give me a business financial health overview'.",
                    parameters = new
                    {
                        type = "object",
                        properties = new { },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_kpo_summary",
                    description = "Use this tool only when the user asks for KPO annual turnover, product/service split, remaining amount to the KPO thresholds, or the overall KPO status for a year. Do not use it for invoice lists, expense lists, tax records, or general business-health questions. Example questions: 'What is my KPO total for 2026?', 'How much do I have left until the 6M KPO limit?'.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            year = new
                            {
                                type = "integer",
                                description = "Optional year filter. If omitted, the current year is used. Example: 2026."
                            }
                        },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_kpo_entries",
                    description = "Use this tool only when the user asks for KPO ledger entries or a list of KPO records for a specific year. Do not use it for annual totals or threshold summaries; use get_kpo_summary for totals and limits. Example questions: 'Show my KPO entries for 2026', 'List KPO records'.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            year = new
                            {
                                type = "integer",
                                description = "Optional year filter. If omitted, the current year is used. Example: 2026."
                            },
                            limit = new
                            {
                                type = "integer",
                                description = "Optional maximum number of KPO entries to return. Example: 25."
                            }
                        },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },

            // ===== OSTALO =====
            new
            {
                type = "function",
                function = new
                {
                    name = "get_upcoming_reminders",
                    description = "Use this tool only when the user asks for pending reminders, upcoming tasks, or the next scheduled reminders. Do not use it for invoices, expenses, taxes, or business profile details. For business profile metadata use get_business_profile_info.",
                    parameters = new
                    {
                        type = "object",
                        properties = new { },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_business_profile_info",
                    description = "Use this tool only when the user asks for static company profile information such as PIB, MB, address, activity code, or business contact details. Do not use it for client lists, invoice data, tax records, or reminders. For financial records use dedicated invoice, expense, and tax tools instead.",
                    parameters = new
                    {
                        type = "object",
                        properties = new { },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_bank_accounts",
                    description = "Use this tool only when the user asks for bank account information, such as account list, default account, iban, or bank details. Do not use it for payments analysis, financial summaries, or client data. For payment timing and cash collection use get_actual_cashflow.",
                    parameters = new
                    {
                        type = "object",
                        properties = new { },
                        required = Array.Empty<string>(),
                        additionalProperties = false
                    }
                }
            }
        };
    }
}
# AI Tool Architecture Analysis

## 1. Current tool inventory

The current schema exposes 19 primary financial-analysis tools, with several overlapping list/filter entry points:

- Client-focused: `get_top_clients`, `get_client_list`, `get_client_service_breakdown`
- Invoice-focused: `get_invoices_by_status`, `get_invoices_by_payment_status`, `get_invoices_by_year`, `get_overdue_invoices`, `get_invoice_summary`, `get_invoice_by_number`
- Expense-focused: `get_expenses_by_status`, `get_expenses_by_year`, `get_expense_summary`
- Tax-focused: `get_tax_obligations_by_year`, `get_tax_obligations_by_status`, `get_overdue_taxes`, `get_tax_summary`, `get_tax_delay_analysis`
- Financial overview: `get_monthly_income`, `get_income_vs_expenses`, `get_top_services`, `get_actual_cashflow`, `get_avg_payment_delay_by_client`
- Business metadata: `get_business_profile_info`, `get_bank_accounts`
- Tasking: `get_upcoming_reminders`

## 2. Detected ambiguities

### A. Filter-only invoice tools overlap heavily
`get_invoices_by_status`, `get_invoices_by_payment_status`, and `get_invoices_by_year` are all list-retrieval tools that answer the same high-level intent: “show me invoice records.” They only differ by a small filter, which creates unnecessary tool-choice ambiguity for the model.

### B. Expense list tools are split by output dimension instead of intent
`get_expenses_by_status` and `get_expenses_by_year` both answer “show expense records” but ask the model to select among them based on a single field filter. This is the same category problem as invoices.

### C. Tax tools have similar or conflicting intents
`get_tax_obligations_by_status`, `get_overdue_taxes`, and `get_tax_summary` all compete for questions about unpaid or delayed tax obligations. The names do not clearly separate:
- status filter list
- overdue-only list
- aggregate overview

The model often has multiple valid choices when the question implies “show me unpaid taxes” or “give me a tax snapshot.”

### D. Tool naming is too generic and inconsistent
Several names use generic wording like `get_*` + a broad noun (`get_tax_summary`, `get_invoice_summary`) without clarifying whether the user wants a record list, an aggregate, or a delayed-payment breakdown. This weakens tool selection determinism.

## 3. Proposed refactor

### Consolidate list/filter tools into one primary tool per domain
- Replace invoice filter tools with one `get_invoice_list` tool.
- Replace expense filter tools with one `get_expense_list` tool.
- Replace tax obligation filter tools with one `get_tax_obligation_list` tool.

This preserves useful capability while reducing model confusion.

### Keep summary and detail tools separate
- `get_invoice_summary` stays distinct from `get_invoice_list`.
- `get_expense_summary` stays distinct from `get_expense_list`.
- `get_tax_summary` stays distinct from `get_tax_obligation_list`.

Summaries are aggregate questions; lists are record-level retrieval questions.

### Improve names and descriptions to encode intent clearly
- `get_tax_obligation_list` should explicitly cover year/status/overdue filtering.
- `get_tax_delay_analysis` should be clearly reserved for payment-delay analysis, not general tax summaries.
- `get_overdue_taxes` should be removed from the schema and replaced with a parameter-based overdue flag on the list tool.

## 4. Expected impact on LLM tool selection

The new tool architecture should:

- reduce the number of near-duplicate tool choices for the same user intent;
- make “overview” vs “list” vs “delay analysis” decisions more deterministic;
- improve parameter accuracy because each tool receives a narrower, more intuitive set of parameters;
- minimize evaluation confusion between similar tax and expense functions.

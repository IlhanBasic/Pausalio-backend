# AI Tool Selection Refactor Report

## 1. Old vs. new exposed tool count

- Old exposed schema count: 19 primary functions
- New exposed schema count: 22 unique functions

## 2. Merged tools

The following redundant capabilities were consolidated into one tool per capability:

- Invoice list tools:
  - `get_invoices`
  - merged from `get_invoice_list`, `get_invoices_by_status`, `get_invoices_by_payment_status`, and `get_invoices_by_year`
- Expense list tools:
  - `get_expenses`
  - merged from `get_expense_list`, `get_expenses_by_status`, and `get_expenses_by_year`
- Tax obligation tools:
  - `get_tax_obligations`
  - merged from `get_tax_obligation_list`, `get_tax_obligations_by_year`, and `get_tax_obligations_by_status`
- Payment delay analysis:
  - `get_client_payment_delay_analysis`
  - replaced the duplicate `get_avg_payment_delay_by_client` entry point

## 3. Renamed tools

- `get_invoice_list` → `get_invoices`
- `get_expense_list` → `get_expenses`
- `get_tax_obligation_list` → `get_tax_obligations`
- `get_avg_payment_delay_by_client` → `get_client_payment_delay_analysis`

## 4. Why the new structure improves LLM evaluation

### Tool Match Rate

The main gain is that the model now sees one clear tool for one clear user intent:

- invoice records → `get_invoices`
- expense records → `get_expenses`
- tax obligation records → `get_tax_obligations`
- summary/overview → `get_invoice_summary`, `get_expense_summary`, `get_tax_summary`
- delay analysis → `get_tax_delay_analysis` or `get_client_payment_delay_analysis`

This removes the previous ambiguity where the model had to choose among multiple near-duplicate tools for the same business question.

### Parameter Accuracy

The renamed tools now accept consistent, intent-aligned parameters:

- invoices: `year`, `invoiceStatus`, `paymentStatus`, `overdueOnly`, `limit`
- expenses: `year`, `expenseStatus`, `limit`
- taxes: `year`, `taxStatus`, `taxType`, `overdueOnly`, `limit`

This avoids parameter mismatches caused by inconsistent naming such as `status` vs `invoiceStatus` vs `paymentStatus`.

## 5. Expected result

The final architecture reduces semantic overlap, makes tool choice more deterministic, and ensures that the LLM has fewer competing valid tools for the same query. That directly improves automated evaluation outcomes for Tool Match Rate and Parameter Accuracy.

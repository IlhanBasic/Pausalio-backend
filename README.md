# Paušalio – Backend

> An information system for managing the business operations of flat-rate (paušal) entrepreneurs in Serbia.

---

## Table of Contents

- [About the Project](#about-the-project)
- [Architecture](#architecture)
- [Technologies & Libraries](#technologies--libraries)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [API Overview](#api-overview)
- [Authentication](#authentication)
- [AI Assistant](#ai-assistant)
- [SignalR Chat](#signalr-chat)
- [Azure Functions – Notifications](#azure-functions--notifications)
- [Invoicing & PDF](#invoicing--pdf)
- [Localization](#localization)
- [Security](#security)
- [CI/CD](#cicd)

---

## About the Project

**Paušalio** is an application designed exclusively for flat-rate taxed entrepreneurs in Serbia (freelancers, IT consultants, designers, translators, etc.). The system automates tax obligation calculations, invoice management, income tracking, and payment reminders — all without the need to hire an accountant.

**Business thresholds:**
- Up to **6,000,000 RSD** per year → entrepreneur can operate independently
- Between 6M and **8,000,000 RSD** → hiring an accountant is mandatory
- Over **8,000,000 RSD** → loss of flat-rate (paušal) status

The application **does not support** LLCs (DOO) or VAT records.

---

## Architecture

The project follows **Clean Architecture** principles, divided into multiple layers:

```
Pausalio.API            → Presentation layer (Controllers, Hubs, Middleware)
Pausalio.Application    → Application layer (Services, DTOs, Validators, Mappings)
Pausalio.Domain         → Domain layer (Entities, Enums)
Pausalio.Infrastructure → Infrastructure layer (DbContext, Repositories, Migrations)
Pausalio.Shared         → Shared layer (Configuration, Enums, Localization)
Pausalio.Functions      → Azure Functions (Notifications)
```

---

## Technologies & Libraries

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 9.0 |
| ORM | Entity Framework Core + Pomelo (MySQL) |
| Authentication | JWT Bearer + Google OAuth2 |
| Real-time | SignalR |
| PDF Generation | QuestPDF |
| Email | MailKit / MimeKit |
| AI Assistant | OpenRouter API (tool calling) |
| Cloud Storage | Azure Blob Storage |
| Serverless Notifications | Azure Functions (Timer Trigger) |
| Mapping | AutoMapper |
| Validation | FluentValidation |
| Logging | Serilog (Console, File, Debug sinks) |
| Encryption | BCrypt.Net-Next, BouncyCastle |
| API Documentation | Swagger / Swashbuckle |

---

## Project Structure

### `Pausalio.API`

The application entry point.

```
Controllers/     → REST API endpoints
Hubs/            → ChatHub (SignalR)
Middlewares/     → BusinessContextMiddleware
Templates/       → HTML email templates (VerifyEmail, InvoiceTemplate, PasswordResetPinEmail, InviteTokenEmail)
Program.cs       → Service and middleware pipeline configuration
```

**Controllers:**
`Auth`, `UserProfile`, `BusinessProfile`, `BusinessInvite`, `Client`, `Invoice`, `InvoiceItem`, `Item`, `Payment`, `TaxObligation`, `Expense`, `Reminder`, `Document`, `BankAccount`, `Chat`, `AIAssistant`, `ExchangeRate`, `File`, `ActivityCode`, `City`, `Country`

---

### `Pausalio.Application`

Business logic and service layer.

```
DTOs/            → Data Transfer Objects organized by domain
Services/        → Interfaces and implementations of all services
Validators/      → FluentValidation validators
Mappings/        → AutoMapper profiles
Helpers/         → AIAssistantPromptHelper, AIToolsDefinition, PasswordHelper, InviteTokenHelper...
```

**Key services:**
- `AIAssistantService` – AI assistant with tool calling support
- `FinancialContextService` – Prepares financial context data for the AI
- `InvoiceExportService` / `PdfFactoryService` – PDF invoice generation
- `ChatService` – Encrypted messages between company members
- `XorEncryptionService` – XOR encryption of chat messages
- `EmailService` / `EmailTemplateService` – Sending emails via MailKit
- `JwtService` – JWT token generation and validation
- `ExchangeRateService` – Currency conversion
- `TaxObligationService` – Tax obligation management
- `ReminderService` – Reminders and obligation calendar

---

### `Pausalio.Domain`

Domain entities with no dependency on infrastructure.

```
Entities/
├── UserProfile.cs
├── BusinessProfile.cs
├── UserBusinessProfile.cs
├── BusinessInvite.cs
├── Client.cs
├── Invoice.cs
├── InvoiceItem.cs
├── Item.cs
├── Payment.cs
├── TaxObligation.cs
├── Expense.cs
├── Reminder.cs
├── Document.cs
├── BankAccount.cs
├── ChatMessage.cs
├── ActivityCode.cs
├── City.cs
└── Country.cs
```

---

### `Pausalio.Infrastructure`

Data access and persistence.

```
Persistence/
├── PausalioDbContext.cs
└── Configurations/    → EF Core Fluent API configurations per entity

Repositories/
├── Implementations/   → Concrete repository implementations
│   └── UnitOfWork.cs
└── Interfaces/        → Repository interfaces

Extensions/
└── IQueryableExtensions.cs

Migrations/            → EF Core migrations
```

---

### `Pausalio.Shared`

Shared code used across all layers.

```
Configuration/     → Settings classes (JWT, SMTP, Azure Blob, Google Auth, OpenRouter, ExchangeRate, Url)
Enums/             → ClientType, Currency, DocumentType, InvoiceStatus, TaxObligationType, UserRole...
Localization/      → Resources for Serbian Latin and Serbian Cyrillic scripts
```

---

### `Pausalio.Functions`

Azure Functions for automated notifications.

```
ReminderNotificationFunction.cs   → Timer trigger – runs every day at 10:00 AM
PausalioFunctionsDbContext.cs     → Direct database access
Templates/
└── ReminderEmail.html            → HTML email template for reminders
```

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- MySQL server
- Azure Storage Account (or Azurite locally for development)
- SMTP credentials (e.g. Gmail)
- OpenRouter API key (for the AI assistant)

### Steps

```bash
# 1. Clone the repository
git clone <repo-url>
cd Pausalio

# 2. Configure appsettings
# Fill in appsettings.Development.json (see the Configuration section below)

# 3. Apply migrations
cd Pausalio.API
dotnet ef database update

# 4. Run the API
dotnet run

# 5. (Optional) Run Azure Functions locally
cd ../Pausalio.Functions
func start
```

Swagger UI available at: `https://localhost:{port}/swagger`

---

## Configuration

Example `appsettings.json` structure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=pausalio;User=...;Password=...;"
  },
  "JwtSettings": {
    "SecretKey": "...",
    "Issuer": "...",
    "Audience": "...",
    "ExpirationMinutes": 60
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "...",
    "Password": "...",
    "FromName": "Paušalio"
  },
  "AzureBlobStorageSettings": {
    "ConnectionString": "...",
    "ContainerName": "..."
  },
  "GoogleAuthSettings": {
    "ClientId": "...",
    "ClientSecret": "..."
  },
  "OpenRouterSettings": {
    "ApiKey": "...",
    "BaseUrl": "https://openrouter.ai/api/v1",
    "Model": "..."
  },
  "ExchangeRateSettings": {
    "BaseUrl": "..."
  },
  "UrlSettings": {
    "FrontendUrl": "https://...",
    "BackendUrl":"..."
  },
  "Encryption":{
    "Key":"..."
  },
  "Authentication": {
    "Google": {
      "ClientId": "...",
      "ClientSecret": "..."
    }
  },
}
```

---

## API Overview

All endpoints are protected by JWT authentication (except auth endpoints). Swagger documentation available at `/swagger`.

| Controller | Description |
|---|---|
| `/api/auth` | Registration, login, Google OAuth, email verification, password reset |
| `/api/user-profile` | User profile management |
| `/api/business-profile` | Business profile management |
| `/api/business-invite` | Inviting and managing assistants |
| `/api/client` | Client records |
| `/api/invoice` | Creating, sending, and managing invoices |
| `/api/invoice-item` | Invoice line items |
| `/api/item` | Service/product catalog |
| `/api/payment` | Payment records |
| `/api/tax-obligation` | Tax obligations |
| `/api/expense` | Expense records |
| `/api/reminder` | Reminders and calendar |
| `/api/document` | Document archive |
| `/api/bank-account` | Bank accounts |
| `/api/chat` | Chat messages between company members |
| `/api/ai-assistant` | AI assistant with tool calling |
| `/api/exchange-rate` | Currency conversion |
| `/api/file` | File upload and download (Azure Blob) |
| `/api/activity-code` | Business activity codes |
| `/api/city` | Cities / municipalities |
| `/api/country` | Countries |

---

## Authentication

The system supports two login methods:

- **Email / password** – Registration with email verification and PIN-based password reset
- **Google OAuth2** – Sign in with a Google account

**User roles:**
- `Owner` – Business owner, full access
- `Assistant` – Limited access, receives an invitation from the owner

**Token flow:**
1. User logs in → receives a JWT access token
2. Token is sent in the `Authorization: Bearer <token>` header
3. `BusinessContextMiddleware` automatically resolves the business context from the token

---

## AI Assistant

The AI assistant has direct access to the company's financial data via **tool calling**.

Available tools:
- Invoice and income overview by client, period, and currency
- Tax obligation analysis
- Expense statistics
- Payment and outstanding balance insights

**Example query:**
> _"Which country do my best clients come from and how much have I earned from the top 3?"_

Configuration is located in:
- `AIAssistantPromptHelper.cs` – System prompt
- `AIToolsDefinition.cs` – Tool definitions
- `FinancialContextService.cs` – Data collection for context

---

## SignalR Chat

Real-time communication between the owner and assistants within the same company.

- **Hub:** `/hubs/chat` (`ChatHub.cs`)
- **Encryption:** Messages are encrypted before being stored in the database via `XorEncryptionService` — plain text is never persisted
- **Message status:** `Sent`, `Delivered`, `Read` (`MessageStatus` enum)

---

## Azure Functions – Notifications

`ReminderNotificationFunction` runs automatically every day at **09:00 AM** (CRON: `0 0 9 * * *`).

- Reads all active reminders for the current day from the database
- Sends an email to the user via MailKit using the `ReminderEmail.html` template
- Independent function with its own `DbContext` and database connection

---

## Invoicing & PDF

Invoices are generated as PDF documents using the **QuestPDF** library with the Lato font.

Invoice creation flow:
1. User creates an invoice via `POST /api/invoice`
2. Line items are added via the `InvoiceItem` endpoint
3. PDF is generated on demand and can be downloaded or sent directly to the client by email
4. Invoice is automatically numbered and archived

Relevant services: `InvoiceService`, `InvoiceExportService`, `PdfFactoryService`

---

## Localization

The backend supports Serbian in both scripts:

| Culture | Script |
|---|---|
| `sr-Latn` | Serbian Latin |
| `sr-Cyrl` | Serbian Cyrillic |

Resources are located in `Pausalio.Shared/Localization/`:
- `Resources.resx` – Default (Latin script)
- `Resources.sr-Cyrl.resx` – Cyrillic variant

Implemented via `ILocalizationHelper` / `LocalizationHelper`.

---

## Security

- **JWT** authentication with short token expiration
- **BCrypt** password hashing
- **Google OAuth2** for social login
- **XOR encryption** of chat messages (plain text is never stored in the database)
- **BouncyCastle** cryptography library
- **FluentValidation** on all incoming DTOs
- **HTTPS** enforced in production
- Multi-factor authentication via email verification and PIN reset mechanism
- Assistant invitations via single-use tokens

---

## CI/CD

GitHub Actions workflow configuration is located in `.github/workflows/`.

Publish profiles for Azure deployment are available at:
- `Pausalio.API/Properties/PublishProfiles/`
- `Pausalio.Functions/Properties/PublishProfiles/`

---

*Paušalio © 2026 – All rights reserved*
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Pausalio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ActivityCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityCodes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PostalCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FirstName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProfilePicture = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsEmailVerified = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    EmailVerificationToken = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailVerificationTokenExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordResetTokenExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BusinessProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BusinessName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PIB = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MB = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActivityCodeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Website = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompanyLogo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessProfiles", x => x.Id);
                    table.CheckConstraint("CK_BusinessProfiles_Dates", "UpdatedAt IS NULL OR UpdatedAt >= CreatedAt");
                    table.CheckConstraint("CK_BusinessProfiles_IsActive", "IsActive IN (0,1)");
                    table.ForeignKey(
                        name: "FK_BusinessProfiles_ActivityCodes_ActivityCodeId",
                        column: x => x.ActivityCodeId,
                        principalTable: "ActivityCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BusinessProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BankName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountNumber = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IBAN = table.Column<string>(type: "varchar(34)", maxLength: 34, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SWIFT = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                    table.CheckConstraint("CK_BankAccounts_AccountOrIBAN", "(AccountNumber IS NOT NULL AND CHAR_LENGTH(AccountNumber) > 0) OR (IBAN IS NOT NULL AND CHAR_LENGTH(IBAN) > 0)");
                    table.CheckConstraint("CK_BankAccounts_Dates", "UpdatedAt IS NULL OR UpdatedAt >= CreatedAt");
                    table.ForeignKey(
                        name: "FK_BankAccounts_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BusinessInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BusinessProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Token = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsUsed = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessInvites_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessInvites_UserProfiles_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BusinessProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClientType = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PIB = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MB = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                    table.CheckConstraint("CK_Clients_Dates", "UpdatedAt IS NULL OR UpdatedAt >= CreatedAt");
                    table.CheckConstraint("CK_Clients_IsActive", "IsActive IN (0,1)");
                    table.ForeignKey(
                        name: "FK_Clients_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Clients_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BusinessProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    DocumentNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FilePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.CheckConstraint("CK_Documents_Dates", "DeletedAt IS NULL OR DeletedAt >= UploadedAt");
                    table.CheckConstraint("CK_Documents_IsDeleted", "IsDeleted IN (0,1)");
                    table.ForeignKey(
                        name: "FK_Documents_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BusinessProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BusinessProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReminderType = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                    table.CheckConstraint("CK_Reminders_Dates", "DeletedAt IS NULL OR DeletedAt >= CreatedAt");
                    table.CheckConstraint("CK_Reminders_IsCompleted", "IsCompleted IN (0,1)");
                    table.CheckConstraint("CK_Reminders_IsDeleted", "IsDeleted IN (0,1)");
                    table.ForeignKey(
                        name: "FK_Reminders_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserBusinessProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BusinessProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBusinessProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBusinessProfiles_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBusinessProfiles_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BusinessProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClientId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    InvoiceNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    AmountToPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    IssueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    InvoiceStatus = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    TotalAmountRSD = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false, defaultValue: 1m),
                    ReferenceNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoices_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    InvoiceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BusinessProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferenceNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    PaymentId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.CheckConstraint("CK_Expenses_Dates", "DeletedAt IS NULL OR DeletedAt >= CreatedAt");
                    table.CheckConstraint("CK_Expenses_IsDeleted", "IsDeleted IN (0,1)");
                    table.CheckConstraint("CK_Expenses_Status", "Status IN (1,2,3)");
                    table.ForeignKey(
                        name: "FK_Expenses_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BusinessProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PaymentType = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TaxObligationId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ExpenseId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    AmountRSD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TaxObligations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BusinessProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PaymentId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxObligations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxObligations_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaxObligations_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "ActivityCodes",
                columns: new[] { "Id", "Code", "Description" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "IT01", "Računarsko programiranje" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "IT02", "Razvoj veb aplikacija" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "IT03", "Razvoj mobilnih aplikacija" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "IT04", "Softversko inženjerstvo" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "IT05", "Administracija baza podataka" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "IT06", "Konsalting u oblasti informacionih tehnologija" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "IT07", "Razvoj video igara" },
                    { new Guid("00000000-0000-0000-0000-000000000008"), "IT08", "Testiranje softvera" },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "DS01", "Analiza podataka" },
                    { new Guid("00000000-0000-0000-0000-000000000010"), "DS02", "Mašinsko učenje" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "DS03", "Poslovna inteligencija" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "GD01", "Grafički dizajn" },
                    { new Guid("00000000-0000-0000-0000-000000000013"), "GD02", "UI/UX dizajn" },
                    { new Guid("00000000-0000-0000-0000-000000000014"), "GD03", "Dizajn logotipa i brendiranje" },
                    { new Guid("00000000-0000-0000-0000-000000000015"), "GD04", "Ilustracija i crtež" },
                    { new Guid("00000000-0000-0000-0000-000000000016"), "GD05", "Dizajn štampanih materijala" },
                    { new Guid("00000000-0000-0000-0000-000000000017"), "WR01", "Pisanje sadržaja za veb sajtove" },
                    { new Guid("00000000-0000-0000-0000-000000000018"), "WR02", "Kopirajting" },
                    { new Guid("00000000-0000-0000-0000-000000000019"), "WR03", "Tehničko pisanje" },
                    { new Guid("00000000-0000-0000-0000-000000000020"), "TR01", "Prevodenje tekstova" },
                    { new Guid("00000000-0000-0000-0000-000000000021"), "TR02", "Lektura i korekcija teksta" },
                    { new Guid("00000000-0000-0000-0000-000000000022"), "TR03", "Transkribovanje audio i video zapisa" },
                    { new Guid("00000000-0000-0000-0000-000000000023"), "MK01", "Digitalni marketing" },
                    { new Guid("00000000-0000-0000-0000-000000000024"), "MK02", "Upravljanje društvenim mrežama" },
                    { new Guid("00000000-0000-0000-0000-000000000025"), "MK03", "SEO optimizacija" },
                    { new Guid("00000000-0000-0000-0000-000000000026"), "MK04", "E-mail marketing" },
                    { new Guid("00000000-0000-0000-0000-000000000027"), "MK05", "Upravljanje Google Ads kampanjama" },
                    { new Guid("00000000-0000-0000-0000-000000000028"), "VA01", "Virtuelna asistenca" },
                    { new Guid("00000000-0000-0000-0000-000000000029"), "VA02", "Administrativna podrška" },
                    { new Guid("00000000-0000-0000-0000-000000000030"), "VA03", "Unošenje podataka" },
                    { new Guid("00000000-0000-0000-0000-000000000031"), "VA04", "Online istraživanje" },
                    { new Guid("00000000-0000-0000-0000-000000000032"), "AV01", "Video montaža" },
                    { new Guid("00000000-0000-0000-0000-000000000033"), "AV02", "Audio produkcija" },
                    { new Guid("00000000-0000-0000-0000-000000000034"), "AV03", "Fotografija" },
                    { new Guid("00000000-0000-0000-0000-000000000035"), "AV04", "Animacija" },
                    { new Guid("00000000-0000-0000-0000-000000000036"), "AV05", "Snimanje i produkcija podkasta" },
                    { new Guid("00000000-0000-0000-0000-000000000037"), "ED01", "Online podučavanje i mentoring" },
                    { new Guid("00000000-0000-0000-0000-000000000038"), "ED02", "Izrada online kurseva" },
                    { new Guid("00000000-0000-0000-0000-000000000039"), "ED03", "Poslovni konsalting" },
                    { new Guid("00000000-0000-0000-0000-000000000040"), "ED04", "Karijerno savetovanje" },
                    { new Guid("00000000-0000-0000-0000-000000000041"), "ET01", "Online prodaja proizvoda" },
                    { new Guid("00000000-0000-0000-0000-000000000042"), "ET02", "Dropshipping" },
                    { new Guid("00000000-0000-0000-0000-000000000043"), "ET03", "Upravljanje e-trgovinom" },
                    { new Guid("00000000-0000-0000-0000-000000000044"), "ET04", "Afilijet marketing" },
                    { new Guid("00000000-0000-0000-0000-000000000045"), "FK01", "Online knjigovodstvo" },
                    { new Guid("00000000-0000-0000-0000-000000000046"), "FK02", "Finansijsko savetovanje" },
                    { new Guid("00000000-0000-0000-0000-000000000047"), "FK03", "Poresko savetovanje" },
                    { new Guid("00000000-0000-0000-0000-000000000048"), "FK04", "Priprema poreskih prijava" },
                    { new Guid("00000000-0000-0000-0000-000000000049"), "PR01", "Pravno savetovanje" },
                    { new Guid("00000000-0000-0000-0000-000000000050"), "PR02", "Izrada pravnih dokumenata" },
                    { new Guid("00000000-0000-0000-0000-000000000051"), "TE01", "CAD projektovanje" },
                    { new Guid("00000000-0000-0000-0000-000000000052"), "TE02", "Arhitektonsko projektovanje" },
                    { new Guid("00000000-0000-0000-0000-000000000053"), "TE03", "3D modelovanje" },
                    { new Guid("00000000-0000-0000-0000-000000000054"), "ZH01", "Online psihološko savetovanje" },
                    { new Guid("00000000-0000-0000-0000-000000000055"), "ZH02", "Nutricionističko savetovanje" },
                    { new Guid("00000000-0000-0000-0000-000000000056"), "ZH03", "Fitnes trenering online" },
                    { new Guid("00000000-0000-0000-0000-000000000057"), "OS01", "Planiranje događaja online" },
                    { new Guid("00000000-0000-0000-0000-000000000058"), "OS02", "Online istraživanje tržišta" },
                    { new Guid("00000000-0000-0000-0000-000000000059"), "OS03", "Recenziranje proizvoda i usluga" },
                    { new Guid("00000000-0000-0000-0000-000000000060"), "OS04", "Moderacija online sadržaja" }
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "Name", "PostalCode" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "Beograd", "11000" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "Novi Sad", "21000" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "Niš", "18000" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "Kragujevac", "34000" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "Subotica", "24000" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "Zrenjanin", "23000" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "Pančevo", "26000" },
                    { new Guid("00000000-0000-0000-0000-000000000008"), "Čačak", "32000" },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "Novi Pazar", "36300" },
                    { new Guid("00000000-0000-0000-0000-000000000010"), "Kraljevo", "36000" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "Smederevo", "11300" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "Leskovac", "16000" },
                    { new Guid("00000000-0000-0000-0000-000000000013"), "Valjevo", "14000" },
                    { new Guid("00000000-0000-0000-0000-000000000014"), "Kruševac", "37000" },
                    { new Guid("00000000-0000-0000-0000-000000000015"), "Vranje", "17500" },
                    { new Guid("00000000-0000-0000-0000-000000000016"), "Šabac", "15000" },
                    { new Guid("00000000-0000-0000-0000-000000000017"), "Užice", "31000" },
                    { new Guid("00000000-0000-0000-0000-000000000018"), "Sombor", "25000" },
                    { new Guid("00000000-0000-0000-0000-000000000019"), "Požarevac", "12000" },
                    { new Guid("00000000-0000-0000-0000-000000000020"), "Pirot", "18300" },
                    { new Guid("00000000-0000-0000-0000-000000000021"), "Zaječar", "19000" },
                    { new Guid("00000000-0000-0000-0000-000000000022"), "Kikinda", "23300" },
                    { new Guid("00000000-0000-0000-0000-000000000023"), "Sremska Mitrovica", "22000" },
                    { new Guid("00000000-0000-0000-0000-000000000024"), "Jagodina", "35000" },
                    { new Guid("00000000-0000-0000-0000-000000000025"), "Vršac", "26300" },
                    { new Guid("00000000-0000-0000-0000-000000000026"), "Bor", "19210" },
                    { new Guid("00000000-0000-0000-0000-000000000027"), "Prokuplje", "18400" },
                    { new Guid("00000000-0000-0000-0000-000000000028"), "Loznica", "15300" },
                    { new Guid("00000000-0000-0000-0000-000000000029"), "Smederevska Palanka", "11420" },
                    { new Guid("00000000-0000-0000-0000-000000000030"), "Bačka Palanka", "21400" },
                    { new Guid("00000000-0000-0000-0000-000000000031"), "Temerin", "21235" },
                    { new Guid("00000000-0000-0000-0000-000000000032"), "Bečej", "21220" },
                    { new Guid("00000000-0000-0000-0000-000000000033"), "Senta", "24400" },
                    { new Guid("00000000-0000-0000-0000-000000000034"), "Apatin", "25260" },
                    { new Guid("00000000-0000-0000-0000-000000000035"), "Ada", "24430" },
                    { new Guid("00000000-0000-0000-0000-000000000036"), "Alibunar", "26310" },
                    { new Guid("00000000-0000-0000-0000-000000000037"), "Bač", "21420" },
                    { new Guid("00000000-0000-0000-0000-000000000038"), "Bačka Topola", "24300" },
                    { new Guid("00000000-0000-0000-0000-000000000039"), "Bela Crkva", "26340" },
                    { new Guid("00000000-0000-0000-0000-000000000040"), "Beočin", "21300" },
                    { new Guid("00000000-0000-0000-0000-000000000041"), "Čoka", "23320" },
                    { new Guid("00000000-0000-0000-0000-000000000042"), "Inđija", "22320" },
                    { new Guid("00000000-0000-0000-0000-000000000043"), "Irig", "22406" },
                    { new Guid("00000000-0000-0000-0000-000000000044"), "Kanjiža", "24420" },
                    { new Guid("00000000-0000-0000-0000-000000000045"), "Kovačica", "26210" },
                    { new Guid("00000000-0000-0000-0000-000000000046"), "Kovin", "26220" },
                    { new Guid("00000000-0000-0000-0000-000000000047"), "Mali Iđoš", "24320" },
                    { new Guid("00000000-0000-0000-0000-000000000048"), "Nova Crnja", "23218" },
                    { new Guid("00000000-0000-0000-0000-000000000049"), "Novi Bečej", "23272" },
                    { new Guid("00000000-0000-0000-0000-000000000050"), "Novi Kneževac", "23330" },
                    { new Guid("00000000-0000-0000-0000-000000000051"), "Odžaci", "25250" },
                    { new Guid("00000000-0000-0000-0000-000000000052"), "Opovo", "26204" },
                    { new Guid("00000000-0000-0000-0000-000000000053"), "Pećinci", "22410" },
                    { new Guid("00000000-0000-0000-0000-000000000054"), "Plandište", "26360" },
                    { new Guid("00000000-0000-0000-0000-000000000055"), "Ruma", "22400" },
                    { new Guid("00000000-0000-0000-0000-000000000056"), "Sečanj", "23240" },
                    { new Guid("00000000-0000-0000-0000-000000000057"), "Srbobran", "21480" },
                    { new Guid("00000000-0000-0000-0000-000000000058"), "Sremski Karlovci", "21205" },
                    { new Guid("00000000-0000-0000-0000-000000000059"), "Stara Pazova", "22300" },
                    { new Guid("00000000-0000-0000-0000-000000000060"), "Šid", "22240" },
                    { new Guid("00000000-0000-0000-0000-000000000061"), "Titel", "21240" },
                    { new Guid("00000000-0000-0000-0000-000000000062"), "Vrbas", "21460" },
                    { new Guid("00000000-0000-0000-0000-000000000063"), "Žabalj", "21230" },
                    { new Guid("00000000-0000-0000-0000-000000000064"), "Prijepolje", "31300" },
                    { new Guid("00000000-0000-0000-0000-000000000065"), "Priboj", "31330" },
                    { new Guid("00000000-0000-0000-0000-000000000066"), "Nova Varoš", "31320" },
                    { new Guid("00000000-0000-0000-0000-000000000067"), "Aranđelovac", "34300" },
                    { new Guid("00000000-0000-0000-0000-000000000068"), "Bajina Bašta", "31250" },
                    { new Guid("00000000-0000-0000-0000-000000000069"), "Gornji Milanovac", "32300" },
                    { new Guid("00000000-0000-0000-0000-000000000070"), "Ivanjica", "32250" },
                    { new Guid("00000000-0000-0000-0000-000000000071"), "Knić", "34240" },
                    { new Guid("00000000-0000-0000-0000-000000000072"), "Koceljeva", "15220" },
                    { new Guid("00000000-0000-0000-0000-000000000073"), "Kosjerić", "31260" },
                    { new Guid("00000000-0000-0000-0000-000000000074"), "Krupanj", "15314" },
                    { new Guid("00000000-0000-0000-0000-000000000075"), "Lajkovac", "14224" },
                    { new Guid("00000000-0000-0000-0000-000000000076"), "Lapovo", "34220" },
                    { new Guid("00000000-0000-0000-0000-000000000077"), "Lučani", "32230" },
                    { new Guid("00000000-0000-0000-0000-000000000078"), "Mionica", "14242" },
                    { new Guid("00000000-0000-0000-0000-000000000079"), "Mladenovac", "11400" },
                    { new Guid("00000000-0000-0000-0000-000000000080"), "Osečina", "14253" },
                    { new Guid("00000000-0000-0000-0000-000000000081"), "Paraćin", "35250" },
                    { new Guid("00000000-0000-0000-0000-000000000082"), "Rača", "34210" },
                    { new Guid("00000000-0000-0000-0000-000000000083"), "Raška", "36350" },
                    { new Guid("00000000-0000-0000-0000-000000000084"), "Rekovac", "35260" },
                    { new Guid("00000000-0000-0000-0000-000000000085"), "Sjenica", "36310" },
                    { new Guid("00000000-0000-0000-0000-000000000086"), "Trstenik", "37240" },
                    { new Guid("00000000-0000-0000-0000-000000000087"), "Tutin", "36320" },
                    { new Guid("00000000-0000-0000-0000-000000000088"), "Ub", "14210" },
                    { new Guid("00000000-0000-0000-0000-000000000089"), "Varvarin", "37260" },
                    { new Guid("00000000-0000-0000-0000-000000000091"), "Aleksandrovac", "37230" },
                    { new Guid("00000000-0000-0000-0000-000000000093"), "Ćićevac", "37210" },
                    { new Guid("00000000-0000-0000-0000-000000000094"), "Despotovac", "35213" },
                    { new Guid("00000000-0000-0000-0000-000000000095"), "Požega", "31210" },
                    { new Guid("00000000-0000-0000-0000-000000000096"), "Arilje", "31230" },
                    { new Guid("00000000-0000-0000-0000-000000000097"), "Čajetina", "31310" },
                    { new Guid("00000000-0000-0000-0000-000000000098"), "Aleksinac", "18220" },
                    { new Guid("00000000-0000-0000-0000-000000000099"), "Babušnica", "18330" },
                    { new Guid("00000000-0000-0000-0000-000000000100"), "Bela Palanka", "18310" },
                    { new Guid("00000000-0000-0000-0000-000000000101"), "Blace", "18420" },
                    { new Guid("00000000-0000-0000-0000-000000000102"), "Bojnik", "16205" },
                    { new Guid("00000000-0000-0000-0000-000000000103"), "Bosilegrad", "17540" },
                    { new Guid("00000000-0000-0000-0000-000000000104"), "Brus", "37220" },
                    { new Guid("00000000-0000-0000-0000-000000000105"), "Bujanovac", "17520" },
                    { new Guid("00000000-0000-0000-0000-000000000106"), "Crna Trava", "16215" },
                    { new Guid("00000000-0000-0000-0000-000000000107"), "Dimitrovgrad", "18320" },
                    { new Guid("00000000-0000-0000-0000-000000000108"), "Doljevac", "18410" },
                    { new Guid("00000000-0000-0000-0000-000000000109"), "Gadžin Han", "18240" },
                    { new Guid("00000000-0000-0000-0000-000000000110"), "Golubac", "12223" },
                    { new Guid("00000000-0000-0000-0000-000000000111"), "Kladovo", "19320" },
                    { new Guid("00000000-0000-0000-0000-000000000112"), "Knjaževac", "19350" },
                    { new Guid("00000000-0000-0000-0000-000000000113"), "Kuršumlija", "18430" },
                    { new Guid("00000000-0000-0000-0000-000000000114"), "Lebane", "16230" },
                    { new Guid("00000000-0000-0000-0000-000000000115"), "Majdanpek", "19250" },
                    { new Guid("00000000-0000-0000-0000-000000000116"), "Malo Crniće", "12312" },
                    { new Guid("00000000-0000-0000-0000-000000000117"), "Medveđa", "16240" },
                    { new Guid("00000000-0000-0000-0000-000000000118"), "Merošina", "18250" },
                    { new Guid("00000000-0000-0000-0000-000000000119"), "Negotin", "19300" },
                    { new Guid("00000000-0000-0000-0000-000000000120"), "Preševo", "17523" },
                    { new Guid("00000000-0000-0000-0000-000000000121"), "Ražanj", "37215" },
                    { new Guid("00000000-0000-0000-0000-000000000122"), "Sokobanja", "18230" },
                    { new Guid("00000000-0000-0000-0000-000000000123"), "Surdulica", "17530" },
                    { new Guid("00000000-0000-0000-0000-000000000124"), "Svrljig", "18360" },
                    { new Guid("00000000-0000-0000-0000-000000000125"), "Trgovište", "17525" },
                    { new Guid("00000000-0000-0000-0000-000000000126"), "Velika Plana", "11320" },
                    { new Guid("00000000-0000-0000-0000-000000000127"), "Veliko Gradište", "12220" },
                    { new Guid("00000000-0000-0000-0000-000000000128"), "Vladimirci", "15214" },
                    { new Guid("00000000-0000-0000-0000-000000000129"), "Vlasotince", "16210" },
                    { new Guid("00000000-0000-0000-0000-000000000130"), "Žabari", "12374" },
                    { new Guid("00000000-0000-0000-0000-000000000131"), "Žagubica", "12320" },
                    { new Guid("00000000-0000-0000-0000-000000000132"), "Žitorađa", "18412" },
                    { new Guid("00000000-0000-0000-0000-000000000133"), "Sremska Kamenica", "21208" },
                    { new Guid("00000000-0000-0000-0000-000000000134"), "Futog", "21410" },
                    { new Guid("00000000-0000-0000-0000-000000000135"), "Kačarevo", "26212" },
                    { new Guid("00000000-0000-0000-0000-000000000136"), "Beška", "22324" },
                    { new Guid("00000000-0000-0000-0000-000000000137"), "Petrovaradin", "21131" },
                    { new Guid("00000000-0000-0000-0000-000000000138"), "Stara Moravica", "24344" },
                    { new Guid("00000000-0000-0000-0000-000000000139"), "Bački Petrovac", "21470" },
                    { new Guid("00000000-0000-0000-0000-000000000140"), "Kula", "25230" },
                    { new Guid("00000000-0000-0000-0000-000000000142"), "Barajevo", "11460" },
                    { new Guid("00000000-0000-0000-0000-000000000143"), "Čukarica", "11030" },
                    { new Guid("00000000-0000-0000-0000-000000000144"), "Grocka", "11306" },
                    { new Guid("00000000-0000-0000-0000-000000000145"), "Lazarevac", "11550" },
                    { new Guid("00000000-0000-0000-0000-000000000147"), "Novi Beograd", "11070" },
                    { new Guid("00000000-0000-0000-0000-000000000148"), "Obrenovac", "11500" },
                    { new Guid("00000000-0000-0000-0000-000000000149"), "Palilula", "11000" },
                    { new Guid("00000000-0000-0000-0000-000000000150"), "Rakovica", "11090" },
                    { new Guid("00000000-0000-0000-0000-000000000151"), "Savski venac", "11000" },
                    { new Guid("00000000-0000-0000-0000-000000000153"), "Stari grad", "11000" },
                    { new Guid("00000000-0000-0000-0000-000000000154"), "Surčin", "11271" },
                    { new Guid("00000000-0000-0000-0000-000000000155"), "Voždovac", "11010" },
                    { new Guid("00000000-0000-0000-0000-000000000156"), "Vračar", "11000" },
                    { new Guid("00000000-0000-0000-0000-000000000157"), "Zemun", "11080" },
                    { new Guid("00000000-0000-0000-0000-000000000158"), "Zvezdara", "11050" },
                    { new Guid("00000000-0000-0000-0000-000000000160"), "Ljig", "14240" },
                    { new Guid("00000000-0000-0000-0000-000000000161"), "Mali Zvornik", "15318" },
                    { new Guid("00000000-0000-0000-0000-000000000163"), "Pribojska Banja", "31335" },
                    { new Guid("00000000-0000-0000-0000-000000000164"), "Sjenička Banja", "36313" },
                    { new Guid("00000000-0000-0000-0000-000000000165"), "Sopot", "11450" }
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "AF", "Afganistan" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "AL", "Albanija" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "DZ", "Alžir" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "AD", "Andora" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "AO", "Angola" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "AG", "Antigva i Barbuda" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "AR", "Argentina" },
                    { new Guid("00000000-0000-0000-0000-000000000008"), "AM", "Armenija" },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "AU", "Australija" },
                    { new Guid("00000000-0000-0000-0000-000000000010"), "AT", "Austrija" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "AZ", "Azerbejdžan" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "BS", "Bahami" },
                    { new Guid("00000000-0000-0000-0000-000000000013"), "BH", "Bahrein" },
                    { new Guid("00000000-0000-0000-0000-000000000014"), "BD", "Bangladeš" },
                    { new Guid("00000000-0000-0000-0000-000000000015"), "BB", "Barbados" },
                    { new Guid("00000000-0000-0000-0000-000000000016"), "BE", "Belgija" },
                    { new Guid("00000000-0000-0000-0000-000000000017"), "BZ", "Belize" },
                    { new Guid("00000000-0000-0000-0000-000000000018"), "BY", "Belorusija" },
                    { new Guid("00000000-0000-0000-0000-000000000019"), "BJ", "Benin" },
                    { new Guid("00000000-0000-0000-0000-000000000020"), "BW", "Bocvana" },
                    { new Guid("00000000-0000-0000-0000-000000000021"), "BO", "Bolivija" },
                    { new Guid("00000000-0000-0000-0000-000000000022"), "BA", "Bosna i Hercegovina" },
                    { new Guid("00000000-0000-0000-0000-000000000023"), "BR", "Brazil" },
                    { new Guid("00000000-0000-0000-0000-000000000024"), "BN", "Brunej" },
                    { new Guid("00000000-0000-0000-0000-000000000025"), "BG", "Bugarska" },
                    { new Guid("00000000-0000-0000-0000-000000000026"), "BF", "Burkina Faso" },
                    { new Guid("00000000-0000-0000-0000-000000000027"), "BI", "Burundi" },
                    { new Guid("00000000-0000-0000-0000-000000000028"), "BT", "Butan" },
                    { new Guid("00000000-0000-0000-0000-000000000030"), "ME", "Crna Gora" },
                    { new Guid("00000000-0000-0000-0000-000000000031"), "TD", "Čad" },
                    { new Guid("00000000-0000-0000-0000-000000000032"), "CZ", "Češka" },
                    { new Guid("00000000-0000-0000-0000-000000000033"), "CL", "Čile" },
                    { new Guid("00000000-0000-0000-0000-000000000034"), "DK", "Danska" },
                    { new Guid("00000000-0000-0000-0000-000000000035"), "CD", "Demokratska Republika Kongo" },
                    { new Guid("00000000-0000-0000-0000-000000000036"), "DM", "Dominika" },
                    { new Guid("00000000-0000-0000-0000-000000000037"), "DO", "Dominikanska Republika" },
                    { new Guid("00000000-0000-0000-0000-000000000038"), "DJ", "Džibuti" },
                    { new Guid("00000000-0000-0000-0000-000000000039"), "EG", "Egipat" },
                    { new Guid("00000000-0000-0000-0000-000000000040"), "EC", "Ekvador" },
                    { new Guid("00000000-0000-0000-0000-000000000041"), "GQ", "Ekvatorijalna Gvineja" },
                    { new Guid("00000000-0000-0000-0000-000000000042"), "ER", "Eritreja" },
                    { new Guid("00000000-0000-0000-0000-000000000043"), "EE", "Estonija" },
                    { new Guid("00000000-0000-0000-0000-000000000044"), "SZ", "Esvatini" },
                    { new Guid("00000000-0000-0000-0000-000000000045"), "ET", "Etiopija" },
                    { new Guid("00000000-0000-0000-0000-000000000046"), "FJ", "Fiji" },
                    { new Guid("00000000-0000-0000-0000-000000000047"), "PH", "Filipini" },
                    { new Guid("00000000-0000-0000-0000-000000000048"), "FI", "Finska" },
                    { new Guid("00000000-0000-0000-0000-000000000049"), "FR", "Francuska" },
                    { new Guid("00000000-0000-0000-0000-000000000050"), "GA", "Gabon" },
                    { new Guid("00000000-0000-0000-0000-000000000051"), "GM", "Gambija" },
                    { new Guid("00000000-0000-0000-0000-000000000052"), "GH", "Gana" },
                    { new Guid("00000000-0000-0000-0000-000000000053"), "GR", "Grčka" },
                    { new Guid("00000000-0000-0000-0000-000000000054"), "GD", "Grenada" },
                    { new Guid("00000000-0000-0000-0000-000000000055"), "GE", "Gruzija" },
                    { new Guid("00000000-0000-0000-0000-000000000056"), "GY", "Gvajana" },
                    { new Guid("00000000-0000-0000-0000-000000000057"), "GT", "Gvatemala" },
                    { new Guid("00000000-0000-0000-0000-000000000058"), "GN", "Gvineja" },
                    { new Guid("00000000-0000-0000-0000-000000000059"), "GW", "Gvineja Bisau" },
                    { new Guid("00000000-0000-0000-0000-000000000060"), "HT", "Haiti" },
                    { new Guid("00000000-0000-0000-0000-000000000061"), "NL", "Holandija" },
                    { new Guid("00000000-0000-0000-0000-000000000062"), "HN", "Honduras" },
                    { new Guid("00000000-0000-0000-0000-000000000063"), "HR", "Hrvatska" },
                    { new Guid("00000000-0000-0000-0000-000000000064"), "IN", "Indija" },
                    { new Guid("00000000-0000-0000-0000-000000000065"), "ID", "Indonezija" },
                    { new Guid("00000000-0000-0000-0000-000000000066"), "IQ", "Irak" },
                    { new Guid("00000000-0000-0000-0000-000000000067"), "IR", "Iran" },
                    { new Guid("00000000-0000-0000-0000-000000000068"), "IE", "Irska" },
                    { new Guid("00000000-0000-0000-0000-000000000069"), "IS", "Island" },
                    { new Guid("00000000-0000-0000-0000-000000000070"), "TL", "Istočni Timor" },
                    { new Guid("00000000-0000-0000-0000-000000000071"), "IT", "Italija" },
                    { new Guid("00000000-0000-0000-0000-000000000072"), "IL", "Izrael" },
                    { new Guid("00000000-0000-0000-0000-000000000073"), "JM", "Jamajka" },
                    { new Guid("00000000-0000-0000-0000-000000000074"), "JP", "Japan" },
                    { new Guid("00000000-0000-0000-0000-000000000075"), "YE", "Jemen" },
                    { new Guid("00000000-0000-0000-0000-000000000076"), "JO", "Jordan" },
                    { new Guid("00000000-0000-0000-0000-000000000077"), "ZA", "Južna Afrika" },
                    { new Guid("00000000-0000-0000-0000-000000000078"), "KR", "Južna Koreja" },
                    { new Guid("00000000-0000-0000-0000-000000000079"), "SS", "Južni Sudan" },
                    { new Guid("00000000-0000-0000-0000-000000000080"), "KH", "Kambodža" },
                    { new Guid("00000000-0000-0000-0000-000000000081"), "CM", "Kamerun" },
                    { new Guid("00000000-0000-0000-0000-000000000082"), "CA", "Kanada" },
                    { new Guid("00000000-0000-0000-0000-000000000083"), "QA", "Katar" },
                    { new Guid("00000000-0000-0000-0000-000000000084"), "KZ", "Kazahstan" },
                    { new Guid("00000000-0000-0000-0000-000000000085"), "KE", "Kenija" },
                    { new Guid("00000000-0000-0000-0000-000000000086"), "CN", "Kina" },
                    { new Guid("00000000-0000-0000-0000-000000000087"), "CY", "Kipar" },
                    { new Guid("00000000-0000-0000-0000-000000000088"), "KG", "Kirgistan" },
                    { new Guid("00000000-0000-0000-0000-000000000089"), "KI", "Kiribati" },
                    { new Guid("00000000-0000-0000-0000-000000000090"), "CO", "Kolumbija" },
                    { new Guid("00000000-0000-0000-0000-000000000091"), "KM", "Komori" },
                    { new Guid("00000000-0000-0000-0000-000000000092"), "CG", "Kongo" },
                    { new Guid("00000000-0000-0000-0000-000000000093"), "CR", "Kostarika" },
                    { new Guid("00000000-0000-0000-0000-000000000094"), "CU", "Kuba" },
                    { new Guid("00000000-0000-0000-0000-000000000095"), "KW", "Kuvajt" },
                    { new Guid("00000000-0000-0000-0000-000000000096"), "LA", "Laos" },
                    { new Guid("00000000-0000-0000-0000-000000000097"), "LS", "Lesoto" },
                    { new Guid("00000000-0000-0000-0000-000000000098"), "LV", "Letonija" },
                    { new Guid("00000000-0000-0000-0000-000000000099"), "LB", "Libanon" },
                    { new Guid("00000000-0000-0000-0000-000000000100"), "LR", "Liberija" },
                    { new Guid("00000000-0000-0000-0000-000000000101"), "LY", "Libija" },
                    { new Guid("00000000-0000-0000-0000-000000000102"), "LI", "Lihtenštajn" },
                    { new Guid("00000000-0000-0000-0000-000000000103"), "LT", "Litvanija" },
                    { new Guid("00000000-0000-0000-0000-000000000104"), "LU", "Luksemburg" },
                    { new Guid("00000000-0000-0000-0000-000000000105"), "MG", "Madagaskar" },
                    { new Guid("00000000-0000-0000-0000-000000000106"), "HU", "Mađarska" },
                    { new Guid("00000000-0000-0000-0000-000000000107"), "MW", "Malavi" },
                    { new Guid("00000000-0000-0000-0000-000000000108"), "MV", "Maldivi" },
                    { new Guid("00000000-0000-0000-0000-000000000109"), "MY", "Malezija" },
                    { new Guid("00000000-0000-0000-0000-000000000110"), "ML", "Mali" },
                    { new Guid("00000000-0000-0000-0000-000000000111"), "MT", "Malta" },
                    { new Guid("00000000-0000-0000-0000-000000000112"), "MA", "Maroko" },
                    { new Guid("00000000-0000-0000-0000-000000000113"), "MH", "Maršalska Ostrva" },
                    { new Guid("00000000-0000-0000-0000-000000000114"), "MU", "Mauricijus" },
                    { new Guid("00000000-0000-0000-0000-000000000115"), "MR", "Mauritanija" },
                    { new Guid("00000000-0000-0000-0000-000000000116"), "MX", "Meksiko" },
                    { new Guid("00000000-0000-0000-0000-000000000117"), "MM", "Mijanmar" },
                    { new Guid("00000000-0000-0000-0000-000000000118"), "FM", "Mikronezija" },
                    { new Guid("00000000-0000-0000-0000-000000000119"), "MD", "Moldavija" },
                    { new Guid("00000000-0000-0000-0000-000000000120"), "MC", "Monako" },
                    { new Guid("00000000-0000-0000-0000-000000000121"), "MN", "Mongolija" },
                    { new Guid("00000000-0000-0000-0000-000000000122"), "MZ", "Mozambik" },
                    { new Guid("00000000-0000-0000-0000-000000000123"), "NA", "Namibija" },
                    { new Guid("00000000-0000-0000-0000-000000000124"), "NR", "Nauru" },
                    { new Guid("00000000-0000-0000-0000-000000000125"), "DE", "Nemačka" },
                    { new Guid("00000000-0000-0000-0000-000000000126"), "NP", "Nepal" },
                    { new Guid("00000000-0000-0000-0000-000000000127"), "NE", "Niger" },
                    { new Guid("00000000-0000-0000-0000-000000000128"), "NG", "Nigerija" },
                    { new Guid("00000000-0000-0000-0000-000000000129"), "NI", "Nikaragva" },
                    { new Guid("00000000-0000-0000-0000-000000000130"), "NO", "Norveška" },
                    { new Guid("00000000-0000-0000-0000-000000000131"), "NZ", "Novi Zeland" },
                    { new Guid("00000000-0000-0000-0000-000000000132"), "OM", "Oman" },
                    { new Guid("00000000-0000-0000-0000-000000000133"), "PK", "Pakistan" },
                    { new Guid("00000000-0000-0000-0000-000000000134"), "PW", "Palau" },
                    { new Guid("00000000-0000-0000-0000-000000000135"), "PS", "Palestina" },
                    { new Guid("00000000-0000-0000-0000-000000000136"), "PA", "Panama" },
                    { new Guid("00000000-0000-0000-0000-000000000137"), "PG", "Papua Nova Gvineja" },
                    { new Guid("00000000-0000-0000-0000-000000000138"), "PY", "Paragvaj" },
                    { new Guid("00000000-0000-0000-0000-000000000139"), "PE", "Peru" },
                    { new Guid("00000000-0000-0000-0000-000000000140"), "PL", "Poljska" },
                    { new Guid("00000000-0000-0000-0000-000000000141"), "PT", "Portugal" },
                    { new Guid("00000000-0000-0000-0000-000000000142"), "RW", "Ruanda" },
                    { new Guid("00000000-0000-0000-0000-000000000143"), "RO", "Rumunija" },
                    { new Guid("00000000-0000-0000-0000-000000000144"), "RU", "Rusija" },
                    { new Guid("00000000-0000-0000-0000-000000000145"), "SV", "Salvador" },
                    { new Guid("00000000-0000-0000-0000-000000000146"), "WS", "Samoa" },
                    { new Guid("00000000-0000-0000-0000-000000000147"), "SM", "San Marino" },
                    { new Guid("00000000-0000-0000-0000-000000000148"), "SA", "Saudijska Arabija" },
                    { new Guid("00000000-0000-0000-0000-000000000149"), "SC", "Sejšeli" },
                    { new Guid("00000000-0000-0000-0000-000000000150"), "SN", "Senegal" },
                    { new Guid("00000000-0000-0000-0000-000000000151"), "KP", "Severna Koreja" },
                    { new Guid("00000000-0000-0000-0000-000000000152"), "MK", "Severna Makedonija" },
                    { new Guid("00000000-0000-0000-0000-000000000153"), "SL", "Sijera Leone" },
                    { new Guid("00000000-0000-0000-0000-000000000154"), "SG", "Singapur" },
                    { new Guid("00000000-0000-0000-0000-000000000155"), "SY", "Sirija" },
                    { new Guid("00000000-0000-0000-0000-000000000156"), "US", "Sjedinjene Američke Države" },
                    { new Guid("00000000-0000-0000-0000-000000000157"), "SK", "Slovačka" },
                    { new Guid("00000000-0000-0000-0000-000000000158"), "SI", "Slovenija" },
                    { new Guid("00000000-0000-0000-0000-000000000159"), "SB", "Solomonska Ostrva" },
                    { new Guid("00000000-0000-0000-0000-000000000160"), "SO", "Somalija" },
                    { new Guid("00000000-0000-0000-0000-000000000161"), "RS", "Srbija" },
                    { new Guid("00000000-0000-0000-0000-000000000162"), "SD", "Sudan" },
                    { new Guid("00000000-0000-0000-0000-000000000163"), "SR", "Surinam" },
                    { new Guid("00000000-0000-0000-0000-000000000164"), "TJ", "Tadžikistan" },
                    { new Guid("00000000-0000-0000-0000-000000000165"), "TH", "Tajland" },
                    { new Guid("00000000-0000-0000-0000-000000000166"), "TZ", "Tanzanija" },
                    { new Guid("00000000-0000-0000-0000-000000000167"), "TG", "Togo" },
                    { new Guid("00000000-0000-0000-0000-000000000168"), "TO", "Tonga" },
                    { new Guid("00000000-0000-0000-0000-000000000169"), "TT", "Trinidad i Tobago" },
                    { new Guid("00000000-0000-0000-0000-000000000170"), "TN", "Tunis" },
                    { new Guid("00000000-0000-0000-0000-000000000171"), "TM", "Turkmenistan" },
                    { new Guid("00000000-0000-0000-0000-000000000172"), "TR", "Turska" },
                    { new Guid("00000000-0000-0000-0000-000000000173"), "TV", "Tuvalu" },
                    { new Guid("00000000-0000-0000-0000-000000000174"), "UG", "Uganda" },
                    { new Guid("00000000-0000-0000-0000-000000000175"), "AE", "Ujedinjeni Arapski Emirati" },
                    { new Guid("00000000-0000-0000-0000-000000000176"), "GB", "Ujedinjeno Kraljevstvo" },
                    { new Guid("00000000-0000-0000-0000-000000000177"), "UA", "Ukrajina" },
                    { new Guid("00000000-0000-0000-0000-000000000178"), "UY", "Urugvaj" },
                    { new Guid("00000000-0000-0000-0000-000000000179"), "UZ", "Uzbekistan" },
                    { new Guid("00000000-0000-0000-0000-000000000180"), "VU", "Vanuatu" },
                    { new Guid("00000000-0000-0000-0000-000000000181"), "VA", "Vatikan" },
                    { new Guid("00000000-0000-0000-0000-000000000182"), "VE", "Venecuela" },
                    { new Guid("00000000-0000-0000-0000-000000000183"), "VN", "Vijetnam" },
                    { new Guid("00000000-0000-0000-0000-000000000184"), "ZM", "Zambija" },
                    { new Guid("00000000-0000-0000-0000-000000000185"), "CV", "Zelenortska Ostrva" },
                    { new Guid("00000000-0000-0000-0000-000000000186"), "ZW", "Zimbabve" },
                    { new Guid("00000000-0000-0000-0000-000000000187"), "XK", "Kosovo" },
                    { new Guid("00000000-0000-0000-0000-000000000188"), "TW", "Tajvan" },
                    { new Guid("00000000-0000-0000-0000-000000000189"), "HK", "Hong Kong" },
                    { new Guid("00000000-0000-0000-0000-000000000190"), "MO", "Makao" },
                    { new Guid("00000000-0000-0000-0000-000000000191"), "PR", "Portoriko" },
                    { new Guid("00000000-0000-0000-0000-000000000192"), "GL", "Grenland" },
                    { new Guid("00000000-0000-0000-0000-000000000193"), "FO", "Farska Ostrva" },
                    { new Guid("00000000-0000-0000-0000-000000000194"), "GP", "Gvadalupa" },
                    { new Guid("00000000-0000-0000-0000-000000000195"), "MQ", "Martinik" },
                    { new Guid("00000000-0000-0000-0000-000000000196"), "GF", "Francuska Gvajana" },
                    { new Guid("00000000-0000-0000-0000-000000000197"), "RE", "Reunion" },
                    { new Guid("00000000-0000-0000-0000-000000000198"), "NC", "Novi Kaledonija" },
                    { new Guid("00000000-0000-0000-0000-000000000199"), "PF", "Francuska Polinezija" },
                    { new Guid("00000000-0000-0000-0000-000000000200"), "BM", "Bermuda" },
                    { new Guid("00000000-0000-0000-0000-000000000201"), "KY", "Kajmanska Ostrva" },
                    { new Guid("00000000-0000-0000-0000-000000000202"), "VG", "Britanska Devičanska Ostrva" },
                    { new Guid("00000000-0000-0000-0000-000000000203"), "AI", "Angvila" },
                    { new Guid("00000000-0000-0000-0000-000000000204"), "MS", "Montserat" },
                    { new Guid("00000000-0000-0000-0000-000000000205"), "TC", "Terks i Keikos Ostrva" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCodes_Code",
                table: "ActivityCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_AccountNumber",
                table: "BankAccounts",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_BusinessProfileId",
                table: "BankAccounts",
                column: "BusinessProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_BusinessProfileId_IsActive",
                table: "BankAccounts",
                columns: new[] { "BusinessProfileId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_IBAN",
                table: "BankAccounts",
                column: "IBAN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessInvites_BusinessProfileId_Email",
                table: "BusinessInvites",
                columns: new[] { "BusinessProfileId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessInvites_CreatedById",
                table: "BusinessInvites",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessInvites_Token",
                table: "BusinessInvites",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfiles_ActivityCodeId",
                table: "BusinessProfiles",
                column: "ActivityCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfiles_City_IsActive",
                table: "BusinessProfiles",
                columns: new[] { "City", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfiles_MB",
                table: "BusinessProfiles",
                column: "MB",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfiles_PIB",
                table: "BusinessProfiles",
                column: "PIB",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name",
                table: "Cities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_BusinessProfileId",
                table: "Clients",
                column: "BusinessProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_City_IsActive",
                table: "Clients",
                columns: new[] { "City", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CountryId",
                table: "Clients",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Name_BusinessProfileId",
                table: "Clients",
                columns: new[] { "Name", "BusinessProfileId" });

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Code",
                table: "Countries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_BusinessProfileId",
                table: "Documents",
                column: "BusinessProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_BusinessProfileId_IsDeleted",
                table: "Documents",
                columns: new[] { "BusinessProfileId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentNumber",
                table: "Documents",
                column: "DocumentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BusinessProfileId",
                table: "Expenses",
                column: "BusinessProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BusinessProfileId_IsDeleted",
                table: "Expenses",
                columns: new[] { "BusinessProfileId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_PaymentId",
                table: "Expenses",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ReferenceNumber",
                table: "Expenses",
                column: "ReferenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BusinessProfileId",
                table: "Invoices",
                column: "BusinessProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ClientId",
                table: "Invoices",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Items_BusinessProfileId",
                table: "Items",
                column: "BusinessProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BusinessProfileId",
                table: "Payments",
                column: "BusinessProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ExpenseId",
                table: "Payments",
                column: "ExpenseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TaxObligationId",
                table: "Payments",
                column: "TaxObligationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_BusinessProfileId",
                table: "Reminders",
                column: "BusinessProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_BusinessProfileId_IsCompleted",
                table: "Reminders",
                columns: new[] { "BusinessProfileId", "IsCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_BusinessProfileId_IsDeleted",
                table: "Reminders",
                columns: new[] { "BusinessProfileId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxObligations_BusinessProfileId",
                table: "TaxObligations",
                column: "BusinessProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxObligations_PaymentId",
                table: "TaxObligations",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBusinessProfiles_BusinessProfileId",
                table: "UserBusinessProfiles",
                column: "BusinessProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBusinessProfiles_UserId",
                table: "UserBusinessProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBusinessProfiles_UserId_BusinessProfileId",
                table: "UserBusinessProfiles",
                columns: new[] { "UserId", "BusinessProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Email",
                table: "UserProfiles",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Payments_PaymentId",
                table: "Expenses",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_BusinessProfiles_BusinessProfileId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_BusinessProfiles_BusinessProfileId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_BusinessProfiles_BusinessProfileId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_BusinessProfiles_BusinessProfileId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Countries_CountryId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Payments_PaymentId",
                table: "Expenses");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "BusinessInvites");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "TaxObligations");

            migrationBuilder.DropTable(
                name: "UserBusinessProfiles");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "BusinessProfiles");

            migrationBuilder.DropTable(
                name: "ActivityCodes");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Clients");
        }
    }
}

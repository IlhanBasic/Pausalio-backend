using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Shared.Localization
{
    public class LocalizationHelper : ILocalizationHelper
    {
        private readonly IStringLocalizer<Resources> _localizer;
        public LocalizationHelper(IStringLocalizer<Resources> localizer)
        {
            _localizer = localizer;
        }
        public string BankNameRequired => _localizer["BankNameRequired"];

        public string AccountNumberRequired => _localizer["AccountNumberRequired"];

        public string UnknownCurrency => _localizer["UnknownCurrency"];

        public string InvalidIBAN => _localizer["InvalidIBAN"];

        public string InvalidSWIFT => _localizer["InvalidSWIFT"];
        public string BankNameMaxLength => _localizer["BankNameMaxLength"];
        public string AccountNumberLength => _localizer["AccountNumberLength"];
        // ---- Business Profile ----
        public string BusinessNameRequired => _localizer["BusinessNameRequired"];
        public string BusinessNameMaxLength => _localizer["BusinessNameMaxLength"];
        public string PIBRequired => _localizer["PIBRequired"];
        public string PIBLength => _localizer["PIBLength"];
        public string MBLength => _localizer["MBLength"];
        public string ActivityCodeMaxLength => _localizer["ActivityCodeMaxLength"];
        public string CityRequired => _localizer["CityRequired"];
        public string CityMaxLength => _localizer["CityMaxLength"];
        public string AddressRequired => _localizer["AddressRequired"];
        public string AddressMaxLength => _localizer["AddressMaxLength"];
        public string EmailRequired => _localizer["EmailRequired"];
        public string EmailInvalid => _localizer["EmailInvalid"];
        public string EmailMaxLength => _localizer["EmailMaxLength"];
        public string PhoneInvalid => _localizer["PhoneInvalid"];
        public string PhoneMaxLength => _localizer["PhoneMaxLength"];
        public string WebsiteInvalid => _localizer["WebsiteInvalid"];
        public string WebsiteMaxLength => _localizer["WebsiteMaxLength"];
        public string CompanyLogoMaxLength => _localizer["CompanyLogoMaxLength"];
        public string ClientNameRequired => _localizer["ClientNameRequired"];
        public string ClientNameMaxLength => _localizer["ClientNameMaxLength"];
        public string ClientPIBLength => _localizer["ClientPIBLength"];
        public string ClientMBLength => _localizer["ClientMBLength"];
        public string ClientAddressRequired => _localizer["ClientAddressRequired"];
        public string ClientAddressMaxLength => _localizer["ClientAddressMaxLength"];
        public string ClientCityRequired => _localizer["ClientCityRequired"];
        public string ClientCityMaxLength => _localizer["ClientCityMaxLength"];
        public string ClientEmailRequired => _localizer["ClientEmailRequired"];
        public string ClientEmailInvalid => _localizer["ClientEmailInvalid"];
        public string ClientEmailMaxLength => _localizer["ClientEmailMaxLength"];
        public string ClientPhoneInvalid => _localizer["ClientPhoneInvalid"];
        public string ClientPhoneMaxLength => _localizer["ClientPhoneMaxLength"];
        public string ClientCountryMaxLength => _localizer["ClientCountryMaxLength"];
        public string DocumentNumberRequired => _localizer["DocumentNumberRequired"];
        public string DocumentNumberMaxLength => _localizer["DocumentNumberMaxLength"];
        public string DocumentFilePathRequired => _localizer["DocumentFilePathRequired"];
        public string DocumentFilePathMaxLength => _localizer["DocumentFilePathMaxLength"];
        public string ExpenseNameRequired => _localizer["ExpenseNameRequired"];
        public string ExpenseNameMaxLength => _localizer["ExpenseNameMaxLength"];
        public string ExpenseAmountGreaterThanZero => _localizer["ExpenseAmountGreaterThanZero"];
        public string ExpenseStatusInvalid => _localizer["ExpenseStatusInvalid"];

        public string InvoiceItemInvoiceIdRequired => _localizer["InvoiceItemInvoiceIdRequired"];

        public string InvoiceItemNameRequired => _localizer["InvoiceItemNameRequired"];
        public string InvoiceItemNameMaxLength => _localizer["InvoiceItemNameMaxLength"];

        public string InvoiceItemDescriptionMaxLength => _localizer["InvoiceItemDescriptionMaxLength"];

        public string InvoiceItemQuantityGreaterThanZero => _localizer["InvoiceItemQuantityGreaterThanZero"];

        public string InvoiceItemUnitPriceMinZero => _localizer["InvoiceItemUnitPriceMinZero"];
        public string InvoiceClientIdRequired => _localizer["InvoiceClientIdRequired"];

        public string InvoiceDueDateNotInPast => _localizer["InvoiceDueDateNotInPast"];

        public string InvoiceExchangeRateGreaterThanZero => _localizer["InvoiceExchangeRateGreaterThanZero"];

        public string InvoiceItemsRequired => _localizer["InvoiceItemsRequired"];
        public string ItemNameRequired => _localizer["ItemNameRequired"];
        public string ItemNameMaxLength => _localizer["ItemNameMaxLength"];

        public string ItemDescriptionMaxLength => _localizer["ItemDescriptionMaxLength"];

        public string ItemUnitPriceNonNegative => _localizer["ItemUnitPriceNonNegative"];
        public string PaymentTypeUnknown => _localizer["PaymentTypeUnknown"];
        public string PaymentEntityIdRequired => _localizer["PaymentEntityIdRequired"];
        public string PaymentAmountGreaterThanZero => _localizer["PaymentAmountGreaterThanZero"];
        public string PaymentUnknownCurrency => _localizer["PaymentUnknownCurrency"];
        public string PaymentExchangeRateGreaterThanZero => _localizer["PaymentExchangeRateGreaterThanZero"];
        public string PaymentReferenceMaxLength => _localizer["PaymentReferenceMaxLength"];
        public string PaymentDescriptionMaxLength => _localizer["PaymentDescriptionMaxLength"];
        public string ReminderTitleRequired => _localizer["ReminderTitleRequired"];
        public string ReminderTitleMaxLength => _localizer["ReminderTitleMaxLength"];
        public string ReminderDescriptionMaxLength => _localizer["ReminderDescriptionMaxLength"];
        public string ReminderTypeUnknown => _localizer["ReminderTypeUnknown"];
        public string ReminderDueDateInPast => _localizer["ReminderDueDateInPast"];

        public string TaxDueDateInPast => _localizer["TaxDueDateInPast"];
        public string TaxTypeUnknown => _localizer["TaxTypeUnknown"];
        public string TaxStatusUnknown => _localizer["TaxStatusUnknown"];
        public string TaxTotalAmountGreaterThanZero => _localizer["TaxTotalAmountGreaterThanZero"];
        public string UserIdRequired => _localizer["UserIdRequired"];
        public string BusinessProfileIdRequired => _localizer["BusinessProfileIdRequired"];
        public string UserBusinessRoleRequired => _localizer["UserBusinessRoleRequired"];
        public string UserFirstNameRequired => _localizer["UserFirstNameRequired"];
        public string UserFirstNameMaxLength => _localizer["UserFirstNameMaxLength"];
        public string UserLastNameRequired => _localizer["UserLastNameRequired"];
        public string UserLastNameMaxLength => _localizer["UserLastNameMaxLength"];
        public string UserEmailRequired => _localizer["UserEmailRequired"];
        public string UserEmailInvalid => _localizer["UserEmailInvalid"];
        public string UserEmailMaxLength => _localizer["UserEmailMaxLength"];
        public string UserPasswordRequired => _localizer["UserPasswordRequired"];
        public string UserPasswordMaxLength => _localizer["UserPasswordMaxLength"];
        public string UserProfilePictureMaxLength => _localizer["UserProfilePictureMaxLength"];
        public string UserPhoneInvalid => _localizer["UserPhoneInvalid"];
        public string UserPhoneMaxLength => _localizer["UserPhoneMaxLength"];
        public string UserCityMaxLength => _localizer["UserCityMaxLength"];
        public string UserAddressMaxLength => _localizer["UserAddressMaxLength"];
        public string ServerError => _localizer["ServerError"];
        public string UserProfileCreationFailed => _localizer["UserProfileCreationFailed"];


    }
}

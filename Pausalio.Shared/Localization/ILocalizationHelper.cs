namespace Pausalio.Shared.Localization
{
    public interface ILocalizationHelper
    {
        string BankNameRequired { get; }
        string AccountNumberRequired { get; }
        string UnknownCurrency {  get; }
        string InvalidIBAN { get; }
        string InvalidSWIFT { get; }
        string BankNameMaxLength { get; }
        string AccountNumberLength { get; }
        string BusinessNameRequired { get; }
        string BusinessNameMaxLength { get; }
        string PIBRequired { get; }
        string PIBLength { get; }
        string MBLength { get; }
        string ActivityCodeMaxLength { get; }
        string CityRequired { get; }
        string CityMaxLength { get; }
        string AddressRequired { get; }
        string AddressMaxLength { get; }
        string EmailRequired { get; }
        string EmailInvalid { get; }
        string EmailMaxLength { get; }
        string PhoneInvalid { get; }
        string PhoneMaxLength { get; }
        string WebsiteInvalid { get; }
        string WebsiteMaxLength { get; }
        string CompanyLogoMaxLength { get; }
        string ClientNameRequired { get; }
        string ClientNameMaxLength { get; }
        string ClientPIBLength { get; }
        string ClientMBLength { get; }
        string ClientAddressRequired { get; }
        string ClientAddressMaxLength { get; }
        string ClientCityRequired { get; }
        string ClientCityMaxLength { get; }
        string ClientEmailRequired { get; }
        string ClientEmailInvalid { get; }
        string ClientEmailMaxLength { get; }
        string ClientPhoneInvalid { get; }
        string ClientPhoneMaxLength { get; }
        string ClientCountryMaxLength { get; }
        string DocumentNumberRequired { get; }
        string DocumentNumberMaxLength { get; }
        string DocumentFilePathRequired { get; }
        string DocumentFilePathMaxLength { get; }
        string ExpenseNameRequired { get; }
        string ExpenseNameMaxLength { get; }
        string ExpenseAmountGreaterThanZero { get; }
        string ExpenseStatusInvalid { get; }
        string InvoiceItemInvoiceIdRequired { get; }

        string InvoiceItemNameRequired { get; }
        string InvoiceItemNameMaxLength { get; }

        string InvoiceItemDescriptionMaxLength { get; }

        string InvoiceItemQuantityGreaterThanZero { get; }

        string InvoiceItemUnitPriceMinZero { get; }
        string InvoiceClientIdRequired { get; }

        string InvoiceDueDateNotInPast { get; }

        string InvoiceExchangeRateGreaterThanZero { get; }

        string InvoiceItemsRequired { get; }
        string ItemNameRequired { get; }
        string ItemNameMaxLength { get; }

        string ItemDescriptionMaxLength { get; }

        string ItemUnitPriceNonNegative { get; }
        string PaymentTypeUnknown { get; }
        string PaymentEntityIdRequired { get; }
        string PaymentAmountGreaterThanZero { get; }
        string PaymentUnknownCurrency { get; }
        string PaymentExchangeRateGreaterThanZero { get; }
        string PaymentReferenceMaxLength { get; }
        string PaymentDescriptionMaxLength { get; }
        string ReminderTitleRequired { get; }
        string ReminderTitleMaxLength { get; }
        string ReminderDescriptionMaxLength { get; }
        string ReminderTypeUnknown { get; }
        string ReminderDueDateInPast { get; }
        string TaxDueDateInPast { get; }
        string TaxTypeUnknown { get; }
        string TaxStatusUnknown { get; }
        string TaxTotalAmountGreaterThanZero { get; }

        string UserIdRequired { get; }
        string BusinessProfileIdRequired { get; }
        string UserBusinessRoleRequired { get; }
        string UserFirstNameRequired { get; }
        string UserFirstNameMaxLength { get; }
        string UserLastNameRequired { get; }
        string UserLastNameMaxLength { get; }
        string UserEmailRequired { get; }
        string UserEmailInvalid { get; }
        string UserEmailMaxLength { get; }
        string UserPasswordRequired { get; }
        string UserPasswordMaxLength { get; }
        string UserProfilePictureMaxLength { get; }
        string UserPhoneInvalid { get; }
        string UserPhoneMaxLength { get; }
        string UserCityMaxLength { get; }
        string UserAddressMaxLength { get; }
        string ServerError { get; }
        string UserProfileCreationFailed { get; }
    }
}

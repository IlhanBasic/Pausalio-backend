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
        public string ClientEmailAlreadyExists => _localizer["ClientEmailAlreadyExists"];
        public string InvalidPassword => _localizer["InvalidPassword"];
        public string LoginSuccessfull => _localizer["LoginSuccessfull"];
        public string PasswordRegex => _localizer["PasswordRegex"];
        public string Register => _localizer["Register"];
        public string InviteTokenFooter => _localizer["InviteTokenFooter"];
        public string InviteTokenPageTitle => _localizer["InviteTokenPageTitle"];
        public string BusinesProfileNotFound => _localizer["BusinesProfileNotFound"];
        public string InviteTokenDismatch => _localizer["InviteTokenDismatch"];
        public string InviteTokenDoesNotExist => _localizer["InviteTokenDoesNotExist"];
        public string CompanyAlreadyExists => _localizer["CompanyAlreadyExists"];
        public string InvalidRequest => _localizer["InvalidRequest"];
        public string UserNotFound => _localizer["UserNotFound"];
        public string InvalidOrExpiredToken => _localizer["InvalidOrExpiredToken"];
        public string EmailVerify => _localizer["EmailVerify"];
        public string EmailVerifyGreeting => _localizer["EmailVerifyGreeting"];
        public string EmailVerifyText => _localizer["EmailVerifyText"];
        public string EmailVerifyButton => _localizer["EmailVerifyButton"];
        public string EmailVerifyFallback => _localizer["EmailVerifyFallback"];
        public string EmailVerifyFooter => _localizer["EmailVerifyFooter"];
        public string ConfirmEmail => _localizer["ConfirmEmail"];
        public string RegistrationFailed => _localizer["RegistrationFailed"];
        public string UserAlreadyExists => _localizer["UserAlreadyExists"];
        public string InviteTokenRequired => _localizer["InviteTokenRequired"];
        public string ExpiresAtFuture => _localizer["ExpiresAtFuture"];
        public string InviteExpiresAtFuture => _localizer["InviteExpiresAtFuture"];
        public string BusinessProfileIdInvalid => _localizer["BusinessProfileIdInvalid"];
        public string ClientCountryInvalid => _localizer["ClientCountryInvalid"];
        public string ActivityCodeRequired => _localizer["ActivityCodeRequired"];
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

        public string EmailVerifiedSuccessfully => _localizer["EmailVerifiedSuccessfully"];

        public string VerificationSuccessTitle => _localizer["VerificationSuccessTitle"];

        public string VerificationSuccessHeading => _localizer["VerificationSuccessHeading"];

        public string VerificationSuccessMessage => _localizer["VerificationSuccessMessage"];

        public string GoToLogin => _localizer["GoToLogin"];

        public string VerificationErrorTitle => _localizer["VerificationErrorTitle"];

        public string VerificationErrorHeading => _localizer["VerificationErrorHeading"];

        public string ResendVerificationEmail => _localizer["ResendVerificationEmail"];

        public string GoToHome => _localizer["GoToHome"];

        public string EmailAlreadyVerified => _localizer["EmailAlreadyVerified"];

        public string VerificationEmailResent => _localizer["VerificationEmailResent"];

        public string RegistrationSuccessful => _localizer["RegistrationSuccessful"];

        public string InviteTokenPageMessage => _localizer["InviteTokenPageMessage"];

        public string InviteTokenTitle => _localizer["InviteTokenTitle"];

        public string InvalidCredentials => _localizer["InvalidCredentials"];

        public string EmailNotVerified => _localizer["EmailNotVerified"];

        public string UserInactive => _localizer["UserInactive"];

        public string UserEmailNotProvided => _localizer["UserEmailNotProvided"];

        public string InviteTokenAlreadyExists => _localizer["InviteTokenAlreadyExists"];

        public string InviteTokenCreateFail => _localizer["InviteTokenCreateFail"];

        public string InvalidCompanyId => _localizer["InvalidCompanyId"];

        public string UserCompanyNotFound => _localizer["UserCompanyNotFound"];

        public string CityNameMaxLength => _localizer["CityNameMaxLength"];

        public string CityNameRequired => _localizer["CityNameRequired"];

        public string PostalCodeMaxLength => _localizer["PostalCodeMaxLength"];

        public string PostalCodeRequired => _localizer["PostalCodeRequired"];

        public string ReminderDeletedSuccessfully => _localizer["ReminderDeletedSuccessfully"];

        public string ReminderUpdatedSuccessfully => _localizer["ReminderUpdatedSuccessfully"];

        public string ReminderCreatedSuccessfully => _localizer["ReminderCreatedSuccessfully"];

        public string ReminderNotFound => _localizer["ReminderNotFound"];

        public string ItemDeletedSuccessfully => _localizer["ItemDeletedSuccessfully"];

        public string ItemUpdatedSuccessfully => _localizer["ItemUpdatedSuccessfully"];

        public string ItemCreatedSuccessfully => _localizer["ItemCreatedSuccessfully"];

        public string ItemNotFound => _localizer["ItemNotFound"];

        public string DocumentDeletedSuccessfully => _localizer["DocumentDeletedSuccessfully"];

        public string DocumentUpdatedSuccessfully => _localizer["DocumentUpdatedSuccessfully"];

        public string DocumentCreatedSuccessfully => _localizer["DocumentCreatedSuccessfully"];

        public string DocumentNotFound => _localizer["DocumentNotFound"];

        public string CountryDeletedSuccessfully => _localizer["CountryDeletedSuccessfully"];

        public string CountryUpdatedSuccessfully => _localizer["CountryUpdatedSuccessfully"];

        public string CountryCreatedSuccessfully => _localizer["CountryCreatedSuccessfully"];

        public string CountryNotFound => _localizer["CountryNotFound"];

        public string CityDeletedSuccessfully => _localizer["CityDeletedSuccessfully"];

        public string CityUpdatedSuccessfully => _localizer["CityUpdatedSuccessfully"];

        public string CityCreatedSuccessfully => _localizer["CityCreatedSuccessfully"];

        public string CityNotFound => _localizer["CityNotFound"];

        public string BankAccountDeletedSuccessfully => _localizer["BankAccountDeletedSuccessfully"];

        public string BankAccountUpdatedSuccessfully => _localizer["BankAccountUpdatedSuccessfully"];

        public string BankAccountCreatedSuccessfully => _localizer["BankAccountCreatedSuccessfully"];

        public string BankAccountNotFound => _localizer["BankAccountNotFound"];

        public string ActivityCodeAlreadyExists => _localizer["ActivityCodeAlreadyExists"];

        public string ActivityCodeDeletedSuccessfully => _localizer["ActivityCodeDeletedSuccessfully"];

        public string ActivityCodeUpdatedSuccessfully => _localizer["ActivityCodeUpdatedSuccessfully"];

        public string ActivityCodeCreatedSuccessfully => _localizer["ActivityCodeCreatedSuccessfully"];

        public string ActivityCodeNotFound => _localizer["ActivityCodeNotFound"];

        public string CountryCodeInvalidLength => _localizer["CountryCodeInvalidLength"];

        public string CountryCodeRequired => _localizer["CountryCodeRequired"];

        public string CountryNameTooLong => _localizer["CountryNameTooLong"];

        public string CountryNameRequired => _localizer["CountryNameRequired"];

        public string ActivityCodeDescriptionTooLong => _localizer["ActivityCodeDescriptionTooLong"];

        public string ActivityCodeDescriptionRequired => _localizer["ActivityCodeDescriptionRequired"];

        public string ActivityCodeTooLong => _localizer["ActivityCodeTooLong"];

        public string ReminderMarkedCompleted => _localizer["ReminderMarkedCompleted"];

        public string FileUploadFailed => _localizer["FileUploadFailed"];

        public string FileUploadedSuccessfully => _localizer["FileUploadedSuccessfully"];

        public string UnsupportedFileType => _localizer["UnsupportedFileType"];

        public string FileIsEmptyOrNotProvided => _localizer["FileIsEmptyOrNotProvided"];

        public string FileDeleteFailed => _localizer["FileDeleteFailed"];

        public string FileDeletedSuccessfully => _localizer["FileDeletedSuccessfully"];

        public string UrlIsRequired => _localizer["UrlIsRequired"];

        public string InviteSent => _localizer["InviteSent"];

        public string InviteRemoved => _localizer["InviteRemoved"];

        public string InviteAcceptedSuccessfully => _localizer["InviteAcceptedSuccessfully"];

        public string FailedToAddUserToBusiness => _localizer["FailedToAddUserToBusiness"];

        public string AlreadyAssistantInBusiness => _localizer["AlreadyAssistantInBusiness"];

        public string Unauthorized => _localizer["Unauthorized"];

        public string UserAlreadyAssistantInYourBusiness => _localizer["UserAlreadyAssistantInYourBusiness"];

        public string InviteAlreadySentToThisUser => _localizer["InviteAlreadySentToThisUser"];

        public string CannotInviteOwner => _localizer["CannotInviteOwner"];

        public string CannotInviteAdmin => _localizer["CannotInviteAdmin"];

        public string CompanyWithPIBOrMBAlreadyExists => _localizer["CompanyWithPIBOrMBAlreadyExists"];

        public string CompanyActivatedSuccessfully => _localizer["CompanyActivatedSuccessfully"];

        public string CompanyDeactivatedSuccessfully => _localizer["CompanyDeactivatedSuccessfully"];

        public string CompanyUpdatedSuccessfully => _localizer["CompanyUpdatedSuccessfully"];

        public string ForeignClientMustHaveCountry => _localizer["ForeignClientMustHaveCountry"];

        public string ClientWithPIBAlreadyExists => _localizer["ClientWithPIBAlreadyExists"];

        public string LegalEntityMustHavePIB => _localizer["LegalEntityMustHavePIB"];

        public string ClientActivatedSuccessfully => _localizer["ClientActivatedSuccessfully"];

        public string ClientDeletedSuccessfully => _localizer["ClientDeletedSuccessfully"];

        public string ClientUpdatedSuccessfully => _localizer["ClientUpdatedSuccessfully"];

        public string ClientCreatedSuccessfully => _localizer["ClientCreatedSuccessfully"];

        public string ClientNotFound => _localizer["ClientNotFound"];

        public string CannotDeletePaidExpense => _localizer["CannotDeletePaidExpense"];

        public string CannotModifyPaidExpense => _localizer["CannotModifyPaidExpense"];

        public string AmountMustBePositive => _localizer["AmountMustBePositive"];

        public string ExpenseArchivedSuccessfully => _localizer["ExpenseArchivedSuccessfully"];

        public string ExpenseDeletedSuccessfully => _localizer["ExpenseDeletedSuccessfully"];

        public string ExpenseUpdatedSuccessfully => _localizer["ExpenseUpdatedSuccessfully"];

        public string ExpenseCreatedSuccessfully => _localizer["ExpenseCreatedSuccessfully"];

        public string ExpenseNotFound => _localizer["ExpenseNotFound"];

        public string CannotDeletePaidObligation => _localizer["CannotDeletePaidObligation"];

        public string CannotModifyPaidObligation => _localizer["CannotModifyPaidObligation"];

        public string ObligationAlreadyExistsForMonth => _localizer["ObligationAlreadyExistsForMonth"];

        public string ObligationsAlreadyExistForYear => _localizer["ObligationsAlreadyExistForYear"];

        public string InvalidDueDay => _localizer["InvalidDueDay"];

        public string InvalidYear => _localizer["InvalidYear"];

        public string TaxObligationMarkedAsPaid => _localizer["TaxObligationMarkedAsPaid"];

        public string TaxObligationDeletedSuccessfully => _localizer["TaxObligationDeletedSuccessfully"];

        public string TaxObligationUpdatedSuccessfully => _localizer["TaxObligationUpdatedSuccessfully"];

        public string TaxObligationsGeneratedSuccessfully => _localizer["TaxObligationsGeneratedSuccessfully"];

        public string TaxObligationCreatedSuccessfully => _localizer["TaxObligationCreatedSuccessfully"];

        public string TaxObligationNotFound => _localizer["TaxObligationNotFound"];

        public string CountrySerbiaNotFound => _localizer["CountrySerbiaNotFound"];

        public string IBANRequiredForForeignCurrency => _localizer["IBANRequiredForForeignCurrency"];

        public string SWIFTRequiredForForeignCurrency => _localizer["SWIFTRequiredForForeignCurrency"];

        public string CannotDeletePaidInvoice => _localizer["CannotDeletePaidInvoice"];

        public string CannotModifyPaidInvoice => _localizer["CannotModifyPaidInvoice"];

        public string InvalidInvoiceItemValues => _localizer["InvalidInvoiceItemValues"];

        public string InvoiceMustHaveItems => _localizer["InvoiceMustHaveItems"];

        public string InvoiceDeletedSuccessfully => _localizer["InvoiceDeletedSuccessfully"];

        public string InvoiceUpdatedSuccessfully => _localizer["InvoiceUpdatedSuccessfully"];

        public string InvoiceCreatedSuccessfully => _localizer["InvoiceCreatedSuccessfully"];

        public string InvoiceNotFound => _localizer["InvoiceNotFound"];

        public string InvoiceItemUnitPriceGreaterThanZero => _localizer["InvoiceItemUnitPriceGreaterThanZero"];

        public string InvalidPaymentType => _localizer["InvalidPaymentType"];

        public string EntityIdRequired => _localizer["EntityIdRequired"];

        public string PaymentNotFound => _localizer["PaymentNotFound"];

        public string InvoiceAlreadyPaid => _localizer["InvoiceAlreadyPaid"];

        public string PaymentExceedsRemainingAmount => _localizer["PaymentExceedsRemainingAmount"];

        public string TaxObligationAlreadyPaid => _localizer["TaxObligationAlreadyPaid"];

        public string TaxObligationMustBePaidInFull => _localizer["TaxObligationMustBePaidInFull"];

        public string ExpenseAlreadyPaid => _localizer["ExpenseAlreadyPaid"];

        public string ExpenseMustBePaidInFull => _localizer["ExpenseMustBePaidInFull"];

        public string PaymentCreatedSuccessfully => _localizer["PaymentCreatedSuccessfully"];

        public string PaymentUpdatedSuccessfully => _localizer["PaymentUpdatedSuccessfully"];

        public string PaymentDeletedSuccessfully => _localizer["PaymentDeletedSuccessfully"];

        public string LogoutSuccessful => _localizer["LogoutSuccessful"];

        public string NewPasswordMustBeDifferent => _localizer["NewPasswordMustBeDifferent"];

        public string InvalidOldPassword => _localizer["InvalidOldPassword"];

        public string PasswordChangedSuccessfully => _localizer["PasswordChangedSuccessfully"];

        public string PasswordChangeFailed => _localizer["PasswordChangeFailed"];

        public string PasswordRequired => _localizer["PasswordRequired"];

        public string PasswordReset => _localizer["PasswordReset"];

        public string PasswordResetEmailSent => _localizer["PasswordResetEmailSent"];

        public string PasswordResetTokenInvalidOrExpired => _localizer["PasswordResetTokenInvalidOrExpired"];

        public string PasswordResetFailed => _localizer["PasswordResetFailed"];

        public string AllFieldsRequired => _localizer["AllFieldsRequired"];

        public string PasswordResetText => _localizer["PasswordResetText"];

        public string PasswordResetGreeting => _localizer["PasswordResetGreeting"];

        public string UserUpdatedSuccessfully => _localizer["UserUpdatedSuccessfully"];

        public string ReminderMarkedFailed => _localizer["ReminderMarkedFailed"];

        public string UserDeletedSuccessfully => _localizer["UserDeletedSuccessfully"];

        public string CannotDeleteYourselfProfile => _localizer["CannotDeleteYourselfProfile"];

        public string InvalidUserId => _localizer["InvalidUserId"];

        public string CannotDeleteAdmin => _localizer["CannotDeleteAdmin"];

        public string CurrencyCannotBeChanged => _localizer["CurrencyCannotBeChanged"];
        public string CurrencyTypeCannotBeChanged => _localizer["CurrencyTypeCannotBeChanged"];

        public string InvoiceArchivedSuccessfully => _localizer["InvoiceArchivedSuccessfully"];

        public string CannotArchiveUnfinishedInvoice => _localizer["CannotArchiveUnfinishedInvoice"];
        public string CannotCancelFinishedInvoice => _localizer["CannotCancelFinishedInvoice"];

        public string CannotModifyOtherProfile => _localizer["CannotModifyOtherProfile"];

        public string ForeignClientCannotBeFromSerbia => _localizer["ForeignClientCannotBeFromSerbia"];

        public string ObligationsAlreadyExistForYearAndType => _localizer["ObligationsAlreadyExistForYearAndType"];

        public string ObligationAlreadyExistsForMonthAndType => _localizer["ObligationAlreadyExistsForMonthAndType"];

        public string ObligationAlreadyPaid => _localizer["ObligationAlreadyPaid"];

        public string CannotMarkFutureObligationAsPaid => _localizer["CannotMarkFutureObligationAsPaid"];

        public string NoEmailsProvided => _localizer["NoEmailsProvided"];

        public string BusinessProfileNotFound => _localizer["BusinessProfileNotFound"];

        public string InvoiceSentSuccessfully => _localizer["InvoiceSentSuccessfully"];

        public string CannotChangeStatusAdmin => _localizer["CannotChangeStatusAdmin"];

        public string UserActivatedSuccessfully => _localizer["UserActivatedSuccessfully"];

        public string UserDeactivatedSuccessfully => _localizer["UserDeactivatedSuccessfully"];
    }
}

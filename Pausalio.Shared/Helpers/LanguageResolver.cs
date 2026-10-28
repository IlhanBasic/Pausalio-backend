using Pausalio.Shared.Enums;

namespace Pausalio.Shared.Helpers
{
    public static class LanguageResolver
    {
        /// <summary>
        /// Determines the language for outgoing documents/emails.
        /// Foreign clients get English, all others get Serbian.
        /// </summary>
        public static Language Resolve(ClientType clientType)
        {
            return clientType == ClientType.Foreign
                ? Language.English
                : Language.Serbian;
        }
    }
}

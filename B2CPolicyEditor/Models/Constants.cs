using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace B2CPolicyEditor.Models
{
    public static class Constants
    {
        public static XNamespace dflt = "http://schemas.microsoft.com/online/cpim/schemas/2013/06";
        public static string PolicyBaseUrl = "https://raw.githubusercontent.com/Azure-Samples/active-directory-b2c-custom-policy-starterpack/master/{0}/";
        public static string[] PolicyFolderNames =
        {
            "Local",
            "SocialAccounts",
            "SocialAndLocalAccounts",
            "SocialAndLocalAccountsWithMfa"
        };
        public static Dictionary<string,string> SupportedIdPs = new Dictionary<string, string>()
        {
            { "AAD", "Azure AD" },
            { "Facebook", "Facebook" },
            { "MSA", "Microsoft Account" },
            { "Google", "Google"},
            { "LinkedIn", "LinkedIn" },
            { "Twitter", "Twitter" },
            { "SAML", "SAML SSO" },
            //{ "OAuth2", "OAUth2/OIDC" }
        };
    }
}

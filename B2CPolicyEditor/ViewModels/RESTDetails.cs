using B2CPolicyEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using B2CPolicyEditor.Extensions;

namespace B2CPolicyEditor.ViewModels
{
    public class RESTDetails : TechnicalProfileClaims, IViewModel
    {
        public RESTDetails(XElement tp) : base(tp)
        {
            CertName = _tp
                .element("CryptographicKeys")?
                    .elements("Key")
                        .FirstOrDefault(k => k.Attribute("Id").Value == "ClientCertificate")?.Attribute("StorageReferenceId").Value;
            if(CertName == null)
                CertName = "B2C_1A_B2cRestClientCertificate";
            ClientId = _tp
                .element("CryptographicKeys")?
                    .elements("Key")
                        .FirstOrDefault(k => k.Attribute("Id").Value == "BasicAuthenticationUsername")?.Attribute("StorageReferenceId").Value;
            if (ClientId == null)
                ClientId = "B2C_1A_B2cRestClientId";
            Secret = _tp
                .element("CryptographicKeys")?
                    .elements("Key")
                        .FirstOrDefault(k => k.Attribute("Id").Value == "BasicAuthenticationPassword")?.Attribute("StorageReferenceId").Value;
            if (Secret == null)
                Secret = "B2C_1A_B2cRestClientSecret";
        }

        public void Closing()
        {
            var keys =_tp.element("CryptographicKeys");
            if (keys != null) keys.Remove();
            switch (Authentication)
            {
                case "None":
                    _tp.SetMetadataValue("AllowInsecureAuthInProduction", "true");
                    break;
                case "Basic":
                    _tp.element("Metadata").AddAfterSelf(
                        new XElement(Constants.dflt + "CryptographicKeys",
                            new XElement(Constants.dflt + "Key",
                                new XAttribute("Id", "BasicAuthenticationUsername"),
                                new XAttribute("StorageReferenceId", ClientId)),
                            new XElement(Constants.dflt + "Key",
                                new XAttribute("Id", "BasicAuthenticationPassword"),
                                new XAttribute("StorageReferenceId", Secret))));
                    _tp.SetMetadataValue("AllowInsecureAuthInProduction", "false");
                    break;
                case "Certificate":
                    _tp.element("Metadata").AddAfterSelf(
                        new XElement(Constants.dflt + "CryptographicKeys",
                            new XElement(Constants.dflt + "Key",
                                new XAttribute("Id", "ClientCertificate"),
                                new XAttribute("StorageReferenceId", CertName))));
                    _tp.SetMetadataValue("AllowInsecureAuthInProduction", "false");
                    break;
            }
        }

        public string Id
        {
            get { return _tp.Attribute("Id").Value; }
            set { _tp.SetAttributeValue(Constants.dflt + "Id", value); }
        }

        public string Description
        {
            get { return _tp.Element(Constants.dflt + "DisplayName").Value; }
            set { _tp.SetElementValue(Constants.dflt + "DisplayName", value); }
        }
        public string Url
        {
            get { return _tp.GetMetadataValue("ServiceUrl"); }
            set { _tp.SetMetadataValue("ServiceUrl", value); }
        }
        public string Authentication
        {
            get
            {
                var authType = _tp.GetMetadataValue("AuthenticationType");
                SetVisibilities(authType);
                return authType;
            }
            set
            {
                _tp.SetMetadataValue("AuthenticationType", value);
                SetVisibilities(value);
            }
        }
        public string SendMethod
        {
            get { return _tp.GetMetadataValue("SendClaimsIn"); }
            set { _tp.SetMetadataValue("SendClaimsIn", value); }
        }
        public IEnumerable<string> Authentications
        {
            get => new string[] { "None", "Basic", "Certificate" };
        }
        public IEnumerable<string> SendMethods
        {
            get => new string[] { "Body", "QueryString" };
        }
        private bool _IsCertAuthVisible;
        public bool IsCertAuthVisible
        {
            get { return _IsCertAuthVisible; }
            set { Set(ref _IsCertAuthVisible, value); }
        }
        private bool _IsBasicAuthVisible;
        public bool IsBasicAuthVisible
        {
            get { return _IsBasicAuthVisible; }
            set { Set(ref _IsBasicAuthVisible, value); }
        }
        public string CertName { get; set; }
        public string ClientId { get; set; }
        public string Secret { get; set; }

        private void SetVisibilities(string authType)
        {
            switch (authType)
            {
                case "None":
                    IsCertAuthVisible = false;
                    IsBasicAuthVisible = false;
                    break;
                case "Basic":
                    IsCertAuthVisible = false;
                    IsBasicAuthVisible = true;
                    break;
                case "Certificate":
                    IsCertAuthVisible = true;
                    IsBasicAuthVisible = false;
                    break;
            }
        }

    }
}

using B2CPolicyEditor.Models;
using DataToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace B2CPolicyEditor.ViewModels
{
    public class OAuthConfiguration: ObservableObject
    {
        public OAuthConfiguration(XElement tp)
        {
            //TODO: Fix this using XElement.SetElementValue
            
            _technicalProfile = tp;
            if (XDomainName != null)
                DomainName = XDomainName.Value;
            if (XDisplayName != null)
                DisplayName = XDisplayName.Value;
            if (XProviderName != null)
                ProviderName = XProviderName.Value;
            if (XAuthorizationEndpoint != null)
                AuthorizationEndpoint = XAuthorizationEndpoint.Value;
            if (XTokenEndpoint != null)
                TokenEndpoint = XTokenEndpoint.Value;
            if (XClientId != null)
                ClientId = XClientId.Value;
            if (XSecret != null)
                SecretName = XSecret.Attribute("StorageReferenceId").Value;
        }
        XElement _technicalProfile;
        public string DisplayName
        {
            get { return _DisplayName; }
            set
            {
                if (Set(ref _DisplayName, value))
                {
                    if (XDisplayName == null)
                        _technicalProfile.Parent.Parent.Add(new XElement(Constants.dflt + "DisplayName"), value);
                    else
                        XDisplayName.Value = value;
                }
            }
        }
        private string _DisplayName;
        public string DomainName
        {
            get { return _DomainName; }
            set
            {
                if (Set(ref _DomainName, value))
                {
                    if (XDomainName == null)
                        _technicalProfile.Parent.Parent.Add(new XElement(Constants.dflt + "Domain"), value);
                    else
                        XDomainName.Value = value;
                }
            }
        }
        private string _DomainName;
        public string ProviderName
        {
            get { return _ProviderName; }
            set
            {
                if (Set(ref _ProviderName, value))
                {
                    if (XProviderName == null)
                        _technicalProfile.Element(Constants.dflt + "Metadata").Add(new XElement(Constants.dflt + "Item", new XAttribute("Key", "ProviderName"), value));
                    else
                        XProviderName.Value = value;
                }
            }
        }
        private string _ProviderName;
        public string AuthorizationEndpoint
        {
            get { return _AuthorizationEndpoint; }
            set
            {
                if (Set(ref _AuthorizationEndpoint, value))
                {
                    if (XAuthorizationEndpoint == null)
                        _technicalProfile.Element(Constants.dflt + "Metadata").Add(new XElement(Constants.dflt + "Item", new XAttribute("Key", "authorization_endpoint"), value));
                    else
                        XAuthorizationEndpoint.Value = value;
                }
            }
        }
        private string _AuthorizationEndpoint;
        public string TokenEndpoint
        {
            get { return _TokenEndpoint; }
            set
            {
                if (Set(ref _TokenEndpoint, value))
                {
                    if (XTokenEndpoint == null)
                        _technicalProfile.Element(Constants.dflt + "Metadata").Add(new XElement(Constants.dflt + "Item", new XAttribute("Key", "AccessTokenEndpoint"), value));
                    else
                        XTokenEndpoint.Value = value;
                }
            }
        }
        private string _TokenEndpoint;
        public string ClientId
        {
            get
            {
                return _ClientId;
            }
            set
            {
                if (Set(ref _ClientId, value))
                {
                    if (XClientId == null)
                        _technicalProfile.Element(Constants.dflt + "Metadata").Add(new XElement(Constants.dflt + "Item", new XAttribute("Key", "client_id"), value));
                    else
                        XClientId.Value = value;
                }
            }
        }
        private string _ClientId;

        public string SecretName
        {
            get
            {
                return _SecretName;
            }
            set
            {
                if (Set(ref _SecretName, value))
                {
                    var el = XSecret;
                    if (el == null)
                        _technicalProfile.Element(Constants.dflt + "CryptographicKeys").Add(
                            new XElement(Constants.dflt + "Key", 
                                new XAttribute("Id", "client_secret"), 
                                new XAttribute("StorageReferenceId", value)));
                    else
                        el.Attribute("StorageReferenceId").Value = value;
                }
            }
        }
        private string _SecretName;
        XElement XDomainName
        {
            get
            {
                return _technicalProfile.Parent.Parent.Element(Constants.dflt + "Domain");
            }
        }
        XElement XDisplayName
        {
            get
            {
                return _technicalProfile.Parent.Parent.Element(Constants.dflt + "DisplayName");
            }
        }
        XElement XProviderName
        {
            get
            {
                return _technicalProfile.Element(Constants.dflt + "Metadata").Elements(Constants.dflt + "Item").FirstOrDefault(el => el.Attribute("Key").Value == "ProviderName");
            }
        }
        XElement XAuthorizationEndpoint
        {
            get
            {
                return _technicalProfile.Element(Constants.dflt + "Metadata").Elements(Constants.dflt + "Item").FirstOrDefault(el => el.Attribute("Key").Value == "authorization_endpoint");
            }
        }
        XElement XTokenEndpoint
        {
            get
            {
                return _technicalProfile.Element(Constants.dflt + "Metadata").Elements(Constants.dflt + "Item").FirstOrDefault(el => el.Attribute("Key").Value == "AccessTokenEndpoint");
            }
        }
        XElement XClientId
        {
            get
            {
                return _technicalProfile.Element(Constants.dflt + "Metadata").Elements(Constants.dflt + "Item").FirstOrDefault(el => el.Attribute("Key").Value == "client_id");
            }
        }
        private XElement XSecret
        {
            get
            {
                return _technicalProfile.Element(Constants.dflt + "CryptographicKeys").Elements(Constants.dflt + "Key").FirstOrDefault(c => c.Attribute("Id").Value == "client_secret");
            }
        }
    }
}

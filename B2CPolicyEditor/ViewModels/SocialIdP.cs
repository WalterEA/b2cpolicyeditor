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
    public class SocialIdP : ObservableObject
    {
        public SocialIdP(XElement tp)
        {
            _technicalProfile = tp;
            DisplayName = _technicalProfile.Element(Constants.dflt + "DisplayName").Value;
            if (XClientId != null)
                ClientId = XClientId.Value;
            if (XSecret != null)
                SecretName = XSecret.Attribute("StorageReferenceId").Value;
            Delete = new DelegateCommand(() =>
            {
                MainWindow.Current.DeleteItem.Execute(null);
            });
        }
        XElement _technicalProfile;
        public string DisplayName { get; private set; }

        public string HelpUrl
        { 
            get
            {
                try
                {
                    return (new Dictionary<string, string>()
                    {
                        {"Facebook", "https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-get-started-custom#next-steps" },
                        {"Google", "https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-custom-setup-goog-idp" },
                        {"Microsoft Account", "https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-custom-setup-msa-idp" },
                        {"LinkedIn", "https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-custom-setup-li-idp" },
                        {"Twitter", "https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-custom-setup-twitter-idp" },

                    })[DisplayName];
                } catch { return null; }
            }
        }

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
                    App.PolicySet.IsDirty = true;
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
                    App.PolicySet.IsDirty = true;
                }
            }
        }
        private string _SecretName;
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
        public ICommand Delete { get; private set; }
    }
}

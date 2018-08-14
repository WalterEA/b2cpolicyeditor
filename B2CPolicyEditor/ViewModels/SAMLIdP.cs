using B2CPolicyEditor.Models;
using DataToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace B2CPolicyEditor.ViewModels
{
    public class SAMLIdP: ObservableObject
    {
        private XElement _cp;
        private XElement _tp;
        private IEnumerable<XElement> _meta;

        public SAMLIdP(XElement tp)
        {
            _tp = tp;
            _cp = tp.Parent.Parent;
            _meta = _tp.Element(Constants.dflt + "Metadata").Elements();
        }
        public string DomainName
        {
            get { return _cp.Element(Constants.dflt + "Domain")?.Value; }
            set { _cp.Element(Constants.dflt + "Domain").Value = value; }
        }
        public string DisplayName
        {
            get { return _tp.Element(Constants.dflt + "DisplayName")?.Value; }
            set
            {
                _cp.Element(Constants.dflt + "DisplayName").Value = value;
                _tp.Element(Constants.dflt + "DisplayName").Value = value;
            }
        }
        public string Id
        {
            get { return _tp.Attribute("Id").Value; }
            set { _tp.Attribute("Id").Value = value; }
        }
        public string Description
        {
            get { return _tp.Element(Constants.dflt + "Description").Value; }
            set { _tp.SetElementValue(Constants.dflt + "Description", value); }
        }
        public string MetadataUrl
        {
            get { return _meta.First(i => i.Attribute("Key").Value == "PartnerEntity").Value; }
            set { _meta.First(i => i.Attribute("Key").Value == "PartnerEntity").Value = value; }
        }
        public bool RequestsSigned
        {
            get { return bool.Parse(_meta.First(i => i.Attribute("Key").Value == "RequestsSigned").Value); }
            set { _meta.First(i => i.Attribute("Key").Value == "RequestsSigned").Value = value.ToString(); }
        }
        public bool WantsEncryptedAssertions
        {
            get { return bool.Parse(_meta.First(i => i.Attribute("Key").Value == "WantsEncryptedAssertions").Value); }
            set { _meta.First(i => i.Attribute("Key").Value == "WantsEncryptedAssertions").Value = value.ToString(); }
        }
        public bool WantsSignedAssertions
        {
            get { return bool.Parse(_meta.First(i => i.Attribute("Key").Value == "WantsSignedAssertions").Value); }
            set { _meta.First(i => i.Attribute("Key").Value == "WantsSignedAssertions").Value = value.ToString(); }
        }
        public string SamlMessageSigning
        {
            get { return _tp.Element(Constants.dflt + "CryptographicKeys").Elements().First(i => i.Attribute("Id").Value == "SamlMessageSigning").Attribute("StorageReferenceId").Value; }
            set { _tp.Element(Constants.dflt + "CryptographicKeys").Elements().First(i => i.Attribute("Id").Value == "SamlMessageSigning").Attribute("StorageReferenceId").Value = value; }
        }
        public string SamlAssertionSigning
        {
            get { return _tp.Element(Constants.dflt + "CryptographicKeys").Elements().First(i => i.Attribute("Id").Value == "SamlAssertionSigning").Attribute("StorageReferenceId").Value; }
            set { _tp.Element(Constants.dflt + "CryptographicKeys").Elements().First(i => i.Attribute("Id").Value == "SamlAssertionSigning").Attribute("StorageReferenceId").Value = value; }
        }
    }
}

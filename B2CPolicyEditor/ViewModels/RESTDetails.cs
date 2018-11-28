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
    public class RESTDetails : TechnicalProfileClaims
    {
        public RESTDetails(XElement tp) : base(tp)
        {
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
            get { return _tp.GetMetadataValue("AuthenticationType"); }
            set { _tp.SetMetadataValue("AuthenticationType", value); }
        }
        public string SendMethod
        {
            get { return _tp.GetMetadataValue("SendClaimsIn"); }
            set { _tp.SetMetadataValue("SendClaimsIn", value); }
        }
        public IEnumerable<string> Authentications
        {
            get => new string[] { "Basic", "Advanced", "???" };
        }
        public IEnumerable<string> SendMethods
        {
            get => new string[] { "Body", "QueryString" };
        }
    }
}

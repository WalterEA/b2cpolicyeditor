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
    public class ClaimUsage: ObservableObject
    {
        public ClaimUsage()
        {
        }
        public ClaimUsage(XElement source)
        {
            _source = source;
        }
        XElement _source;
        public XElement Source { get => _source; set => _source = value; }
        public string Id
        {
            get { return _source.Attribute("ClaimTypeReferenceId")?.Value; }
            set { _source.SetAttributeValue("ClaimTypeReferenceId", value); }
        }
        public string PartnerClaimType
        {
            get { return _source.Attribute("PartnerClaimType")?.Value; }
            set { _source.SetAttributeValue("PartnerClaimType", value); }
        }
        public bool? IsRequired
        {
            get { return _source.Attribute("Required")?.Value == "true"; }
            set { _source.SetAttributeValue("Required", value); }
        }
        public bool IsSubject // applies to RPs only
        {
            get { return _IsSubject; }
            set
            {
                Set(ref _IsSubject, value);
            }
        }
        private bool _IsSubject;
        public string ExternalName
        {
            get => String.IsNullOrEmpty(PartnerClaimType) ? Id : PartnerClaimType;
        }

        public string DefaultValue
        {
            get => _source.Attribute("DefaultValue")?.Value;
            set => _source.SetAttributeValue("DefaultValue", value);
        }

        public static IEnumerable<string> AllClaims
        {
            get => App.PolicySet.Base.Root
                .Element(Constants.dflt + "BuildingBlocks")
                    .Element(Constants.dflt + "ClaimsSchema")
                        .Elements(Constants.dflt + "ClaimType")
                            .Select(c => c.Attribute("Id")?.Value);
        }
    }
}

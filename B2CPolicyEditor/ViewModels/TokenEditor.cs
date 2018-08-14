using B2CPolicyEditor.Models;
using DataToolkit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using B2CPolicyEditor.Extensions;

namespace B2CPolicyEditor.ViewModels
{
    public class TokenEditor: ObservableObject
    {
        public TokenEditor(XDocument journey)
        {
            _journeyDoc = journey;
            _tp = journey.Root.Element(Constants.dflt + "RelyingParty").Element(Constants.dflt + "TechnicalProfile");
            ProtocolName = _tp.Element(Constants.dflt + "Protocol").Attribute("Name").Value;
            Claims = new ObservableCollection<ClaimUsage>();
            _tp.BuildClaimCollection("OutputClaims", "OutputClaim", Claims);
            var subjectId = _tp.Element(Constants.dflt + "SubjectNamingInfo").Attribute("ClaimType").Value;
            foreach (var c in Claims)
            {
                if (c.ExternalName == subjectId)
                {
                    Subject = c;
                }
                c.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == "IsSubject")
                    {
                        if (_inChangeTransaction) // handle the change once if it applies to this property, subject is exclusive
                        {
                            _inChangeTransaction = false;
                            return; // ignore changes we cause below
                        }
                        _inChangeTransaction = true;
                        if (c.IsSubject) // new subject
                        {
                            Subject.IsSubject = false; // old subject
                            Subject = c; // new subject
                        }
                    }
                };
            }

        }
        XDocument _journeyDoc;
        XElement _tp;
        bool _inChangeTransaction = false;
        public string ProtocolName
        {
            get { return _ProtocolName; }
            set
            {
                Set(ref _ProtocolName, value);
            }
        }
        private string _ProtocolName;
        public ClaimUsage Subject
        {
            get { return _Subject; }
            set
            {
                if (Set(ref _Subject, value))
                {
                    _Subject.IsSubject = true;
                    _tp.Element(Constants.dflt + "SubjectNamingInfo").SetAttributeValue("ClaimType", _Subject.ExternalName);
                }
            }
        }
        private ClaimUsage _Subject;

        public ObservableCollection<ClaimUsage> Claims { get; private set; }
    }
}

using B2CPolicyEditor.Models;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using B2CPolicyEditor.Extensions;
using DataToolkit;

namespace B2CPolicyEditor.ViewModels
{
    public class TechnicalProfileClaims: ObservableObject
    {
        public TechnicalProfileClaims(XElement tp)
        {
            _tp = tp;
            if (_tp != null)
            {
                InputClaims = new ObservableCollection<ClaimUsage>();
                _tp.BuildClaimCollection("InputClaims", "InputClaim", InputClaims);

                OutputClaims = new ObservableCollection<ClaimUsage>();
                _tp.BuildClaimCollection("OutputClaims", "OutputClaim", OutputClaims);

                if (_tp.Element(Constants.dflt + "PersistedClaims") != null)
                {
                    PersistedClaims = new ObservableCollection<ClaimUsage>();
                    _tp.BuildClaimCollection("PersistedClaims", "PersistedClaim", PersistedClaims);
                }
            }
        }
        protected XElement _tp;
        public ObservableCollection<ClaimUsage> InputClaims { get; set; }
        public ObservableCollection<ClaimUsage> OutputClaims { get; set; }
        public ObservableCollection<ClaimUsage> PersistedClaims { get; set; }

        public bool IsPersistedVisible
        {
            get { return PersistedClaims?.Count > 0; }
        }
    }
}

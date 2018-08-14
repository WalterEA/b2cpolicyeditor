using B2CPolicyEditor.Models;
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
    public class TechnicalProfileClaims
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
        XElement _tp;
        public ObservableCollection<ClaimUsage> InputClaims { get; set; }
        public ObservableCollection<ClaimUsage> OutputClaims { get; set; }
        public ObservableCollection<ClaimUsage> PersistedClaims { get; set; }

        public bool IsPersistedVisible
        {
            get { return PersistedClaims?.Count > 0; }
        }
        //private static void BuildClaimCollection(string collectionElementName, string claimTypeName, ObservableCollection<ClaimUsage> collection)
        //{
        //    if (_tp.Element(Constants.dflt + collectionElementName) != null)
        //    {
        //        foreach (var c in _tp.Element(Constants.dflt + collectionElementName).Elements())
        //            collection.Add(new ClaimUsage(c));
        //        collection.CollectionChanged += (s, e) =>
        //        {
        //            switch (e.Action)
        //            {
        //                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
        //                    var claim = (ClaimUsage)e.NewItems[0];
        //                    var source = new XElement(Constants.dflt + claimTypeName);
        //                    _tp.Element(Constants.dflt + collectionElementName).Add(source);
        //                    claim.Source = source;
        //                    break;
        //                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
        //                    claim = (ClaimUsage)e.OldItems[0];
        //                    claim.Source.Remove();
        //                    break;
        //            }
        //        };
        //    }
        //}
    }
}

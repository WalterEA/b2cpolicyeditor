using B2CPolicyEditor.Extensions;
using B2CPolicyEditor.Models;
using DataToolkit;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;

namespace B2CPolicyEditor.ViewModels
{
    public class JourneyStep : ObservableObject
    {
        public JourneyStep(JourneyEditor parent, XElement step)
        {
            _parent = parent;
            _source = step;
            Delete = new DelegateCommand(() =>
            {
                _parent.Steps.Remove(this);
            });
        }
        JourneyEditor _parent;
        XElement _source;
        public XElement Source { get => _source; set => _source = value; }
        public int StepNo
        {
            get { return int.Parse(_source.Attribute("Order").Value); }
            set
            {
                _source.SetAttributeValue("Order", value);
                OnPropertyChanged("StepNo");
            }
        }

        public string StepType
        {
            get { return _source.Attribute("Type").Value; }
            set { _source.SetAttributeValue("Type", value); }
        }
        public string UIDef
        {
            get { return _source.Attribute("ContentDefinitionReferenceId")?.Value; }
            set { _source.SetAttributeValue("ContentDefinitionReferenceId", value); }
        }
        public IEnumerable<Precondition> Preconditions
        {
            get => _source.Element(Constants.dflt + "Preconditions")?.Elements().Select(p => new Precondition(p));
        }
        public bool? IsConditionVisible
        {
            get { return _source.Element(Constants.dflt + "Preconditions") != null; }
        }
        public IEnumerable<TechnicalProfile> Providers
        {
            get
            {
                //var providers = _source
                //    .Elements(Constants.dflt + "ClaimsProviderSelections")
                //        .Elements(Constants.dflt + "ClaimsProviderSelection")
                //            .Where(p => p.Attribute("TargetClaimsExchangeId") != null)
                //                .Select(p => new Exchange(p) { Name = p.Attribute("TargetClaimsExchangeId").Value });
                return _source
                    .Elements(Constants.dflt + "ClaimsExchanges")
                        .Elements(Constants.dflt + "ClaimsExchange")
                            .Select(e => new TechnicalProfile(PolicyDocExtensions.GetTechnicalProfile(e.Attribute("TechnicalProfileReferenceId").Value)));
                //return providers.Concat(exchanges);
            }
        }
        public ICommand Delete { get; private set; }
    }
}

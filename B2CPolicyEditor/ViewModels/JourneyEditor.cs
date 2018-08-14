using B2CPolicyEditor.Models;
using DataToolkit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace B2CPolicyEditor.ViewModels
{
    public class JourneyEditor : ObservableObject
    {

        public JourneyEditor(XElement journey)
        {
            _journey = journey;
            Steps = new ObservableCollection<JourneyStep>();
            foreach (var step in _journey.Element(Constants.dflt + "OrchestrationSteps").Elements())
            {
                Steps.Add(new JourneyStep(this, step));
            }
            Steps.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        var step = (JourneyStep)e.OldItems[0];
                        var stepNo = e.OldStartingIndex;
                        for (var i = stepNo; i < Steps.Count; i++)
                            Steps[i].StepNo = i + 1;
                        step.Source.Remove();
                        break;
                    default:
                        break;
                }
            };
            var sendStep = Steps.Last();
            if (sendStep.StepType != "SendClaims") throw new ApplicationException("Last journey step must be SendClaims.");
            var rps = App.PolicySet
                .Journeys.Where(j =>
                    j.Root.Element(Constants.dflt + "RelyingParty")
                        .Element(Constants.dflt + "DefaultJourney").Attribute("ReferenceId").Value == journey.Attribute("Id").Value);
        }
        XElement _journey;
        public ObservableCollection<JourneyStep> Steps { get; private set; }
        public JourneyStep SelectedStep
        {
            get { return _SelectedStep; }
            set
            {
                if (Set(ref _SelectedStep, value))
                    OnPropertyChanged("Providers");
            }
        }
        private JourneyStep _SelectedStep;

        public IEnumerable<Provider> Providers
        {
            get
            {
                if (SelectedStep != null)
                    return SelectedStep.Source.Element(Constants.dflt + "ClaimsExchanges")?.Elements().Select(e => new Provider(e));
                return null;
            }
        }
        public Provider SelectedProvider
        {
            get { return _SelectedProvider; }
            set
            {
                if (Set(ref _SelectedProvider, value))
                {
                    if (_SelectedProvider == null)
                        ClaimsUsage = null;
                    else
                    {
                        var tp = App.PolicySet.Base.Root
                            .Element(Constants.dflt + "ClaimsProviders")
                                .Elements(Constants.dflt + "ClaimsProvider")
                                    .Elements(Constants.dflt + "TechnicalProfiles")
                                        .Elements(Constants.dflt + "TechnicalProfile").First(p => p.Attribute("Id").Value == _SelectedProvider.Id);
                        ClaimsUsage = new TechnicalProfileClaims(tp);
                    }
                }
            }
        }
        private Provider _SelectedProvider;

        public TechnicalProfileClaims ClaimsUsage
        {
            get { return _ClaimsUsage; }
            set { Set(ref _ClaimsUsage, value); }
        }
        private TechnicalProfileClaims _ClaimsUsage;
        private Exchange _SelectedExchange;
        public Exchange SelectedExchange
        {
            get { return _SelectedExchange; }
            set
            {
                _SelectedExchange = value;
                var tp = App.PolicySet.Base.Root
                    .Element(Constants.dflt + "ClaimsProviders")
                        .Elements(Constants.dflt + "ClaimsProvider")
                            .Elements(Constants.dflt + "TechnicalProfiles")
                                .Elements(Constants.dflt + "TechnicalProfile").FirstOrDefault(p => p.Attribute("Id").Value == _SelectedExchange.Name);
                ClaimsUsage = new TechnicalProfileClaims(tp);
                throw new ApplicationException("Ignore this exception. HAck till I figure out how to keep ietms unselected in the listbox.");
        }
    }

    public class JourneyStep: ObservableObject
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
        public IEnumerable<Exchange> Exchanges
        {
            get
            {
                var providers = _source
                    .Elements(Constants.dflt + "ClaimsProviderSelections")
                        .Elements(Constants.dflt + "ClaimsProviderSelection")
                            .Where(p => p.Attribute("TargetClaimsExchangeId") != null)
                                .Select(p => new Exchange(p) { Name = p.Attribute("TargetClaimsExchangeId").Value });
                var exchanges = _source
                    .Elements(Constants.dflt + "ClaimsExchanges")
                        .Elements(Constants.dflt + "ClaimsExchange")
                            .Select(e => new Exchange(e) { Name = e.Attribute("TechnicalProfileReferenceId").Value } );
                return providers.Concat(exchanges);
            }
        }
        public ICommand Delete { get; private set; }
    }

    public class Precondition
    {
        public Precondition(XElement cond)
        {
            _cond = cond;
        }
        XElement _cond;

        public string Text
        {
            get
            {
                string text = String.Empty;
                switch (_cond.Attribute("Type").Value)
                {
                    case "ClaimEquals":
                        var values = _cond.Elements(Constants.dflt + "Value").Select(c => c.Value).ToList();
                        var claim = values[0];
                        var val = values[1];
                        var cond = _cond.Attribute("ExecuteActionsIf").Value == "true" ? "equals" : "does not equal";
                        text = $"If {claim} {cond} {val} skip this step";
                        break;
                    case "ClaimsExist":
                        var claims = _cond.Elements(Constants.dflt + "Value")?.Select(c => c.Value).Aggregate("", (n, a) => a += n, r => r);
                        cond = _cond.Attribute("ExecuteActionsIf").Value == "true" ? "exists" : "does not exist";
                        text = $"If {claims} {cond} skip this step";
                        break;
                }
                return text;
            }
        }
    }

    public class Provider
    {
        public Provider(XElement p)
        {
            _provider = p;
        }
        XElement _provider;
        public string Id
        {
            get => _provider.Attribute("TechnicalProfileReferenceId").Value;
        }
    }

    public class Exchange
    {
        public Exchange(XElement source)
        {
            Source = source;
        }
        public string Name { get; set; }
        public XElement Source { get; }
    }
}
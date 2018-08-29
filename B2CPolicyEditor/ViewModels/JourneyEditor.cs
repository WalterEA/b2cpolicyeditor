using B2CPolicyEditor.Extensions;
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
            SelectedArtifact = (Steps.Count > 0)? Steps[0]: null; // otherwise Selection does not databing
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
            //var sendStep = Steps.Last();
            //if (sendStep.StepType != "SendClaims") throw new ApplicationException("Last journey step must be SendClaims.");
            //var rps = App.PolicySet
            //    .Journeys.Where(j =>
            //        j.Root.Element(Constants.dflt + "RelyingParty")
            //            .Element(Constants.dflt + "DefaultJourney").Attribute("ReferenceId").Value == journey.Attribute("Id").Value);
        }
        XElement _journey;

        public string Id
        {
            get => _journey.Attribute("Id").Value;
            set => _journey.SetAttributeValue("Id", value);
        }

        public ObservableCollection<JourneyStep> Steps { get; private set; }
        public object SelectedArtifact
        {
            get { return _SelectedArtifact; }
            set
            {
                if (Set(ref _SelectedArtifact, value))
                {
                    if (_SelectedArtifact is JourneyStep)
                        ClaimsUsage = null;
                    else
                    {
                        ClaimsUsage = new TechnicalProfileClaims(((TechnicalProfile)value).Source);
                    }
                }
            }
        }
        private object _SelectedArtifact;


        public TechnicalProfileClaims ClaimsUsage
        {
            get { return _ClaimsUsage; }
            set { Set(ref _ClaimsUsage, value); }
        }
        private TechnicalProfileClaims _ClaimsUsage;
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

    public class TechnicalProfile
    {
        public TechnicalProfile(XElement tp)
        {
            if (tp == null) throw new ArgumentException("tp cannot be null");
            Source = tp;

            Providers = new List<TechnicalProfile>();
            var validations = Source.Element(Constants.dflt + "ValidationTechnicalProfiles");
            if (validations != null)
            {
                foreach (var validationTp in validations.Elements(Constants.dflt + "ValidationTechnicalProfile"))
                {
                    VerifyAndAddIncludedTp(validationTp);
                }
            }
            var refTp = tp.Element(Constants.dflt + "IncludeTechnicalProfile");
            VerifyAndAddIncludedTp(refTp);
        }
        private void VerifyAndAddIncludedTp(XElement el)
        {
            if (el == null) return;
            var refTp = PolicyDocExtensions.GetTechnicalProfile(el.Attribute("ReferenceId").Value);
            if ((refTp.Element(Constants.dflt + "InputClaims") != null) ||
                (refTp.Element(Constants.dflt + "OutputClaims") != null) ||
                (refTp.Element(Constants.dflt + "PersistedClaims") != null))
                Providers.Add(new TechnicalProfile(refTp));
        }
        public string Name { get => Source.Attribute("Id").Value; }
        public XElement Source { get; private set; }
        public List<TechnicalProfile> Providers { get; private set; }
    }
}
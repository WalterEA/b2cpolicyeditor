using B2CPolicyEditor.Models;
using DataToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using B2CPolicyEditor.Extensions;
using System.Text.RegularExpressions;

namespace B2CPolicyEditor.ViewModels
{
    public class AddIdPWizard: ObservableObject
    {
        public AddIdPWizard()
        {
            IsApplied = false;
            var journeys = new List<JourneyRef>();
            foreach (var j in App.PolicySet.Base.Root.Element(Constants.dflt + "UserJourneys").Elements(Constants.dflt + "UserJourney")
                .Where(jr => jr.Element(Constants.dflt + "OrchestrationSteps").Elements(Constants.dflt + "OrchestrationStep").First().Element(Constants.dflt + "ClaimsProviderSelections") != null))
            {
                journeys.Add(new JourneyRef()
                {
                    Name = j.Attribute("Id").Value,
                    IsSelected = true,
                    Source = j,
                });
            }
            Journeys = journeys;
            Apply = new DelegateCommand(() =>
            {
                // TODO: add a wizard step to first configure the IdP (if it is AAD)
                try
                {
                    CreatedIdP = CreateIdP();
                    App.PolicySet.Base.Root.Element(Constants.dflt + "ClaimsProviders").Add(CreatedIdP);
                    foreach (var j in Journeys.Where(jr => jr.IsSelected))
                    {
                        j.Source.AddIdPToJourney(CreatedIdP);
                    };
                    IsApplied = true;
                    if (Closing != null)
                        Closing();
                } catch(Exception ex)
                {
                    MainWindow.Trace.Add(new TraceItem { Msg = ex.Message });
                }
            })
            { Enabled = false } ;
            SelectedIdPType = IdPTypes.First();
        }
        public XElement CreatedIdP { get; private set; }
        public Dictionary<string, string> IdPTypes
        {
            get => Constants.SupportedIdPs;
        }
        public string Id
        {
            get { return _Id; }
            set
            {
                if (Set(ref _Id, value))
                {
                    ((DelegateCommand)Apply).Enabled = Regex.Match(value, @"^[A-Za-z][\w]").Success;
                }
            }
        }
        private string _Id;


        public IEnumerable<JourneyRef> Journeys { get; private set; }
        public KeyValuePair<string, string> SelectedIdPType
        {
            get { return _SelectedIdPType; }
            set
            {
                if (Set(ref _SelectedIdPType, value))
                {
                    Id = $"{_SelectedIdPType.Key}Provider";
                }
            }
        }
        private KeyValuePair<string,string> _SelectedIdPType;
        public ICommand Apply { get; private set; }
        private XElement CreateIdP()
        {
            XElement idp;
            switch(_SelectedIdPType.Key)
            {
                case "AAD":
                case "SAML":
                    idp = PolicyDocExtensions.CreateIdP(_SelectedIdPType.Key, Id);
                    break;
                case "Facebook":
                case "MSA":
                case "Google":
                case "LinkedIn":
                case "Twitter":
                    idp = PolicyDocExtensions.CreateIdP(_SelectedIdPType.Key);
                    break;
                default:
                    throw new NotSupportedException("Have not had time to add this one");
            }
            return idp;
        }
        public bool IsApplied { get; set; }
        public event Action Closing;
    }
    public class JourneyRef
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public XElement Source { get; set; }
    }
}